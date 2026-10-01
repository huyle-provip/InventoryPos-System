using System.ComponentModel.DataAnnotations;

namespace InventoryPos.Catalog;

public class CreateUpdateCategoryDto
{
    [Required]
    [StringLength(128)]
    public string Name { get; set; } = string.Empty;
}
