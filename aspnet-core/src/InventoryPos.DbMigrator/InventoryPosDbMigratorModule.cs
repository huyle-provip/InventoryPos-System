using InventoryPos.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace InventoryPos.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(InventoryPosEntityFrameworkCoreModule),
    typeof(InventoryPosApplicationContractsModule)
    )]
public class InventoryPosDbMigratorModule : AbpModule
{
}
