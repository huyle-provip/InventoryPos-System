namespace InventoryPos.Permissions;

public static class InventoryPosPermissions
{
    public const string GroupName = "InventoryPos";

    public static class Categories
    {
        public const string Default = GroupName + ".Categories";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class Products
    {
        public const string Default = GroupName + ".Products";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class StockTransactions
    {
        public const string Default = GroupName + ".StockTransactions";
        public const string Create = Default + ".Create";
    }

    public static class SaleOrders
    {
        public const string Default = GroupName + ".SaleOrders";
        public const string Create = Default + ".Create";
    }

    public static class SaleReturns
    {
        public const string Default = GroupName + ".SaleReturns";
        public const string Create = Default + ".Create";
    }

    public static class Suppliers
    {
        public const string Default = GroupName + ".Suppliers";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class PurchaseOrders
    {
        public const string Default = GroupName + ".PurchaseOrders";
        public const string Create = Default + ".Create";
        public const string Receive = Default + ".Receive";
        public const string Cancel = Default + ".Cancel";
    }
}
