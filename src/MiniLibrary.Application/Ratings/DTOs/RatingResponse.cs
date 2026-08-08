namespace MiniLibrary.Application.Ratings.DTOs;

/// <summary>
/// DTO representing a rating/review in API responses.
/// </summary>
public sealed record RatingResponse(
    Guid Id,
    Guid BookId,
    Guid UserId,
    string UserName,
    int Score,
    string ReviewText,
    int UsefulVotes,
    DateTime CreatedAt,
    DateTime UpdatedAt);

/// <summary>
/// DTO for the current user's own ratings (includes book info).
/// </summary>
public sealed record MyRatingResponse(
    Guid Id,
    Guid BookId,
    string BookTitle,
    string BookAuthor,
    int Score,
    string ReviewText,
    int UsefulVotes,
    DateTime CreatedAt,
    DateTime UpdatedAt);

/// <summary>
/// DTO for community ratings feed (includes user and book info).
/// </summary>
public sealed record CommunityRatingResponse(
    Guid Id,
    Guid BookId,
    string BookTitle,
    string BookAuthor,
    Guid UserId,
    string UserName,
    int Score,
    string ReviewText,
    int UsefulVotes,
    DateTime CreatedAt);

/// <summary>
/// DTO for the create/update rating request body.
/// </summary>
public sealed record CreateOrUpdateRatingRequest(int Score, string ReviewText);
