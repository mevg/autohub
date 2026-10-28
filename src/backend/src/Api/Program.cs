using AutoHub.Api.Endpoints;
using AutoHub.Infrastructure.Persistence.Seed;
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

    app.UseCors("Frontend");

    app.UseStaticFiles();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapAuthEndpoints();
    app.MapSettingsEndpoints();
    app.MapVehicleEndpoints();

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
