using AutoMapper;
using InventoryPos.Catalog;
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

        CreateMap<SaleOrder, SaleOrderDto>();
        CreateMap<SaleOrderItem, SaleOrderItemDto>();
    }
}
