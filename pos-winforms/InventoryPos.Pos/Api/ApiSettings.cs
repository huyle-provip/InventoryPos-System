using System.Text.Json;

namespace InventoryPos.Pos.Api;

/// <summary>Connection settings read from appsettings.json next to the executable; falls back to local development defaults.</summary>
public class ApiSettings
{
    public string BaseUrl { get; set; } = "https://localhost:44395";

    public string ClientId { get; set; } = "InventoryPos_App";

    public static ApiSettings Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (!File.Exists(path))
        {
            return new ApiSettings();
        }

        try
        {
            var settings = JsonSerializer.Deserialize<ApiSettings>(
                File.ReadAllText(path),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return settings ?? new ApiSettings();
        }
        catch (JsonException)
        {
            return new ApiSettings();
        }
    }
}
