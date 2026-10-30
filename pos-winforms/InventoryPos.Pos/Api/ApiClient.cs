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
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly ApiSettings _settings;
    private readonly HttpClient _httpClient;
    private string? _refreshToken;

    public ApiClient(ApiSettings settings)
    {
        _settings = settings;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(settings.BaseUrl),
            Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds),
        };
    }

    public string? AccessToken { get; private set; }

    public bool IsLoggedIn => !string.IsNullOrEmpty(AccessToken);

    public async Task LoginAsync(string username, string password)
    {
        await RequestTokenAsync(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = _settings.ClientId,
            ["username"] = username,
            ["password"] = password,
            ["scope"] = "InventoryPos offline_access",
        });
    }

    public void Logout()
    {
        AccessToken = null;
        _refreshToken = null;
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    private async Task RequestTokenAsync(Dictionary<string, string> form)
    {
        try
        {
            using var response = await _httpClient.PostAsync("/connect/token", new FormUrlEncodedContent(form));
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(ExtractTokenError(body));
            }

            var token = JsonSerializer.Deserialize<TokenResponse>(body, JsonOptions)
                ?? throw new ApiException("Unexpected login response from server.");

            AccessToken = token.AccessToken;
            _refreshToken = token.RefreshToken;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
        }
        catch (Exception ex) when (IsNetworkFailure(ex))
        {
            throw NetworkError();
        }
    }

    /// <summary>Swaps the refresh token for a new access token; returns false when the session cannot be renewed.</summary>
    private async Task<bool> TryRefreshAsync()
    {
        if (string.IsNullOrEmpty(_refreshToken))
        {
            return false;
        }

        try
        {
            await RequestTokenAsync(new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["client_id"] = _settings.ClientId,
                ["refresh_token"] = _refreshToken,
            });
            return true;
        }
        catch (ApiException)
        {
            return false;
        }
    }

    private static bool IsNetworkFailure(Exception ex) => ex is HttpRequestException or TaskCanceledException;

    private ApiException NetworkError() =>
        new($"Cannot reach the server at {_settings.BaseUrl}. Check that the API is running and your network connection, then try again.");

    /// <summary>Sends a request; on 401 renews the token once and retries, and turns network failures into friendly errors.</summary>
    private async Task<string> SendAsync(Func<HttpRequestMessage> createRequest)
    {
        try
        {
            using var response = await _httpClient.SendAsync(createRequest());
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && await TryRefreshAsync())
            {
                using var retry = await _httpClient.SendAsync(createRequest());
                return await ReadBodyAsync(retry);
            }

            return await ReadBodyAsync(response);
        }
        catch (Exception ex) when (IsNetworkFailure(ex))
        {
            throw NetworkError();
        }
    }

    private static async Task<string> ReadBodyAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        if (response.IsSuccessStatusCode)
        {
            return body;
        }

        throw response.StatusCode switch
        {
            System.Net.HttpStatusCode.Unauthorized => new ApiException("Your session has expired. Please log in again."),
            System.Net.HttpStatusCode.Conflict => new ApiException("Someone else changed this data at the same time. Please refresh and try again."),
            _ => new ApiException(ExtractApiError(body, response.ReasonPhrase)),
        };
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
        var body = await SendAsync(() => new HttpRequestMessage(HttpMethod.Get, url));

        return JsonSerializer.Deserialize<T>(body, JsonOptions)
            ?? throw new ApiException("Unexpected empty response from server.");
    }

    private async Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest input)
    {
        var json = JsonSerializer.Serialize(input);
        var body = await SendAsync(() => new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        });

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
