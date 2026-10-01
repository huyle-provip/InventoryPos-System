using System;
using System.ComponentModel.DataAnnotations;

namespace InventoryPos.Stocks;

public class CreateStockTransactionDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public StockTransactionType Type { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [StringLength(512)]
    public string? Note { get; set; }
}
