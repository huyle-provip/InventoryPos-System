using Volo.Abp.Modularity;

namespace InventoryPos;

[DependsOn(
    typeof(InventoryPosApplicationModule),
    typeof(InventoryPosDomainTestModule)
)]
public class InventoryPosApplicationTestModule : AbpModule
{

}
