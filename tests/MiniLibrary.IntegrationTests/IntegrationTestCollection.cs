namespace MiniLibrary.IntegrationTests;

/// <summary>
/// xUnit collection definition that shares a single CustomWebApplicationFactory
/// (and therefore a single SQL Server container) across ALL integration test classes.
/// 
/// This avoids spinning up a new container per test class, keeping total test time
/// under 2 minutes while still testing against real SQL Server.
/// 
/// All test classes that use [Collection("Integration")] will share this factory instance.
/// </summary>
[CollectionDefinition("Integration")]
public class IntegrationTestCollection : ICollectionFixture<CustomWebApplicationFactory>
{
}
