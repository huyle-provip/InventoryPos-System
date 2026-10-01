using System;
using System.Collections.Generic;
using System.Text;
using InventoryPos.Localization;
using Volo.Abp.Application.Services;

namespace InventoryPos;

/* Inherit your application services from this class.
 */
public abstract class InventoryPosAppService : ApplicationService
{
    protected InventoryPosAppService()
    {
        LocalizationResource = typeof(InventoryPosResource);
    }
}
