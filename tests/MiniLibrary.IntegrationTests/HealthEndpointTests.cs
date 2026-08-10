using System.Net;
using FluentAssertions;

namespace MiniLibrary.IntegrationTests;

[Collection("Integration")]
public class HealthEndpointTests
{
    private readonly HttpClient _client;

    public HealthEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_Endpoint_Returns_200_With_Status()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("healthy");
    }

    [Fact]
    public async Task All_Responses_Include_CorrelationId_Header()
    {
        var response = await _client.GetAsync("/health");

        response.Headers.Should().ContainKey("X-Correlation-Id");
        var correlationId = response.Headers.GetValues("X-Correlation-Id").First();
        correlationId.Should().NotBeNullOrEmpty();
        Guid.TryParse(correlationId, out _).Should().BeTrue();
    }
}
