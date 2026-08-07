namespace MiniLibrary.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when a user attempts an action they are not permitted to perform.
/// Results in HTTP 403 Forbidden.
/// </summary>
public class ForbiddenException : Exception
{
    public ForbiddenException()
        : base("You do not have permission to perform this action.") { }

    public ForbiddenException(string message)
        : base(message) { }

    public ForbiddenException(string message, Exception innerException)
        : base(message, innerException) { }
}
