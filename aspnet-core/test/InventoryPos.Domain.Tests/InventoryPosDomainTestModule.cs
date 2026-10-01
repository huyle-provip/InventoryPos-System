using Volo.Abp.Modularity;

namespace InventoryPos;

[DependsOn(
    typeof(InventoryPosDomainModule),
    typeof(InventoryPosTestBaseModule)
)]
public class InventoryPosDomainTestModule : AbpModule
{

}
