using AutoHub.Application.Common.Exceptions;
using AutoHub.Domain.Enums;

namespace AutoHub.Application.Features.Leads.CreateLead;

public class CreateLeadCommandHandler(ILeadStore store) : IRequestHandler<CreateLeadCommand, LeadCreationResult>
{
    private const int DuplicateWindowMinutes = 10;

    public async Task<LeadCreationResult> Handle(
        CreateLeadCommand request,
        CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        var phone = NormalizePhone(request.Phone);
        var email = request.Email.Trim().ToLowerInvariant();
        var message = string.IsNullOrWhiteSpace(request.Message) ? null : request.Message.Trim();
        var idempotencyKey = string.IsNullOrWhiteSpace(request.IdempotencyKey)
            ? null
            : request.IdempotencyKey.Trim();

        if (idempotencyKey is not null)
        {
            var existing = await store.FindByIdempotencyKeyAsync(idempotencyKey, cancellationToken);
            if (existing is not null)
            {
                EnsureSamePayload(existing, request.VehicleId, name, phone, email, message);
                return new LeadCreationResult(existing.Id, false);
            }
        }

        var recent = await store.FindRecentDuplicateAsync(
            request.VehicleId,
            email,
            phone,
            DateTime.UtcNow.AddMinutes(-DuplicateWindowMinutes),
            cancellationToken);
        if (recent is not null)
        {
            return new LeadCreationResult(recent.Id, false);
        }

        if (!await store.IsVehicleAvailableAsync(request.VehicleId, cancellationToken))
        {
            throw new KeyNotFoundException("El vehículo no está disponible.");
        }

        var lead = new Lead
        {
            Id = Guid.NewGuid(),
            VehicleId = request.VehicleId,
            Name = name,
            Phone = phone,
            Email = email,
            Message = message,
            Status = LeadStatus.New,
            Source = "WebCatalog",
            IdempotencyKey = idempotencyKey,
            ConsentAccepted = request.ConsentAccepted,
            IpHash = request.IpHash,
            UserAgent = Truncate(request.UserAgent, 512),
            CreatedAt = DateTime.UtcNow
        };

        store.Add(lead);
        try
        {
            await store.SaveChangesAsync(cancellationToken);
        }
        catch (DuplicateIdempotencyKeyException)
        {
            var existing = idempotencyKey is null
                ? null
                : await store.FindByIdempotencyKeyAsync(idempotencyKey, cancellationToken);
            if (existing is not null)
            {
                EnsureSamePayload(existing, request.VehicleId, name, phone, email, message);
                return new LeadCreationResult(existing.Id, false);
            }

            throw new IdempotencyConflictException("La solicitud ya fue registrada.");
        }

        return new LeadCreationResult(lead.Id, true);
    }

    private static void EnsureSamePayload(
        Lead existing,
        Guid vehicleId,
        string name,
        string phone,
        string email,
        string? message)
    {
        var same = existing.VehicleId == vehicleId
            && string.Equals(existing.Name, name, StringComparison.Ordinal)
            && string.Equals(existing.Phone, phone, StringComparison.Ordinal)
            && string.Equals(existing.Email, email, StringComparison.Ordinal)
            && string.Equals(existing.Message, message, StringComparison.Ordinal);

        if (!same)
        {
            throw new IdempotencyConflictException(
                "La clave de idempotencia ya fue usada con otros datos.");
        }
    }

    private static string NormalizePhone(string value)
    {
        var digits = new string(value.Where(char.IsDigit).ToArray());
        return digits;
    }

    private static string? Truncate(string? value, int maxLength)
        => string.IsNullOrWhiteSpace(value)
            ? null
            : value.Length <= maxLength ? value : value[..maxLength];
}
