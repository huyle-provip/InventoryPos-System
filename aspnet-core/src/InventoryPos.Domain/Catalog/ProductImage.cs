using System;
using Volo.Abp.Domain.Entities;

namespace InventoryPos.Catalog;

/// <summary>Kept in its own table so product list queries never load image bytes. Id equals the product id.</summary>
public class ProductImage : Entity<Guid>
{
    public string ContentType { get; set; } = string.Empty;

    public byte[] Data { get; set; } = Array.Empty<byte>();

    protected ProductImage()
    {
    }

    public ProductImage(Guid productId, string contentType, byte[] data)
        : base(productId)
    {
        ContentType = contentType;
        Data = data;
    }
}
