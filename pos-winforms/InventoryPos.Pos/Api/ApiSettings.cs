using Microsoft.Extensions.Configuration;

namespace InventoryPos.Pos.Api;

/// <summary>
/// Connection settings bound from the "Api" configuration section. Sources, later ones winning:
/// appsettings.json (required), appsettings.{DOTNET_ENVIRONMENT}.json (optional), then INVENTORYPOS_ environment variables
/// (for example INVENTORYPOS_Api__BaseUrl). Environment-specific values have no defaults, so a missing one fails at startup.
/// </summary>
public class ApiSettings
{
    public const string SectionName = "Api";

    public string BaseUrl { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;

    public int TimeoutSeconds { get; set; } = 30;

    public static ApiSettings Load()
    {
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables(prefix: "INVENTORYPOS_")
            .Build();

        var settings = configuration.GetSection(SectionName).Get<ApiSettings>() ?? new ApiSettings();
        settings.Validate();
        return settings;
    }

    private void Validate()
    {
        if (!Uri.TryCreate(BaseUrl, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
        {
            throw new InvalidOperationException($"Configuration '{SectionName}:BaseUrl' must be an absolute http(s) URL, but was '{BaseUrl}'.");
        }

        if (string.IsNullOrWhiteSpace(ClientId))
        {
            throw new InvalidOperationException($"Configuration '{SectionName}:ClientId' is required.");
        }

        if (TimeoutSeconds <= 0)
        {
            throw new InvalidOperationException($"Configuration '{SectionName}:TimeoutSeconds' must be greater than zero.");
        }
    }
}
