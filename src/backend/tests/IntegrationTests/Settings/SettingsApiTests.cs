using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace AutoHub.IntegrationTests.Settings;

public sealed class SettingsApiTests : IAsyncLifetime
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
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _db.DisposeAsync();
    }

    private void UseAdminToken()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
    }

    [Fact]
    public async Task Get_branding_is_public_with_defaults()
    {
        var response = await _client.GetAsync("/api/settings/branding");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        body.GetProperty("appName").GetString().Should().Be("AutoHub");
        body.GetProperty("primary").GetString().Should().Be("#4DDBC4");
    }

    [Fact]
    public async Task Put_branding_without_token_returns_401()
    {
        var response = await _client.PutAsJsonAsync("/api/settings/branding",
            new { appName = "X", primaryLight = "#71E2DD", primary = "#4DDBC4", primaryDark = "#45C5B0" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Put_branding_with_admin_updates_and_returns_branding()
    {
        UseAdminToken();

        var response = await _client.PutAsJsonAsync("/api/settings/branding",
            new { appName = "Mi Tienda", primaryLight = "#71DDFF", primary = "#4DC4FF", primaryDark = "#45B0E5" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        body.GetProperty("appName").GetString().Should().Be("Mi Tienda");
        body.GetProperty("primary").GetString().Should().Be("#4DC4FF");
    }

    [Fact]
    public async Task Put_branding_with_invalid_color_returns_400()
    {
        UseAdminToken();

        var response = await _client.PutAsJsonAsync("/api/settings/branding",
            new { appName = "Mi Tienda", primaryLight = "no-es-color", primary = "#4DC4FF", primaryDark = "#45B0E5" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Upload_logo_returns_url_and_file_is_served()
    {
        UseAdminToken();
        using var content = new MultipartFormDataContent();
        // PNG mínimo de 1x1.
        var png = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==");
        var file = new ByteArrayContent(png);
        file.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(file, "file", "logo.png");

        var response = await _client.PostAsync("/api/settings/branding/assets/logo", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var logoUrl = JsonDocument.Parse(await response.Content.ReadAsStringAsync())
            .RootElement.GetProperty("logoUrl").GetString()!;
        logoUrl.Should().StartWith("/branding/logo-");

        var asset = await _client.GetAsync(logoUrl);
        asset.StatusCode.Should().Be(HttpStatusCode.OK);
        asset.Content.Headers.ContentType?.MediaType.Should().Be("image/png");
    }

    [Fact]
    public async Task Upload_disallowed_mime_returns_400()
    {
        UseAdminToken();
        using var content = new MultipartFormDataContent();
        var file = new ByteArrayContent("%PDF-1.4"u8.ToArray());
        file.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        content.Add(file, "file", "doc.pdf");

        var response = await _client.PostAsync("/api/settings/branding/assets/logo", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Upload_without_token_returns_401()
    {
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent([1, 2, 3]), "file", "logo.png");

        var response = await _client.PostAsync("/api/settings/branding/assets/logo", content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Banner_upload_and_remove_roundtrip()
    {
        UseAdminToken();
        using var content = new MultipartFormDataContent();
        var file = new ByteArrayContent([1, 2, 3, 4]);
        file.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(file, "file", "banner.jpg");

        var upload = await _client.PostAsync("/api/settings/branding/assets/banner", content);
        upload.StatusCode.Should().Be(HttpStatusCode.OK);
        var banners = JsonDocument.Parse(await upload.Content.ReadAsStringAsync())
            .RootElement.GetProperty("bannerUrls").EnumerateArray().ToList();
        banners.Should().ContainSingle();

        var remove = await _client.DeleteAsync("/api/settings/branding/banners/0");
        remove.StatusCode.Should().Be(HttpStatusCode.OK);
        JsonDocument.Parse(await remove.Content.ReadAsStringAsync())
            .RootElement.GetProperty("bannerUrls").EnumerateArray().ToList().Should().BeEmpty();

        var invalid = await _client.DeleteAsync("/api/settings/branding/banners/0");
        invalid.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
