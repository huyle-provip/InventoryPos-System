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
    public bool HasImage { get; set; }
}

public enum DiscountType
{
    None = 0,
    Percent = 1,
    Amount = 2,
}

public enum PaymentMethod
{
    Cash = 1,
    Card = 2,
}

public class PricingInfoDto
{
    public decimal TaxRatePercent { get; set; }
}

public class CreateSaleOrderItemDto
{
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class CreateSaleOrderDto
{
    public List<CreateSaleOrderItemDto> Items { get; set; } = new();
    public DiscountType DiscountType { get; set; } = DiscountType.None;
    public decimal DiscountValue { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public decimal? AmountTendered { get; set; }
}

public class SaleOrderDto
{
    public string Id { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxRatePercent { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal AmountTendered { get; set; }
    public decimal ChangeGiven { get; set; }
    public DateTime CreationTime { get; set; }
    public List<SaleOrderItemDto> Items { get; set; } = new();
}

public class SaleOrderItemDto
{
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
