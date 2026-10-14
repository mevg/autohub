using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace AutoHub.IntegrationTests.Vehicles;

public sealed class VehicleApiTests : IAsyncLifetime
{
    private const string AdminEmail = "admin@autohub.local";
    private const string AdminPassword = "Admin123!";

    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .Build();

    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;
    private string _adminToken = null!;

    public async Task InitializeAsync()
    {
        await _db.StartAsync();

        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", _db.GetConnectionString());
        Environment.SetEnvironmentVariable("JwtSettings__Secret", "Integration_Tests_Secret_Must_Be_Long_Enough_32B+");
        Environment.SetEnvironmentVariable("JwtSettings__Issuer", "AutoHubAPI");
        Environment.SetEnvironmentVariable("JwtSettings__Audience", "AutoHubClient");
        Environment.SetEnvironmentVariable("SeedAdmin__Email", AdminEmail);
        Environment.SetEnvironmentVariable("SeedAdmin__Password", AdminPassword);

        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();

        var login = await _client.PostAsJsonAsync("/api/auth/login", new { Email = AdminEmail, Password = AdminPassword });
        login.EnsureSuccessStatusCode();
        _adminToken = JsonDocument.Parse(await login.Content.ReadAsStringAsync())
            .RootElement.GetProperty("token").GetString()!;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _db.DisposeAsync();
    }

    private static object NewVehicle(string vin = "1HGCM82633A123456") => new
    {
        vin,
        make = "Toyota",
        model = "Corolla",
        year = 2022,
        price = 250000m,
        mileage = 15000,
        transmission = "Automática",
        fuelType = "Gasolina",
        description = "Seminuevo"
    };

    private async Task<Guid> CreateVehicleAsync(string vin = "1HGCM82633A123456")
    {
        var response = await _client.PostAsJsonAsync("/api/vehicles", NewVehicle(vin));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return Guid.Parse(JsonDocument.Parse(await response.Content.ReadAsStringAsync())
            .RootElement.GetProperty("id").GetString()!);
    }

    [Fact]
    public async Task List_without_token_returns_401()
    {
        using var anon = _factory.CreateClient();
        var response = await anon.GetAsync("/api/vehicles");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_and_get_roundtrip()
    {
        var id = await CreateVehicleAsync();

        var get = await _client.GetAsync($"/api/vehicles/{id}");
        get.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = JsonDocument.Parse(await get.Content.ReadAsStringAsync()).RootElement;
        body.GetProperty("vin").GetString().Should().Be("1HGCM82633A123456");
        body.GetProperty("status").GetString().Should().Be("Available");
        body.GetProperty("creatorEmail").GetString().Should().Be(AdminEmail);
    }

    [Fact]
    public async Task Create_duplicate_vin_returns_409()
    {
        await CreateVehicleAsync("1HGCM82633A123456");

        var response = await _client.PostAsJsonAsync("/api/vehicles", NewVehicle("1HGCM82633A123456"));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Create_invalid_body_returns_400()
    {
        var response = await _client.PostAsJsonAsync("/api/vehicles", NewVehicle("SHORT"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task List_filters_and_pagination()
    {
        await CreateVehicleAsync("1HGCM82633A111111");
        await CreateVehicleAsync("2HGCM82633A222222");

        var filtered = await _client.GetAsync("/api/vehicles?search=111111");
        filtered.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = JsonDocument.Parse(await filtered.Content.ReadAsStringAsync()).RootElement;
        body.GetProperty("totalCount").GetInt32().Should().Be(1);

        var page = await _client.GetAsync("/api/vehicles?page=1&pageSize=1");
        var pageBody = JsonDocument.Parse(await page.Content.ReadAsStringAsync()).RootElement;
        pageBody.GetProperty("items").GetArrayLength().Should().Be(1);
        pageBody.GetProperty("totalCount").GetInt32().Should().Be(2);
    }

    [Fact]
    public async Task Update_and_archive_flow()
    {
        var id = await CreateVehicleAsync("1HGCM82633A333333");

        var update = await _client.PutAsJsonAsync($"/api/vehicles/{id}", new
        {
            vin = "1HGCM82633A333333",
            make = "Honda",
            model = "Civic",
            year = 2023,
            price = 300000m,
            mileage = 5000,
            transmission = "Manual",
            fuelType = "Gasolina",
            status = "Reserved",
            description = "Apartado"
        });
        update.StatusCode.Should().Be(HttpStatusCode.OK);
        JsonDocument.Parse(await update.Content.ReadAsStringAsync())
            .RootElement.GetProperty("make").GetString().Should().Be("Honda");

        var archive = await _client.PostAsync($"/api/vehicles/{id}/archive", null);
        archive.StatusCode.Should().Be(HttpStatusCode.OK);
        JsonDocument.Parse(await archive.Content.ReadAsStringAsync())
            .RootElement.GetProperty("status").GetString().Should().Be("Archived");

        var editArchived = await _client.PutAsJsonAsync($"/api/vehicles/{id}", new
        {
            vin = "1HGCM82633A333333",
            make = "Honda",
            model = "Civic",
            year = 2023,
            price = 300000m,
            mileage = 5000,
            transmission = "Manual",
            fuelType = "Gasolina",
            status = "Available",
            description = null as string
        });
        editArchived.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Get_missing_vehicle_returns_404()
    {
        var response = await _client.GetAsync($"/api/vehicles/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Image_upload_and_remove_roundtrip()
    {
        var id = await CreateVehicleAsync("1HGCM82633A444444");

        using var content = new MultipartFormDataContent();
        var file = new ByteArrayContent([1, 2, 3, 4]);
        file.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(file, "file", "foto.jpg");

        var upload = await _client.PostAsync($"/api/vehicles/{id}/images", content);
        upload.StatusCode.Should().Be(HttpStatusCode.OK);
        var uploadBody = JsonDocument.Parse(await upload.Content.ReadAsStringAsync()).RootElement;
        var imageId = Guid.Parse(uploadBody.GetProperty("id").GetString()!);

        var asset = await _client.GetAsync(uploadBody.GetProperty("imageUrl").GetString()!);
        asset.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await _client.GetAsync("/api/vehicles?search=444444");
        var item = JsonDocument.Parse(await list.Content.ReadAsStringAsync())
            .RootElement.GetProperty("items").EnumerateArray().First();
        item.GetProperty("imageCount").GetInt32().Should().Be(1);
        item.GetProperty("coverImageUrl").GetString().Should().Be(uploadBody.GetProperty("imageUrl").GetString());

        var remove = await _client.DeleteAsync($"/api/vehicles/{id}/images/{imageId}");
        remove.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Upload_with_generic_content_type_sniffs_webp()
    {
        var id = await CreateVehicleAsync("1HGCM82633A666666");

        using var content = new MultipartFormDataContent();
        var webp = new byte[] { 0x52, 0x49, 0x46, 0x46, 0, 0, 0, 0, 0x57, 0x45, 0x42, 0x50 };
        var file = new ByteArrayContent(webp);
        file.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Add(file, "file", "foto.webp");

        var upload = await _client.PostAsync($"/api/vehicles/{id}/images", content);

        upload.StatusCode.Should().Be(HttpStatusCode.OK);
        var url = JsonDocument.Parse(await upload.Content.ReadAsStringAsync())
            .RootElement.GetProperty("imageUrl").GetString()!;
        url.Should().EndWith(".webp");

        var asset = await _client.GetAsync(url);
        asset.StatusCode.Should().Be(HttpStatusCode.OK);
        asset.Content.Headers.ContentType?.MediaType.Should().Be("image/webp");
    }

    [Fact]
    public async Task Upload_disallowed_mime_returns_400()
    {
        var id = await CreateVehicleAsync("1HGCM82633A555555");

        using var content = new MultipartFormDataContent();
        var file = new ByteArrayContent("%PDF-1.4"u8.ToArray());
        file.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        content.Add(file, "file", "doc.pdf");

        var response = await _client.PostAsync($"/api/vehicles/{id}/images", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
