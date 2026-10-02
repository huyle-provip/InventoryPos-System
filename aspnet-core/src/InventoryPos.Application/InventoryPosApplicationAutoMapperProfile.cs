using AutoMapper;
using InventoryPos.Catalog;
using InventoryPos.Purchasing;
using InventoryPos.Sales;
using InventoryPos.Stocks;

namespace InventoryPos;

public class InventoryPosApplicationAutoMapperProfile : Profile
{
    public InventoryPosApplicationAutoMapperProfile()
    {
        CreateMap<Category, CategoryDto>();
        CreateMap<CreateUpdateCategoryDto, Category>();

        CreateMap<Product, ProductDto>();
        CreateMap<CreateUpdateProductDto, Product>();

        CreateMap<StockTransaction, StockTransactionDto>();

        CreateMap<SaleOrder, SaleOrderDto>().ForMember(d => d.RefundedAmount, o => o.Ignore());
        CreateMap<SaleOrderItem, SaleOrderItemDto>();

        CreateMap<SaleReturn, SaleReturnDto>();
        CreateMap<SaleReturnItem, SaleReturnItemDto>();

        CreateMap<Supplier, SupplierDto>();
        CreateMap<CreateUpdateSupplierDto, Supplier>();

        CreateMap<PurchaseOrder, PurchaseOrderDto>().ForMember(d => d.SupplierName, o => o.Ignore());
        CreateMap<PurchaseOrderItem, PurchaseOrderItemDto>()
            .ForMember(d => d.ProductName, o => o.Ignore())
            .ForMember(d => d.Sku, o => o.Ignore());
    }
}
