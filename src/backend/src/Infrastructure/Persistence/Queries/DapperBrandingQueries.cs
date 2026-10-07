using System.Text.Json;

namespace AutoHub.Infrastructure.Persistence.Queries;

/// <summary>
/// Debe mantenerse en sync con <see cref="Configurations.BrandingConfiguration"/>.
/// </summary>
public sealed class DapperBrandingQueries(ISqlConnectionFactory connectionFactory) : IBrandingQueries
{
    private sealed record BrandingRow(
        int Id,
        string AppName,
        string LogoUrl,
        string FaviconUrl,
        string BannerUrls,
        string PrimaryLight,
        string Primary,
        string PrimaryDark);

    public async Task<Branding?> GetAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT id AS "Id",
                   app_name AS "AppName",
                   logo_url AS "LogoUrl",
                   favicon_url AS "FaviconUrl",
                   banner_urls::text AS "BannerUrls",
                   primary_light AS "PrimaryLight",
                   "primary" AS "Primary",
                   primary_dark AS "PrimaryDark"
            FROM branding
            WHERE id = 1;
            """;

        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
        var row = await connection.QueryFirstOrDefaultAsync<BrandingRow?>(command);

        if (row is null)
        {
            return null;
        }

        return new Branding
        {
            Id = row.Id,
            AppName = row.AppName,
            LogoUrl = row.LogoUrl,
            FaviconUrl = row.FaviconUrl,
            BannerUrls = JsonSerializer.Deserialize<List<string>>(row.BannerUrls) ?? [],
            PrimaryLight = row.PrimaryLight,
            Primary = row.Primary,
            PrimaryDark = row.PrimaryDark
        };
    }
}
