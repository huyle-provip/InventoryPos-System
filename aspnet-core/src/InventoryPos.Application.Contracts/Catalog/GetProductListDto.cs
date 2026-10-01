using System;
using Volo.Abp.Application.Dtos;

namespace InventoryPos.Catalog;

public class GetProductListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }

    public Guid? CategoryId { get; set; }

    public bool? LowStockOnly { get; set; }
}
