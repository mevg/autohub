using System.Threading.RateLimiting;
using AutoHub.Api.Endpoints;
using AutoHub.Application.Features.PublicCatalog;
using AutoHub.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// wwwroot debe existir antes del build para que WebRootPath se resuelva
// y UseStaticFiles + FileBrandAssetStorage usen la misma raíz.
var wwwroot = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
Directory.CreateDirectory(wwwroot);
builder.Environment.WebRootPath = wwwroot;
builder.Environment.WebRootFileProvider = new PhysicalFileProvider(wwwroot);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/autohub-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

try
{

    // Add services to the container.
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();
    var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? ["http://localhost:4200"];
    builder.Services.AddCors(options => options.AddPolicy("Frontend", policy => policy
        .WithOrigins(corsOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()));

    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        // La API solo debe ser accesible a través del proxy de la infraestructura.
        // Si se expone directo, limitar KnownProxies/KnownNetworks por entorno.
        options.KnownIPNetworks.Clear();
        options.KnownProxies.Clear();
    });

    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.AddPolicy("public-leads", httpContext =>
        {
            var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(10),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            });
        });
    });

    var whatsAppSection = builder.Configuration.GetSection("WhatsApp");
    var whatsAppSettings = new WhatsAppSettings(
        whatsAppSection.GetValue("Enabled", true),
        whatsAppSection["PhoneNumber"] ?? string.Empty,
        whatsAppSection["MessageTemplate"] ?? WhatsAppSettings.DefaultMessageTemplate);
    builder.Services.AddSingleton(whatsAppSettings);

    builder.Services
        .AddApplication()
        .AddInfrastructure(builder.Configuration);
    var app = builder.Build();

    await DatabaseInitializer.MigrateAndSeedAsync(app.Services, app.Configuration);

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }

    // En Development no se redirige http->https: los navegadores descartan el
    // header Authorization al seguir el 307 a otro esquema, lo que convertía
    // cada llamada autenticada en 401 + logout silencioso. El perfil https ya
    // escucha en ambos puertos (7086+5116), así el proxy del front funciona
    // con cualquier perfil. En producción la redirección la hace la infra.
    // Detrás del proxy http del compose no hay TLS que redirigir: se desactiva
    // con HttpsRedirection__Enabled=false (por defecto sigue activa).
    if (!app.Environment.IsDevelopment()
        && app.Configuration.GetValue("HttpsRedirection:Enabled", true))
    {
        app.UseHttpsRedirection();
    }

    app.UseForwardedHeaders();
    app.UseCors("Frontend");

    app.UseStaticFiles(new StaticFileOptions
    {
        OnPrepareResponse = context =>
        {
            // Los nombres de archivo incluyen GUID: son inmutables.
            context.Context.Response.Headers.CacheControl = "public,max-age=604800,immutable";
        }
    });

    app.UseAuthentication();
    app.UseAuthorization();
    app.UseRateLimiter();

    app.MapAuthEndpoints();
    app.MapSettingsEndpoints();
    app.MapVehicleEndpoints();
    app.MapPublicCatalogEndpoints(whatsAppSettings);
    app.MapPublicLeadEndpoints(builder.Configuration["Leads:IpHashSalt"]);

    app.Run();


}
catch (Exception ex)
{
    Log.Fatal(ex, "Something went wrong while starting the application");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
