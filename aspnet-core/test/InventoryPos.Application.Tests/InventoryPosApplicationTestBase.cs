using Volo.Abp.Modularity;

namespace InventoryPos;

public abstract class InventoryPosApplicationTestBase<TStartupModule> : InventoryPosTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
