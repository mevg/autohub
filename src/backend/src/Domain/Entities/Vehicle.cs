namespace AutoHub.Domain.Entities;

public class Vehicle
{
    public Guid Id { get; set; }
    public string Vin { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal Price { get; set; }
    public int Mileage { get; set; }
    public string Transmission { get; set; } = string.Empty;
    public string FuelType { get; set; } = string.Empty;
    public VehicleStatus Status { get; set; } = VehicleStatus.Available;
    public string? Description { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Relaciones
    public User Creator { get; set; } = null!;
    public ICollection<VehicleImage> Images { get; set; } = new List<VehicleImage>();
    // public ICollection<SocialPost> SocialPosts { get; set; } = new List<SocialPost>();
    // public ICollection<Lead> Leads { get; set; } = new List<Lead>();
}
