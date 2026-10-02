using InventoryPos.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace InventoryPos.Permissions;

public class InventoryPosPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(InventoryPosPermissions.GroupName, L("Permission:InventoryPos"));

        var categories = myGroup.AddPermission(InventoryPosPermissions.Categories.Default, L("Permission:Categories"));
        categories.AddChild(InventoryPosPermissions.Categories.Create, L("Permission:Create"));
        categories.AddChild(InventoryPosPermissions.Categories.Edit, L("Permission:Edit"));
        categories.AddChild(InventoryPosPermissions.Categories.Delete, L("Permission:Delete"));

        var products = myGroup.AddPermission(InventoryPosPermissions.Products.Default, L("Permission:Products"));
        products.AddChild(InventoryPosPermissions.Products.Create, L("Permission:Create"));
        products.AddChild(InventoryPosPermissions.Products.Edit, L("Permission:Edit"));
        products.AddChild(InventoryPosPermissions.Products.Delete, L("Permission:Delete"));

        var stockTransactions = myGroup.AddPermission(InventoryPosPermissions.StockTransactions.Default, L("Permission:StockTransactions"));
        stockTransactions.AddChild(InventoryPosPermissions.StockTransactions.Create, L("Permission:Create"));

        var saleOrders = myGroup.AddPermission(InventoryPosPermissions.SaleOrders.Default, L("Permission:SaleOrders"));
        saleOrders.AddChild(InventoryPosPermissions.SaleOrders.Create, L("Permission:Create"));

        var saleReturns = myGroup.AddPermission(InventoryPosPermissions.SaleReturns.Default, L("Permission:SaleReturns"));
        saleReturns.AddChild(InventoryPosPermissions.SaleReturns.Create, L("Permission:Create"));

        var suppliers = myGroup.AddPermission(InventoryPosPermissions.Suppliers.Default, L("Permission:Suppliers"));
        suppliers.AddChild(InventoryPosPermissions.Suppliers.Create, L("Permission:Create"));
        suppliers.AddChild(InventoryPosPermissions.Suppliers.Edit, L("Permission:Edit"));
        suppliers.AddChild(InventoryPosPermissions.Suppliers.Delete, L("Permission:Delete"));

        var purchaseOrders = myGroup.AddPermission(InventoryPosPermissions.PurchaseOrders.Default, L("Permission:PurchaseOrders"));
        purchaseOrders.AddChild(InventoryPosPermissions.PurchaseOrders.Create, L("Permission:Create"));
        purchaseOrders.AddChild(InventoryPosPermissions.PurchaseOrders.Receive, L("Permission:Receive"));
        purchaseOrders.AddChild(InventoryPosPermissions.PurchaseOrders.Cancel, L("Permission:Cancel"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<InventoryPosResource>(name);
    }
}
