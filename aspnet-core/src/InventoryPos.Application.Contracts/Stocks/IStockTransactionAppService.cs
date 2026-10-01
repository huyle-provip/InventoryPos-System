using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace InventoryPos.Stocks;

public interface IStockTransactionAppService : IApplicationService
{
    Task<StockTransactionDto> CreateAsync(CreateStockTransactionDto input);

    Task<PagedResultDto<StockTransactionDto>> GetListAsync(GetStockTransactionListDto input);
}
