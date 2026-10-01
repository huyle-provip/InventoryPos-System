using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace InventoryPos.Data;

/* This is used if database provider does't define
 * IInventoryPosDbSchemaMigrator implementation.
 */
public class NullInventoryPosDbSchemaMigrator : IInventoryPosDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
