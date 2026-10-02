using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace InventoryPos.Dashboard;

public interface IDashboardAppService : IApplicationService
{
    Task<DashboardDto> GetAsync();
}
