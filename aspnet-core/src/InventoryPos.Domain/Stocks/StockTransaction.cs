using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace InventoryPos.Stocks;

public class StockTransaction : AuditedAggregateRoot<Guid>
{
    public Guid ProductId { get; set; }

    public StockTransactionType Type { get; set; }

    public int Quantity { get; set; }

    public string? Note { get; set; }
}
