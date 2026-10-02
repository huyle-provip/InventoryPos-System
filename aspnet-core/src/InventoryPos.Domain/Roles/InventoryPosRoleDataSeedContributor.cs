using System;
using System.Threading.Tasks;
using InventoryPos.Permissions;
using Microsoft.AspNetCore.Identity;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Uow;

namespace InventoryPos.Roles;

/* Seeds two demo roles with a matching demo user each, so role-based access
 * can be shown without manual setup. The admin role keeps full access (ABP seeds it).
 */
public class InventoryPosRoleDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    public const string CashierRole = "Cashier";
    public const string WarehouseRole = "Warehouse";
    private const string DemoPassword = "1q2w3E*";

    private readonly IIdentityRoleRepository _roleRepository;
    private readonly IIdentityUserRepository _userRepository;
    private readonly IdentityRoleManager _roleManager;
    private readonly IdentityUserManager _userManager;
    private readonly ILookupNormalizer _lookupNormalizer;
    private readonly IPermissionDataSeeder _permissionDataSeeder;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentTenant _currentTenant;

    public InventoryPosRoleDataSeedContributor(
        IIdentityRoleRepository roleRepository,
        IIdentityUserRepository userRepository,
        IdentityRoleManager roleManager,
        IdentityUserManager userManager,
        ILookupNormalizer lookupNormalizer,
        IPermissionDataSeeder permissionDataSeeder,
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant)
    {
        _roleRepository = roleRepository;
        _userRepository = userRepository;
        _roleManager = roleManager;
        _userManager = userManager;
        _lookupNormalizer = lookupNormalizer;
        _permissionDataSeeder = permissionDataSeeder;
        _guidGenerator = guidGenerator;
        _currentTenant = currentTenant;
    }

    [UnitOfWork]
    public virtual async Task SeedAsync(DataSeedContext context)
    {
        await SeedRoleWithUserAsync(
            context.TenantId,
            CashierRole,
            "cashier",
            new[]
            {
                InventoryPosPermissions.Products.Default,
                InventoryPosPermissions.Categories.Default,
                InventoryPosPermissions.SaleOrders.Default,
                InventoryPosPermissions.SaleOrders.Create,
            });

        await SeedRoleWithUserAsync(
            context.TenantId,
            WarehouseRole,
            "warehouse",
            new[]
            {
                InventoryPosPermissions.Products.Default,
                InventoryPosPermissions.Categories.Default,
                InventoryPosPermissions.StockTransactions.Default,
                InventoryPosPermissions.StockTransactions.Create,
                InventoryPosPermissions.Suppliers.Default,
                InventoryPosPermissions.PurchaseOrders.Default,
                InventoryPosPermissions.PurchaseOrders.Receive,
            });
    }

    private async Task SeedRoleWithUserAsync(Guid? tenantId, string roleName, string userName, string[] permissions)
    {
        using (_currentTenant.Change(tenantId))
        {
            var role = await _roleRepository.FindByNormalizedNameAsync(_lookupNormalizer.NormalizeName(roleName));
            if (role == null)
            {
                role = new IdentityRole(_guidGenerator.Create(), roleName, tenantId);
                (await _roleManager.CreateAsync(role)).CheckErrors();
            }

            await _permissionDataSeeder.SeedAsync(
                RolePermissionValueProvider.ProviderName,
                roleName,
                permissions,
                tenantId);

            var user = await _userRepository.FindByNormalizedUserNameAsync(_lookupNormalizer.NormalizeName(userName));
            if (user == null)
            {
                user = new IdentityUser(_guidGenerator.Create(), userName, $"{userName}@inventorypos.local", tenantId);
                (await _userManager.CreateAsync(user, DemoPassword)).CheckErrors();
                (await _userManager.AddToRoleAsync(user, roleName)).CheckErrors();
            }
        }
    }
}
