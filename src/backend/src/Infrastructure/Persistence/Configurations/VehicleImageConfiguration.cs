namespace AutoHub.Infrastructure.Persistence.Configurations;

public class VehicleImageConfiguration : IEntityTypeConfiguration<VehicleImage>
{
    public void Configure(EntityTypeBuilder<VehicleImage> builder)
    {
        builder.ToTable("VehicleImages");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.ImageUrl)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(i => i.AltText)
            .HasMaxLength(200);

        builder.Property(i => i.DisplayOrder)
            .IsRequired();

        builder.Property(i => i.CreatedAt)
            .IsRequired();

        builder.HasOne(i => i.Vehicle)
            .WithMany(v => v.Images)
            .HasForeignKey(i => i.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
