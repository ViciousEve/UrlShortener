namespace App.Exceptions;

/// <summary>
/// Thrown when a user attempts an action they are not authorized for. Maps to HTTP 403 Forbidden.
/// </summary>
public sealed class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException()
        : base("You do not have permission to perform this action.")
    {
    }

    public ForbiddenAccessException(string message)
        : base(message)
    {
    }
}
