using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using InventoryPos.Data;
using Volo.Abp.DependencyInjection;

namespace InventoryPos.EntityFrameworkCore;

public class EntityFrameworkCoreInventoryPosDbSchemaMigrator
    : IInventoryPosDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreInventoryPosDbSchemaMigrator(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolve the InventoryPosDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<InventoryPosDbContext>()
            .Database
            .MigrateAsync();
    }
}
