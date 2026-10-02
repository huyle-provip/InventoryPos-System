using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InventoryPos.Sales;

public class CreateSaleOrderDto
{
    [Required]
    [MinLength(1)]
    public List<CreateSaleOrderItemDto> Items { get; set; } = new();

    public DiscountType DiscountType { get; set; } = DiscountType.None;

    [Range(0, double.MaxValue)]
    public decimal DiscountValue { get; set; }

    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

    /// <summary>Cash handed over by the customer. Ignored for card payments; defaults to the exact total when omitted.</summary>
    [Range(0, double.MaxValue)]
    public decimal? AmountTendered { get; set; }
}
