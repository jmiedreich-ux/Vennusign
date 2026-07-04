using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Vennu.Data.Models;

namespace Vennu.Api.Tests.Models;

[Trait("Category", "Unit")]
public class EntityTableMappingTests
{
    [Theory]
    [InlineData(typeof(Venue), "Venues")]
    [InlineData(typeof(Screen), "Screens")]
    [InlineData(typeof(ScreenPairingCode), "ScreenPairingCodes")]
    public void PersistedEntities_HaveExpectedTableMappings(Type entityType, string expectedTableName)
    {
        var tableAttribute = entityType.GetCustomAttribute<TableAttribute>();

        Assert.NotNull(tableAttribute);
        Assert.Equal(expectedTableName, tableAttribute!.Name);
    }
}
