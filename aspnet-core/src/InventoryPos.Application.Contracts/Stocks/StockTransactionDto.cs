using System;
using Volo.Abp.Application.Dtos;

namespace InventoryPos.Stocks;

public class StockTransactionDto : AuditedEntityDto<Guid>
{
    public Guid ProductId { get; set; }

    public StockTransactionType Type { get; set; }

    public int Quantity { get; set; }

    public string? Note { get; set; }
}
