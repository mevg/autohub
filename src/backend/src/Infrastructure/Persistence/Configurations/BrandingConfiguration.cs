namespace AutoHub.Infrastructure.Persistence.Configurations;

public class BrandingConfiguration : IEntityTypeConfiguration<Branding>
{
    public void Configure(EntityTypeBuilder<Branding> builder)
    {
        builder.ToTable("branding");

        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(b => b.AppName)
            .HasColumnName("app_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(b => b.LogoUrl)
            .HasColumnName("logo_url")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(b => b.FaviconUrl)
            .HasColumnName("favicon_url")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(b => b.BannerUrls)
            .HasColumnName("banner_urls")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(b => b.PrimaryLight)
            .HasColumnName("primary_light")
            .HasMaxLength(7)
            .IsRequired();

        builder.Property(b => b.Primary)
            .HasColumnName("primary")
            .HasMaxLength(7)
            .IsRequired();

        builder.Property(b => b.PrimaryDark)
            .HasColumnName("primary_dark")
            .HasMaxLength(7)
            .IsRequired();

        builder.Property(b => b.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();
    }
}
