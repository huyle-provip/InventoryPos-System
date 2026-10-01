using System.Text.Json.Serialization;

namespace InventoryPos.Pos.Api;

public class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}

public class ApiErrorEnvelope
{
    public ApiErrorDetail? Error { get; set; }
}

public class ApiErrorDetail
{
    public string? Code { get; set; }
    public string? Message { get; set; }
    public string? Details { get; set; }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
}

public class ProductDto
{
    public string Id { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? CategoryId { get; set; }
    public decimal Price { get; set; }
    public int QuantityOnHand { get; set; }
    public int ReorderThreshold { get; set; }
    public bool IsLowStock { get; set; }
}

public class CreateSaleOrderItemDto
{
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class CreateSaleOrderDto
{
    public List<CreateSaleOrderItemDto> Items { get; set; } = new();
}

public class SaleOrderDto
{
    public string Id { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime CreationTime { get; set; }
    public List<SaleOrderItemDto> Items { get; set; } = new();
}

public class SaleOrderItemDto
{
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
