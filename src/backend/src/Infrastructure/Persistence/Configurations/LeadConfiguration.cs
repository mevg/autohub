namespace AutoHub.Infrastructure.Persistence.Configurations;

public class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        builder.ToTable("Leads");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(l => l.Phone)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(l => l.Email)
            .HasMaxLength(254)
            .IsRequired();

        builder.Property(l => l.Message)
            .HasMaxLength(1000);

        builder.Property(l => l.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(l => l.Source)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(l => l.IdempotencyKey)
            .HasMaxLength(64);

        builder.Property(l => l.IpHash)
            .HasMaxLength(64);

        builder.Property(l => l.UserAgent)
            .HasMaxLength(512);

        builder.Property(l => l.Notes)
            .HasMaxLength(1000);

        builder.Property(l => l.CreatedAt)
            .IsRequired();

        builder.HasIndex(l => l.IdempotencyKey)
            .IsUnique()
            .HasFilter("\"IdempotencyKey\" IS NOT NULL");

        builder.HasIndex(l => new { l.VehicleId, l.CreatedAt });
        builder.HasIndex(l => l.Email);
        builder.HasIndex(l => new { l.Status, l.CreatedAt });

        builder.HasOne(l => l.Vehicle)
            .WithMany(v => v.Leads)
            .HasForeignKey(l => l.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
