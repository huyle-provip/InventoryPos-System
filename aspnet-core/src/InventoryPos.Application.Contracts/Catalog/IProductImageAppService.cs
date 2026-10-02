using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace InventoryPos.Catalog;

public interface IProductImageAppService : IApplicationService
{
    /// <summary>Returns the image bytes. Anonymous so a plain img tag can load it; ids are unguessable GUIDs.</summary>
    Task<IRemoteStreamContent> GetAsync(Guid id);

    /// <summary>Replaces the product's image. PNG or JPEG, up to 1 MB.</summary>
    Task UpdateAsync(Guid id, IRemoteStreamContent file);

    Task DeleteAsync(Guid id);
}
