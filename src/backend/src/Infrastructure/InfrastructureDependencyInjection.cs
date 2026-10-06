namespace AutoHub.Infrastructure;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

        services.AddSingleton<ISqlConnectionFactory>(new NpgsqlConnectionFactory(connectionString));
        services.AddScoped<IUserQueries, DapperUserQueries>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        var jwtSettings = ReadJwtSettings(configuration);
        services.AddSingleton(jwtSettings);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(jwtOptions =>
            {
                jwtOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });
        services.AddAuthorization();

        return services;
    }

    private static JwtSettings ReadJwtSettings(IConfiguration configuration)
    {
        var section = configuration.GetSection(JwtSettings.SectionName);
        var settings = new JwtSettings
        {
            Secret = section["Secret"] ?? string.Empty,
            Issuer = section["Issuer"] ?? string.Empty,
            Audience = section["Audience"] ?? string.Empty,
            ExpiryHours = int.TryParse(section["ExpiryHours"], out var hours) ? hours : 8
        };

        if (Encoding.UTF8.GetByteCount(settings.Secret) < 32)
        {
            throw new InvalidOperationException(
                $"Config '{JwtSettings.SectionName}:Secret' debe tener al menos 32 bytes. " +
                "Configúralo vía variable de entorno JwtSettings__Secret o user-secrets.");
        }

        if (string.IsNullOrWhiteSpace(settings.Issuer) || string.IsNullOrWhiteSpace(settings.Audience))
        {
            throw new InvalidOperationException(
                $"Config '{JwtSettings.SectionName}:Issuer/Audience' es requerida.");
        }

        if (settings.ExpiryHours <= 0)
        {
            throw new InvalidOperationException(
                $"Config '{JwtSettings.SectionName}:ExpiryHours' debe ser mayor que cero.");
        }

        return settings;
    }
}
