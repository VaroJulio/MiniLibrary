using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;

namespace MiniLibrary.IntegrationTests;

[Collection("Integration")]
public class BookCrudTests
{
    private readonly CustomWebApplicationFactory _factory;

    public BookCrudTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Librarian_Can_Create_And_Retrieve_Book()
    {
        var client = _factory.CreateAuthenticatedClient("Librarian");
        // Using a valid ISBN-13: 9780132350884 (Clean Code)
        var createPayload = new StringContent(
            """{"title":"Integration Test Book","author":"Jane Author","isbn":"9780132350884","publishedYear":2023,"description":"A test book","category":"Technology"}""",
            Encoding.UTF8, "application/json");

        var createResponse = await client.PostAsync("/api/books", createPayload);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            because: await createResponse.Content.ReadAsStringAsync());

        var responseBody = await createResponse.Content.ReadAsStringAsync();
        responseBody.Should().NotBeNullOrEmpty();

        var created = JsonDocument.Parse(responseBody);
        created.RootElement.TryGetProperty("id", out var idProp).Should().BeTrue();
        var bookId = idProp.GetString();
        bookId.Should().NotBeNullOrEmpty();

        // Retrieve
        var getResponse = await client.GetAsync($"/api/books/{bookId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var bookJson = await getResponse.Content.ReadAsStringAsync();
        var book = JsonDocument.Parse(bookJson);
        book.RootElement.GetProperty("title").GetString().Should().Be("Integration Test Book");
    }

    [Fact]
    public async Task Delete_Book_Soft_Deletes()
    {
        var client = _factory.CreateAuthenticatedClient("Librarian");
        // Valid ISBN-13: 9780135957059 (Pragmatic Programmer)
        var createPayload = new StringContent(
            """{"title":"Deletable Book","author":"Delete Author","isbn":"9780135957059","publishedYear":2022,"description":"Will be deleted","category":"Mystery"}""",
            Encoding.UTF8, "application/json");
        var createResponse = await client.PostAsync("/api/books", createPayload);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            because: await createResponse.Content.ReadAsStringAsync());

        var responseBody = await createResponse.Content.ReadAsStringAsync();
        var created = JsonDocument.Parse(responseBody);
        var bookId = created.RootElement.GetProperty("id").GetString();

        // Delete
        var deleteResponse = await client.DeleteAsync($"/api/books/{bookId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Should be 404 after soft-delete
        var getResponse = await client.GetAsync($"/api/books/{bookId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_Book_With_Empty_Title_Returns_Validation_Error()
    {
        var client = _factory.CreateAuthenticatedClient("Librarian");
        // Valid ISBN but empty title
        var invalidPayload = new StringContent(
            """{"title":"","author":"Some Author","isbn":"9780201633610","publishedYear":2020,"description":"","category":"Tech"}""",
            Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/books", invalidPayload);

        // ValidationBehavior returns 422 for validation errors
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Nonexistent_Book_Returns_404()
    {
        var client = _factory.CreateAuthenticatedClient("Member");
        var response = await client.GetAsync($"/api/books/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
