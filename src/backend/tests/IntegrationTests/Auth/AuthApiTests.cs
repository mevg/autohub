using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace AutoHub.IntegrationTests.Auth;

/// <summary>
/// Login de punta a punta: Postgres real (Testcontainers), migraciones y seed
/// aplicados por el arranque de la API.
/// </summary>
public sealed class AuthApiTests : IAsyncLifetime
{
    private const string AdminEmail = "admin@autohub.local";
    private const string AdminPassword = "Admin123!";

    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .Build();

    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

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
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _db.DisposeAsync();
    }

    [Fact]
    public async Task Login_with_valid_credentials_returns_token()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { Email = AdminEmail, Password = AdminPassword });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        body.GetProperty("token").GetString().Should().NotBeNullOrWhiteSpace();
        body.GetProperty("email").GetString().Should().Be(AdminEmail);
        body.GetProperty("role").GetString().Should().Be("Admin");
    }

    [Fact]
    public async Task Login_with_wrong_password_returns_401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { Email = AdminEmail, Password = "Wrong123!" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_with_unknown_email_returns_401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { Email = "nadie@autohub.local", Password = "Admin123!" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_with_invalid_body_returns_400()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { Email = "no-es-email", Password = "" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Me_with_token_returns_current_user()
    {
        var login = await _client.PostAsJsonAsync("/api/auth/login", new { Email = AdminEmail, Password = AdminPassword });
        var token = JsonDocument.Parse(await login.Content.ReadAsStringAsync()).RootElement.GetProperty("token").GetString();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        body.GetProperty("email").GetString().Should().Be(AdminEmail);
    }

    [Fact]
    public async Task Me_without_token_returns_401()
    {
        var response = await _client.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
