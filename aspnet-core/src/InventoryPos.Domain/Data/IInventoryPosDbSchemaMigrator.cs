using System.Threading.Tasks;

namespace InventoryPos.Data;

public interface IInventoryPosDbSchemaMigrator
{
    Task MigrateAsync();
}
