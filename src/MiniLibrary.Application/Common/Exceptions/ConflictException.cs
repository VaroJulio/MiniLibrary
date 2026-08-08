namespace MiniLibrary.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when a business rule is violated, resulting in a conflict.
/// </summary>
public class ConflictException : Exception
{
    public ConflictException()
        : base("A conflict occurred due to a business rule violation.")
    {
    }

    public ConflictException(string message)
        : base(message)
    {
    }

    public ConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
