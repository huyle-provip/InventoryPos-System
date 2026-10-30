using InventoryPos.Sales;
using Xunit;

namespace InventoryPos.EntityFrameworkCore.Applications;

[Collection(InventoryPosTestConsts.CollectionDefinitionName)]
public class EfCoreSaleOrderAppServiceTests : SaleOrderAppServiceTests<InventoryPosEntityFrameworkCoreTestModule>
{
}
