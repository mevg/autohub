namespace AutoHub.Infrastructure.Persistence.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("Vehicles");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Vin)
            .HasMaxLength(17)
            .IsRequired();

        builder.HasIndex(v => v.Vin)
            .IsUnique();

        builder.Property(v => v.Make)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(v => v.Model)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(v => v.Price)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(v => v.Transmission)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(v => v.FuelType)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(v => v.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.HasOne(v => v.Creator)
            .WithMany(u => u.Vehicles)
            .HasForeignKey(v => v.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}