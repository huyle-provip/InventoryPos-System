using Microsoft.EntityFrameworkCore;
using InventoryPos.Catalog;
using InventoryPos.Purchasing;
using InventoryPos.Sales;
using InventoryPos.Stocks;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace InventoryPos.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class InventoryPosDbContext :
    AbpDbContext<InventoryPosDbContext>,
    IIdentityDbContext,
    ITenantManagementDbContext
{
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<StockTransaction> StockTransactions { get; set; }
    public DbSet<SaleOrder> SaleOrders { get; set; }
    public DbSet<SaleOrderItem> SaleOrderItems { get; set; }
    public DbSet<SaleReturn> SaleReturns { get; set; }
    public DbSet<SaleReturnItem> SaleReturnItems { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
    public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }

    #region Entities from the modules

    /* Notice: We only implemented IIdentityDbContext and ITenantManagementDbContext
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityDbContext and ITenantManagementDbContext.
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    //Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }
    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    #endregion

    public InventoryPosDbContext(DbContextOptions<InventoryPosDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureFeatureManagement();
        builder.ConfigureTenantManagement();

        /* Configure your own tables/entities inside here */

        builder.Entity<Category>(b =>
        {
            b.ToTable(InventoryPosConsts.DbTablePrefix + "Categories", InventoryPosConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
        });

        builder.Entity<Product>(b =>
        {
            b.ToTable(InventoryPosConsts.DbTablePrefix + "Products", InventoryPosConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Sku).IsRequired().HasMaxLength(64);
            b.Property(x => x.Name).IsRequired().HasMaxLength(256);
            b.Property(x => x.Price).HasColumnType("decimal(18,2)");
            b.HasIndex(x => x.Sku).IsUnique();
            b.HasOne<Category>().WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<StockTransaction>(b =>
        {
            b.ToTable(InventoryPosConsts.DbTablePrefix + "StockTransactions", InventoryPosConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Note).HasMaxLength(512);
            b.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<SaleOrder>(b =>
        {
            b.ToTable(InventoryPosConsts.DbTablePrefix + "SaleOrders", InventoryPosConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Subtotal).HasColumnType("decimal(18,2)");
            b.Property(x => x.DiscountValue).HasColumnType("decimal(18,2)");
            b.Property(x => x.DiscountAmount).HasColumnType("decimal(18,2)");
            b.Property(x => x.TaxRatePercent).HasColumnType("decimal(9,4)");
            b.Property(x => x.TaxAmount).HasColumnType("decimal(18,2)");
            b.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
            b.Property(x => x.AmountTendered).HasColumnType("decimal(18,2)");
            b.Property(x => x.PaymentMethod).HasDefaultValue(PaymentMethod.Cash);
            b.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.SaleOrderId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<SaleOrderItem>(b =>
        {
            b.ToTable(InventoryPosConsts.DbTablePrefix + "SaleOrderItems", InventoryPosConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
            b.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<SaleReturn>(b =>
        {
            b.ToTable(InventoryPosConsts.DbTablePrefix + "SaleReturns", InventoryPosConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Reason).HasMaxLength(512);
            b.Property(x => x.RefundAmount).HasColumnType("decimal(18,2)");
            b.HasOne<SaleOrder>().WithMany().HasForeignKey(x => x.SaleOrderId).OnDelete(DeleteBehavior.Restrict);
            b.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.SaleReturnId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<SaleReturnItem>(b =>
        {
            b.ToTable(InventoryPosConsts.DbTablePrefix + "SaleReturnItems", InventoryPosConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.RefundAmount).HasColumnType("decimal(18,2)");
            b.HasOne<SaleOrderItem>().WithMany().HasForeignKey(x => x.SaleOrderItemId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Supplier>(b =>
        {
            b.ToTable(InventoryPosConsts.DbTablePrefix + "Suppliers", InventoryPosConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.ContactName).HasMaxLength(128);
            b.Property(x => x.Phone).HasMaxLength(32);
            b.Property(x => x.Email).HasMaxLength(256);
        });

        builder.Entity<PurchaseOrder>(b =>
        {
            b.ToTable(InventoryPosConsts.DbTablePrefix + "PurchaseOrders", InventoryPosConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.OrderNumber).IsRequired().HasMaxLength(32);
            b.Property(x => x.Note).HasMaxLength(512);
            b.Property(x => x.TotalCost).HasColumnType("decimal(18,2)");
            b.HasIndex(x => x.OrderNumber).IsUnique();
            b.HasOne<Supplier>().WithMany().HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
            b.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.PurchaseOrderId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<PurchaseOrderItem>(b =>
        {
            b.ToTable(InventoryPosConsts.DbTablePrefix + "PurchaseOrderItems", InventoryPosConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.UnitCost).HasColumnType("decimal(18,2)");
            b.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ProductImage>(b =>
        {
            b.ToTable(InventoryPosConsts.DbTablePrefix + "ProductImages", InventoryPosConsts.DbSchema);
            b.Property(x => x.ContentType).IsRequired().HasMaxLength(64);
            b.Property(x => x.Data).IsRequired();
            b.HasOne<Product>().WithMany().HasForeignKey(x => x.Id).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
