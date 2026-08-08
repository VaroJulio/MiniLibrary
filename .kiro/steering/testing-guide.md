---
inclusion: fileMatch
fileMatchPattern: "**/*Test*,**/*test*,**/*spec*"
---

# Testing Guide

## Overview
Testing is split into **Backend** (C#/.NET) and **Frontend** (TypeScript/React) strategies.
For detailed frontend testing strategy, see `frontend-testing.md` steering file.

## Running Tests

### Backend Unit Tests
```bash
cd tests/MiniLibrary.UnitTests
dotnet test
dotnet test --filter "Category=Unit"
```

### Backend Integration Tests
```bash
# Requires Docker running (TestContainers will spin up SQL Server)
cd tests/MiniLibrary.IntegrationTests
dotnet test
```

### Frontend Unit + Integration Tests
```bash
cd src/MiniLibrary.Web
npm run test -- --run     # Single run
npm run test:coverage     # With coverage
```

### Frontend E2E Tests (Playwright)
```bash
cd src/MiniLibrary.Web
npx playwright test                    # All viewports
npx playwright test --project=mobile   # Mobile only
npx playwright test --project=desktop  # Desktop only
npx playwright show-report             # View HTML report
```

### All Backend Tests
```bash
dotnet test MiniLibrary.sln
```

## Test Structure

### Backend Unit Tests (xUnit + Moq + FluentAssertions)
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

### Backend Integration Tests (TestContainers)
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

### Backend Property-Based Tests (FsCheck)
```csharp
[Property]
public Property BookTitle_ShouldNeverBeEmpty(NonEmptyString title)
{
    var book = Book.Create(title.Get, "Author", "ISBN");
    return (book.Title.Length > 0).ToProperty();
}
```

## Coverage Requirements

### Backend
- Minimum 80% line coverage for business logic
- All public API endpoints must have integration tests
- All domain invariants must have property-based tests

### Frontend
- See `frontend-testing.md` for detailed frontend coverage requirements
- Unit tests: > 80% of functions and hooks
- E2E: all critical flows covered in all viewports
- Visual regression: all screens in all standard viewports
