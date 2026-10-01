using Volo.Abp.Settings;

namespace InventoryPos.Settings;

public class InventoryPosSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(InventoryPosSettings.MySetting1));
    }
}
