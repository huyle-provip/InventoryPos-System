using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace InventoryPos.Sales;

public interface ISaleOrderAppService : IApplicationService
{
    Task<SaleOrderDto> CreateAsync(CreateSaleOrderDto input);

    Task<PagedResultDto<SaleOrderDto>> GetListAsync(PagedAndSortedResultRequestDto input);
}
