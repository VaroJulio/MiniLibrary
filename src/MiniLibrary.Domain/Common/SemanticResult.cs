namespace MiniLibrary.Domain.Common;

/// <summary>
/// A single result from a semantic (vector) similarity search.
/// </summary>
public sealed record SemanticResult(Guid BookId, float Score);
