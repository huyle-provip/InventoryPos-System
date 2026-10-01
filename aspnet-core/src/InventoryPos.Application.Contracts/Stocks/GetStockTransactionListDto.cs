using System;
using Volo.Abp.Application.Dtos;

namespace InventoryPos.Stocks;

public class GetStockTransactionListDto : PagedAndSortedResultRequestDto
{
    public Guid? ProductId { get; set; }
}
