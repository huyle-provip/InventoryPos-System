using Xunit;

namespace InventoryPos.EntityFrameworkCore;

[CollectionDefinition(InventoryPosTestConsts.CollectionDefinitionName)]
public class InventoryPosEntityFrameworkCoreCollection : ICollectionFixture<InventoryPosEntityFrameworkCoreFixture>
{

}
