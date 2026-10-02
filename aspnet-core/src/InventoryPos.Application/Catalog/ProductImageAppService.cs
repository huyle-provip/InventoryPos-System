using System;
using System.IO;
using System.Threading.Tasks;
using InventoryPos.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Content;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace InventoryPos.Catalog;

public class ProductImageAppService : InventoryPosAppService, IProductImageAppService
{
    private const int MaxImageBytes = 1024 * 1024;

    private readonly IRepository<Product, Guid> _productRepository;
    private readonly IRepository<ProductImage, Guid> _imageRepository;

    public ProductImageAppService(
        IRepository<Product, Guid> productRepository,
        IRepository<ProductImage, Guid> imageRepository)
    {
        _productRepository = productRepository;
        _imageRepository = imageRepository;
    }

    [AllowAnonymous]
    public async Task<IRemoteStreamContent> GetAsync(Guid id)
    {
        var image = await _imageRepository.FindAsync(id);
        if (image == null)
        {
            throw new EntityNotFoundException(typeof(ProductImage), id);
        }

        return new RemoteStreamContent(new MemoryStream(image.Data), "product-image", image.ContentType);
    }

    [Authorize(InventoryPosPermissions.Products.Edit)]
    public async Task UpdateAsync(Guid id, IRemoteStreamContent file)
    {
        var product = await _productRepository.GetAsync(id);

        if (file.ContentLength is > MaxImageBytes)
        {
            throw new UserFriendlyException("The image is larger than 1 MB.");
        }

        using var buffer = new MemoryStream();
        await using (var upload = file.GetStream())
        {
            var chunk = new byte[8192];
            int read;
            while ((read = await upload.ReadAsync(chunk)) > 0)
            {
                buffer.Write(chunk, 0, read);
                if (buffer.Length > MaxImageBytes)
                {
                    throw new UserFriendlyException("The image is larger than 1 MB.");
                }
            }
        }

        var data = buffer.ToArray();
        var contentType = DetectImageType(data)
            ?? throw new UserFriendlyException("Only PNG or JPEG images are supported.");

        var existing = await _imageRepository.FindAsync(id);
        if (existing == null)
        {
            await _imageRepository.InsertAsync(new ProductImage(id, contentType, data));
        }
        else
        {
            existing.ContentType = contentType;
            existing.Data = data;
            await _imageRepository.UpdateAsync(existing);
        }

        product.HasImage = true;
        await _productRepository.UpdateAsync(product);
    }

    [Authorize(InventoryPosPermissions.Products.Edit)]
    public async Task DeleteAsync(Guid id)
    {
        var product = await _productRepository.GetAsync(id);
        await _imageRepository.DeleteAsync(x => x.Id == id);

        product.HasImage = false;
        await _productRepository.UpdateAsync(product);
    }

    private static string? DetectImageType(byte[] data)
    {
        if (data.Length >= 8 &&
            data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47 &&
            data[4] == 0x0D && data[5] == 0x0A && data[6] == 0x1A && data[7] == 0x0A)
        {
            return "image/png";
        }

        if (data.Length >= 3 && data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF)
        {
            return "image/jpeg";
        }

        return null;
    }
}
