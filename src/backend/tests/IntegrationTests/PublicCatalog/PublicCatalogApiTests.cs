using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace AutoHub.IntegrationTests.PublicCatalog;

public sealed class PublicCatalogApiTests : IAsyncLifetime
{
    private const string AdminEmail = "admin@autohub.local";
    private const string AdminPassword = "Admin123!";
    private const string WhatsAppPhone = "5215555555555";

    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .Build();

    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _admin = null!;
    private HttpClient _public = null!;

    public async Task InitializeAsync()
    {
        await _db.StartAsync();

        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", _db.GetConnectionString());
        Environment.SetEnvironmentVariable("JwtSettings__Secret", "Integration_Tests_Secret_Must_Be_Long_Enough_32B+");
        Environment.SetEnvironmentVariable("JwtSettings__Issuer", "AutoHubAPI");
        Environment.SetEnvironmentVariable("JwtSettings__Audience", "AutoHubClient");
        Environment.SetEnvironmentVariable("SeedAdmin__Email", AdminEmail);
        Environment.SetEnvironmentVariable("SeedAdmin__Password", AdminPassword);
        Environment.SetEnvironmentVariable("SeedCatalog__Enabled", "false");
        Environment.SetEnvironmentVariable("WhatsApp__PhoneNumber", WhatsAppPhone);
        Environment.SetEnvironmentVariable("Leads__IpHashSalt", "integration-test-salt");

        _factory = new WebApplicationFactory<Program>();
        _admin = _factory.CreateClient();
        _public = _factory.CreateClient();

        var login = await _admin.PostAsJsonAsync("/api/auth/login", new { Email = AdminEmail, Password = AdminPassword });
        login.EnsureSuccessStatusCode();
        var token = JsonDocument.Parse(await login.Content.ReadAsStringAsync())
            .RootElement.GetProperty("token").GetString()!;
        _admin.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task DisposeAsync()
    {
        _admin.Dispose();
        _public.Dispose();
        await _factory.DisposeAsync();
        await _db.DisposeAsync();
    }

    [Fact]
    public async Task Public_catalog_only_exposes_available_vehicles()
    {
        var availableId = await CreateVehicleAsync("1HGCM82633A200001", "Toyota", "Corolla", 2022, 250000m);
        var reservedId = await CreateVehicleAsync("1HGCM82633A200002", "Honda", "Civic", 2023, 320000m);
        await SetStatusAsync(reservedId, "Reserved");

        var list = await _public.GetAsync("/api/public/vehicles");
        list.StatusCode.Should().Be(HttpStatusCode.OK);
        var listBody = JsonDocument.Parse(await list.Content.ReadAsStringAsync()).RootElement;
        listBody.GetProperty("totalCount").GetInt32().Should().Be(1);
        var card = listBody.GetProperty("items")[0];
        card.GetProperty("id").GetString().Should().Be(availableId.ToString());
        card.GetProperty("publicReference").GetString().Should().StartWith("AH-");
        card.TryGetProperty("vin", out _).Should().BeFalse();

        var detail = await _public.GetAsync($"/api/public/vehicles/{availableId}");
        detail.StatusCode.Should().Be(HttpStatusCode.OK);
        var detailBody = JsonDocument.Parse(await detail.Content.ReadAsStringAsync()).RootElement;
        detailBody.GetProperty("whatsAppUrl").GetString().Should()
            .Contain($"https://wa.me/{WhatsAppPhone}");

        var reserved = await _public.GetAsync($"/api/public/vehicles/{reservedId}");
        reserved.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Public_catalog_filters_and_facets_work()
    {
        await CreateVehicleAsync("1HGCM82633A200011", "Toyota", "Corolla", 2022, 250000m);
        await CreateVehicleAsync("1HGCM82633A200012", "Honda", "Civic", 2024, 320000m);

        var filtered = await _public.GetAsync("/api/public/vehicles?make=Toyota&yearFrom=2020&sort=price_asc");
        filtered.StatusCode.Should().Be(HttpStatusCode.OK);
        var filteredBody = JsonDocument.Parse(await filtered.Content.ReadAsStringAsync()).RootElement;
        filteredBody.GetProperty("totalCount").GetInt32().Should().Be(1);
        filteredBody.GetProperty("items")[0].GetProperty("model").GetString().Should().Be("Corolla");

        var facets = await _public.GetAsync("/api/public/vehicles/filters");
        facets.StatusCode.Should().Be(HttpStatusCode.OK);
        var facetsBody = JsonDocument.Parse(await facets.Content.ReadAsStringAsync()).RootElement;
        facetsBody.GetProperty("makes").EnumerateArray()
            .Select(m => m.GetProperty("value").GetString())
            .Should().Contain(["Honda", "Toyota"]);

        var models = await _public.GetAsync("/api/public/vehicles/filters?make=Toyota");
        var modelsBody = JsonDocument.Parse(await models.Content.ReadAsStringAsync()).RootElement;
        modelsBody.GetProperty("models").EnumerateArray()
            .Select(m => m.GetProperty("value").GetString())
            .Should().ContainSingle().Which.Should().Be("Corolla");
    }

    [Fact]
    public async Task Invalid_filter_ranges_return_validation_problem()
    {
        var response = await _public.GetAsync("/api/public/vehicles?yearFrom=2025&yearTo=2020");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Lead_creation_is_idempotent_and_conflicts_on_payload_change()
    {
        var vehicleId = await CreateVehicleAsync("1HGCM82633A200021", "Mazda", "CX-5", 2023, 450000m);
        var key = $"test-{Guid.NewGuid():N}";
        var lead = new
        {
            name = "Ana Pérez",
            phone = "5551234567",
            email = "ana@example.com",
            message = "Quiero información",
            consentAccepted = false
        };

        var first = await SendLeadAsync(vehicleId, lead, key, "203.0.113.21");
        first.StatusCode.Should().Be(HttpStatusCode.Created);
        var firstId = JsonDocument.Parse(await first.Content.ReadAsStringAsync())
            .RootElement.GetProperty("id").GetString();

        var replay = await SendLeadAsync(vehicleId, lead, key, "203.0.113.21");
        replay.StatusCode.Should().Be(HttpStatusCode.OK);
        JsonDocument.Parse(await replay.Content.ReadAsStringAsync())
            .RootElement.GetProperty("id").GetString().Should().Be(firstId);

        var conflict = await SendLeadAsync(
            vehicleId,
            new
            {
                name = "Otra Persona",
                phone = lead.phone,
                email = lead.email,
                message = lead.message,
                consentAccepted = false
            },
            key,
            "203.0.113.21");
        conflict.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Duplicate_recent_lead_returns_existing_record()
    {
        var vehicleId = await CreateVehicleAsync("1HGCM82633A200031", "Nissan", "Versa", 2024, 280000m);
        var lead = new
        {
            name = "Luis Ramírez",
            phone = "5559876543",
            email = "luis@example.com",
            message = (string?)null,
            consentAccepted = false
        };

        var first = await SendLeadAsync(vehicleId, lead, Guid.NewGuid().ToString("N"), "203.0.113.31");
        var second = await SendLeadAsync(vehicleId, lead, Guid.NewGuid().ToString("N"), "203.0.113.31");

        first.StatusCode.Should().Be(HttpStatusCode.Created);
        second.StatusCode.Should().Be(HttpStatusCode.OK);
        JsonDocument.Parse(await first.Content.ReadAsStringAsync())
            .RootElement.GetProperty("id").GetString()
            .Should().Be(JsonDocument.Parse(await second.Content.ReadAsStringAsync())
                .RootElement.GetProperty("id").GetString());
    }

    [Fact]
    public async Task Lead_is_rejected_for_unavailable_vehicle()
    {
        var vehicleId = await CreateVehicleAsync("1HGCM82633A200041", "Kia", "K3", 2022, 210000m);
        await SetStatusAsync(vehicleId, "Reserved");

        var response = await SendLeadAsync(
            vehicleId,
            new
            {
                name = "Pedro López",
                phone = "5551112233",
                email = "pedro@example.com",
                message = (string?)null,
                consentAccepted = false
            },
            Guid.NewGuid().ToString("N"),
            "203.0.113.41");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Lead_sixth_request_from_same_ip_is_rate_limited()
    {
        var vehicleId = await CreateVehicleAsync("1HGCM82633A200051", "Ford", "Ranger", 2022, 560000m);

        for (var i = 0; i < 5; i++)
        {
            var response = await SendLeadAsync(
                vehicleId,
                new
                {
                    name = $"Interesado {i}",
                    phone = $"55500000{i:00}",
                    email = $"lead{i}@example.com",
                    message = (string?)null,
                    consentAccepted = false
                },
                Guid.NewGuid().ToString("N"),
                "203.0.113.51");
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        var blocked = await SendLeadAsync(
            vehicleId,
            new
            {
                name = "Interesado 6",
                phone = "5550000006",
                email = "lead6@example.com",
                message = (string?)null,
                consentAccepted = false
            },
            Guid.NewGuid().ToString("N"),
            "203.0.113.51");

        blocked.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    private Task<HttpResponseMessage> SendLeadAsync(
        Guid vehicleId,
        object body,
        string idempotencyKey,
        string forwardedFor)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/public/vehicles/{vehicleId}/leads")
        {
            Content = JsonContent.Create(body)
        };
        request.Headers.Add("Idempotency-Key", idempotencyKey);
        request.Headers.Add("X-Forwarded-For", forwardedFor);
        return _public.SendAsync(request);
    }

    private async Task<Guid> CreateVehicleAsync(
        string vin,
        string make,
        string model,
        int year,
        decimal price)
    {
        var response = await _admin.PostAsJsonAsync("/api/vehicles", new
        {
            vin,
            make,
            model,
            year,
            price,
            mileage = 10000,
            transmission = "Automática",
            fuelType = "Gasolina",
            description = "Vehículo de prueba"
        });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return Guid.Parse(JsonDocument.Parse(await response.Content.ReadAsStringAsync())
            .RootElement.GetProperty("id").GetString()!);
    }

    private async Task SetStatusAsync(Guid id, string status)
    {
        var get = await _admin.GetAsync($"/api/vehicles/{id}");
        get.EnsureSuccessStatusCode();
        var current = JsonDocument.Parse(await get.Content.ReadAsStringAsync()).RootElement;

        var response = await _admin.PutAsJsonAsync($"/api/vehicles/{id}", new
        {
            vin = current.GetProperty("vin").GetString(),
            make = current.GetProperty("make").GetString(),
            model = current.GetProperty("model").GetString(),
            year = current.GetProperty("year").GetInt32(),
            price = current.GetProperty("price").GetDecimal(),
            mileage = current.GetProperty("mileage").GetInt32(),
            transmission = current.GetProperty("transmission").GetString(),
            fuelType = current.GetProperty("fuelType").GetString(),
            status,
            description = current.GetProperty("description").GetString()
        });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
