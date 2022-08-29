using Xunit;

namespace Hotel.Tests;

[CollectionDefinition("Api")]
public class ApiCollection : ICollectionFixture<ApiFixture>
{
}
