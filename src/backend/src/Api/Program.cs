using AutoHub.Api.Endpoints;
using AutoHub.Infrastructure.Persistence.Seed;
using Microsoft.Extensions.FileProviders;

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
    builder.Services
        .AddApplication()
        .AddInfrastructure(builder.Configuration);
    var app = builder.Build();

    await DatabaseInitializer.MigrateAndSeedAsync(app.Services, app.Configuration);

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseHttpsRedirection();

    app.UseStaticFiles();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapAuthEndpoints();
    app.MapSettingsEndpoints();

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
