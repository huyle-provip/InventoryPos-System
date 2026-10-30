using InventoryPos.Stocks;
using Xunit;

namespace InventoryPos.EntityFrameworkCore.Applications;

[Collection(InventoryPosTestConsts.CollectionDefinitionName)]
public class EfCoreStockTransactionAppServiceTests : StockTransactionAppServiceTests<InventoryPosEntityFrameworkCoreTestModule>
{
}
