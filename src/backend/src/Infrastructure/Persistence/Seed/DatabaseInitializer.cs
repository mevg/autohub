using AutoHub.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace AutoHub.Infrastructure.Persistence.Seed;

/// <summary>
/// Aplica migraciones pendientes y crea el usuario admin inicial si no existe.
/// Las credenciales llegan por configuración (SeedAdmin:Email / SeedAdmin:Password),
/// nunca en código. Si no están configuradas, no se crea nada.
/// </summary>
public static class DatabaseInitializer
{
    public static async Task MigrateAndSeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;

        var db = provider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        var logger = provider.GetRequiredService<ILogger<AppDbContext>>();

        if (!await db.Branding.AnyAsync())
        {
            db.Branding.Add(new Branding());
            await db.SaveChangesAsync();
            logger.LogInformation("Branding inicial creado con valores por defecto.");
        }
        var email = configuration["SeedAdmin:Email"]?.Trim().ToLowerInvariant();
        var password = configuration["SeedAdmin:Password"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("SeedAdmin no configurado (SeedAdmin:Email / SeedAdmin:Password). Se omite la creación del admin inicial.");
            return;
        }

        if (await db.Users.AnyAsync(u => u.Email == email))
        {
            return;
        }

        var hasher = provider.GetRequiredService<IPasswordHasher>();
        db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = hasher.HashPassword(password),
            FullName = "Administrator",
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        logger.LogInformation("Usuario admin inicial creado: {Email}", email);
    }
}
