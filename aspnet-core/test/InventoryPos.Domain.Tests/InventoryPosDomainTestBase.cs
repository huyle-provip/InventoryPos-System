using Volo.Abp.Modularity;

namespace InventoryPos;

/* Inherit from this class for your domain layer tests. */
public abstract class InventoryPosDomainTestBase<TStartupModule> : InventoryPosTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
