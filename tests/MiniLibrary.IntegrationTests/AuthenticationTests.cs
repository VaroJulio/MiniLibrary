using System.Net;
using FluentAssertions;

namespace MiniLibrary.IntegrationTests;

[Collection("Integration")]
public class AuthenticationTests
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthenticationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Unauthenticated_Request_Returns_401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/books/00000000-0000-0000-0000-000000000001");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Authenticated_Member_Can_Access_Endpoints()
    {
        var client = _factory.CreateAuthenticatedClient("Member");

        var response = await client.GetAsync("/api/search/books?page=1&pageSize=10");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Member_Cannot_Access_Admin_Endpoints()
    {
        var client = _factory.CreateAuthenticatedClient("Member");

        var response = await client.GetAsync("/api/users");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Admin_Can_Access_Admin_Endpoints()
    {
        var client = _factory.CreateAuthenticatedClient("Admin");

        var response = await client.GetAsync("/api/users?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Member_Cannot_Create_Book()
    {
        var client = _factory.CreateAuthenticatedClient("Member");
        var content = new StringContent(
            """{"title":"Test","author":"Author","isbn":"1234567890123","category":"Test","description":"","publicationYear":2020}""",
            System.Text.Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/books", content);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Librarian_Can_Create_Book()
    {
        var client = _factory.CreateAuthenticatedClient("Librarian");
        // Valid ISBN-13: 9780553380163 (A Brief History of Time)
        var content = new StringContent(
            """{"title":"Auth Test Book","author":"Test Author","isbn":"9780553380163","category":"Testing","description":"A test book","publishedYear":2024}""",
            System.Text.Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/books", content);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
