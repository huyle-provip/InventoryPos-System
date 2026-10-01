using Microsoft.Extensions.Localization;
using InventoryPos.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace InventoryPos;

[Dependency(ReplaceServices = true)]
public class InventoryPosBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<InventoryPosResource> _localizer;

    public InventoryPosBrandingProvider(IStringLocalizer<InventoryPosResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
