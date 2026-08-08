using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace MiniLibrary.IntegrationTests;

public class PaginationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PaginationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateAuthenticatedClient("Member");
    }

    [Fact]
    public async Task Search_Returns_Paginated_Response_With_Metadata()
    {
        var response = await _client.GetAsync("/api/search/books?page=1&pageSize=5");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);

        doc.RootElement.TryGetProperty("data", out _).Should().BeTrue();
        doc.RootElement.TryGetProperty("pagination", out var pagination).Should().BeTrue();
        pagination.TryGetProperty("currentPage", out _).Should().BeTrue();
        pagination.TryGetProperty("pageSize", out _).Should().BeTrue();
        pagination.TryGetProperty("totalCount", out _).Should().BeTrue();
        pagination.TryGetProperty("totalPages", out _).Should().BeTrue();
        pagination.TryGetProperty("hasNext", out _).Should().BeTrue();
        pagination.TryGetProperty("hasPrevious", out _).Should().BeTrue();
    }

    [Fact]
    public async Task Search_With_Invalid_Page_Returns_400()
    {
        var response = await _client.GetAsync("/api/search/books?page=0&pageSize=5");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Search_With_Empty_Results_Returns_200_With_Empty_Data()
    {
        var response = await _client.GetAsync("/api/search/books?page=999&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("data").GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task Response_Uses_CamelCase_Property_Names()
    {
        var response = await _client.GetAsync("/api/search/books?page=1&pageSize=5");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        // Verify camelCase: "currentPage" not "CurrentPage"
        json.Should().Contain("currentPage");
        json.Should().NotContain("CurrentPage");
    }
}
