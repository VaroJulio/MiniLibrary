---
inclusion: fileMatch
fileMatchPattern: "**/*Test*,**/*test*,**/*spec*"
---

# Testing Guide

## Running Tests

### Unit Tests
```bash
cd tests/MiniLibrary.UnitTests
dotnet test
dotnet test --filter "Category=Unit"
```

### Integration Tests
```bash
# Requires Docker running (TestContainers will spin up SQL Server)
cd tests/MiniLibrary.IntegrationTests
dotnet test
```

### Frontend Tests
```bash
cd src/MiniLibrary.Web
npm test
npm run test:coverage
```

### All Tests
```bash
dotnet test MiniLibrary.sln
```

## Test Structure

### Unit Tests (xUnit + Moq + FluentAssertions)
```csharp
public class CreateBookCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateBook()
    {
        // Arrange
        var mockRepo = new Mock<IBookRepository>();
        var handler = new CreateBookCommandHandler(mockRepo.Object);
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.Should().BeSuccess();
    }
}
```

### Integration Tests (TestContainers)
```csharp
public class BooksControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task GetBooks_ReturnsOkWithBooks()
    {
        // Uses real SQL Server in container
        var response = await _client.GetAsync("/api/books");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

### Property-Based Tests (FsCheck)
```csharp
[Property]
public Property BookTitle_ShouldNeverBeEmpty(NonEmptyString title)
{
    var book = Book.Create(title.Get, "Author", "ISBN");
    return (book.Title.Length > 0).ToProperty();
}
```

## Coverage Requirements
- Minimum 80% line coverage for business logic
- All public API endpoints must have integration tests
- All domain invariants must have property-based tests
