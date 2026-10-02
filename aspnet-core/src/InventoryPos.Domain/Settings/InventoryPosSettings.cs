namespace InventoryPos.Settings;

public static class InventoryPosSettings
{
    private const string Prefix = "InventoryPos";

    /// <summary>Sales tax percent applied to every sale after discount. Override in appsettings.json under "Settings".</summary>
    public const string TaxRatePercent = Prefix + ".TaxRatePercent";
}
