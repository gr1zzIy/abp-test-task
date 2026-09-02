namespace Integration.Tests.Infrastructure;

[CollectionDefinition("Integration tests")]
public sealed class IntegrationTestCollection
    : ICollectionFixture<IntegrationTestFixture>
{
    public const string Name = "Integration tests";
}