namespace AutoHub.Domain.Entities;

public class Lead
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Message { get; set; }
    public LeadStatus Status { get; set; } = LeadStatus.New;
    public string Source { get; set; } = "WebCatalog";
    public string? IdempotencyKey { get; set; }
    public bool ConsentAccepted { get; set; }
    public string? IpHash { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ContactedAt { get; set; }
    public string? Notes { get; set; }

    public Vehicle Vehicle { get; set; } = null!;
}
