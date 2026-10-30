using System;
using Volo.Abp.Domain.Entities;

namespace InventoryPos.Sales;

public class SaleReturnItem : Entity<Guid>
{
    protected SaleReturnItem()
    {
    }

    public SaleReturnItem(Guid id) : base(id)
    {
    }

    public Guid SaleReturnId { get; set; }

    public Guid SaleOrderItemId { get; set; }

    public Guid ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal RefundAmount { get; set; }
}
