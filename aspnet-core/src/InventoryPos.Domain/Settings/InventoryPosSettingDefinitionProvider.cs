using Volo.Abp.Settings;

namespace InventoryPos.Settings;

public class InventoryPosSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(new SettingDefinition(InventoryPosSettings.TaxRatePercent, "10", isVisibleToClients: false));
    }
}
