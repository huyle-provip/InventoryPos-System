using InventoryPos.Samples;
using Xunit;

namespace InventoryPos.EntityFrameworkCore.Domains;

[Collection(InventoryPosTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<InventoryPosEntityFrameworkCoreTestModule>
{

}
