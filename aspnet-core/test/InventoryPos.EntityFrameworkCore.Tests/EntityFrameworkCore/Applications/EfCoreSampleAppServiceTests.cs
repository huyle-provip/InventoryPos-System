using InventoryPos.Samples;
using Xunit;

namespace InventoryPos.EntityFrameworkCore.Applications;

[Collection(InventoryPosTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<InventoryPosEntityFrameworkCoreTestModule>
{

}
