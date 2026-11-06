namespace AutoHub.Application.Features.Leads.CreateLead;

public record CreateLeadCommand(
    Guid VehicleId,
    string Name,
    string Phone,
    string Email,
    string? Message,
    string? Website,
    string? IdempotencyKey,
    bool ConsentAccepted,
    string? IpHash,
    string? UserAgent
) : IRequest<LeadCreationResult>;

public record LeadCreationResult(Guid Id, bool Created);
