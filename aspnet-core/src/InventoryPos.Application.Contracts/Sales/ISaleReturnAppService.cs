using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace InventoryPos.Sales;

public interface ISaleReturnAppService : IApplicationService
{
    Task<SaleReturnDto> CreateAsync(CreateSaleReturnDto input);

    Task<PagedResultDto<SaleReturnDto>> GetListAsync(GetSaleReturnListDto input);
}
