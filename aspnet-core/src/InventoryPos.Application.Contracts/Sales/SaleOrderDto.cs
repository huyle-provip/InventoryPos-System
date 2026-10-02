using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace InventoryPos.Sales;

public class SaleOrderDto : AuditedEntityDto<Guid>
{
    public decimal Subtotal { get; set; }

    public DiscountType DiscountType { get; set; }

    public decimal DiscountValue { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TaxRatePercent { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public decimal AmountTendered { get; set; }

    public decimal ChangeGiven => PaymentMethod == PaymentMethod.Cash && AmountTendered > TotalAmount
        ? AmountTendered - TotalAmount
        : 0;

    /// <summary>Total already refunded through sale returns.</summary>
    public decimal RefundedAmount { get; set; }

    public List<SaleOrderItemDto> Items { get; set; } = new();
}
