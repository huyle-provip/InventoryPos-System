using InventoryPos.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace InventoryPos.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class InventoryPosController : AbpControllerBase
{
    protected InventoryPosController()
    {
        LocalizationResource = typeof(InventoryPosResource);
    }
}
