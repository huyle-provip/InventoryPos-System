using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace InventoryPos.Sales;

public interface ISaleOrderAppService : IApplicationService
{
    Task<SaleOrderDto> CreateAsync(CreateSaleOrderDto input);

    Task<SaleOrderDto> GetAsync(Guid id);

    Task<PagedResultDto<SaleOrderDto>> GetListAsync(PagedAndSortedResultRequestDto input);

    Task<PricingInfoDto> GetPricingInfoAsync();
}
