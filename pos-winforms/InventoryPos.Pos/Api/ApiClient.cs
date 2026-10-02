using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace InventoryPos.Pos.Api;

public class ApiException : Exception
{
    public ApiException(string message) : base(message)
    {
    }
}

public class ApiClient
{
    private const string BaseUrl = "https://localhost:44395";
    private const string ClientId = "InventoryPos_App";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _httpClient = new() { BaseAddress = new Uri(BaseUrl) };

    public string? AccessToken { get; private set; }

    public bool IsLoggedIn => !string.IsNullOrEmpty(AccessToken);

    public async Task LoginAsync(string username, string password)
    {
        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = ClientId,
            ["username"] = username,
            ["password"] = password,
            ["scope"] = "InventoryPos offline_access",
        };

        using var response = await _httpClient.PostAsync("/connect/token", new FormUrlEncodedContent(form));
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new ApiException(ExtractTokenError(body));
        }

        var token = JsonSerializer.Deserialize<TokenResponse>(body, JsonOptions)
            ?? throw new ApiException("Unexpected login response from server.");

        AccessToken = token.AccessToken;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
    }

    public void Logout()
    {
        AccessToken = null;
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    public async Task<PagedResult<ProductDto>> GetProductsAsync(string? filter = null)
    {
        var url = "/api/app/product?maxResultCount=1000";
        if (!string.IsNullOrWhiteSpace(filter))
        {
            url += "&filter=" + Uri.EscapeDataString(filter);
        }

        return await GetAsync<PagedResult<ProductDto>>(url);
    }

    public async Task<SaleOrderDto> CreateSaleOrderAsync(CreateSaleOrderDto input)
    {
        return await PostAsync<CreateSaleOrderDto, SaleOrderDto>("/api/app/sale-order", input);
    }

    public async Task<PricingInfoDto> GetPricingInfoAsync()
    {
        return await GetAsync<PricingInfoDto>("/api/app/sale-order/pricing-info");
    }

    /// <summary>Returns null when the product has no image (or it could not be loaded) so the UI can simply show nothing.</summary>
    public async Task<Image?> GetProductImageAsync(string productId)
    {
        try
        {
            using var response = await _httpClient.GetAsync($"/api/app/product-image/{productId}");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var bytes = await response.Content.ReadAsByteArrayAsync();
            using var stream = new MemoryStream(bytes);
            using var original = Image.FromStream(stream);
            return new Bitmap(original);
        }
        catch (Exception ex) when (ex is HttpRequestException or ArgumentException or OutOfMemoryException)
        {
            return null;
        }
    }

    private async Task<T> GetAsync<T>(string url)
    {
        using var response = await _httpClient.GetAsync(url);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new ApiException(ExtractApiError(body, response.ReasonPhrase));
        }

        return JsonSerializer.Deserialize<T>(body, JsonOptions)
            ?? throw new ApiException("Unexpected empty response from server.");
    }

    private async Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest input)
    {
        var json = JsonSerializer.Serialize(input);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await _httpClient.PostAsync(url, content);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new ApiException(ExtractApiError(body, response.ReasonPhrase));
        }

        return JsonSerializer.Deserialize<TResponse>(body, JsonOptions)
            ?? throw new ApiException("Unexpected empty response from server.");
    }

    private static string ExtractApiError(string body, string? fallback)
    {
        try
        {
            var envelope = JsonSerializer.Deserialize<ApiErrorEnvelope>(body, JsonOptions);
            if (!string.IsNullOrWhiteSpace(envelope?.Error?.Message))
            {
                return envelope!.Error!.Message!;
            }
        }
        catch (JsonException)
        {
            // fall through to generic message
        }

        return fallback ?? "The request failed.";
    }

    private static string ExtractTokenError(string body)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("error_description", out var descriptionElement))
            {
                return descriptionElement.GetString() ?? "Login failed.";
            }

            if (doc.RootElement.TryGetProperty("error", out var errorElement))
            {
                return errorElement.GetString() ?? "Login failed.";
            }
        }
        catch (JsonException)
        {
            // fall through to generic message
        }

        return "Invalid username or password.";
    }
}
