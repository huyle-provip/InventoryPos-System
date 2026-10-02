using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.Domain.Entities.Auditing;

namespace InventoryPos.Sales;

public class SaleOrder : AuditedAggregateRoot<Guid>
{
    /// <summary>Sum of line totals before discount and tax.</summary>
    public decimal Subtotal { get; set; }

    public DiscountType DiscountType { get; set; }

    /// <summary>The percent or amount the cashier entered, depending on <see cref="DiscountType"/>.</summary>
    public decimal DiscountValue { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TaxRatePercent { get; set; }

    public decimal TaxAmount { get; set; }

    /// <summary>Final amount charged: subtotal - discount + tax.</summary>
    public decimal TotalAmount { get; set; }

    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

    public decimal AmountTendered { get; set; }

    public List<SaleOrderItem> Items { get; set; } = new();

    /// <summary>Line total of the items as originally sold; independent of the stored Subtotal so old rows still work.</summary>
    public decimal GetItemsSubtotal()
    {
        return Items.Sum(i => i.Quantity * i.UnitPrice);
    }
}
