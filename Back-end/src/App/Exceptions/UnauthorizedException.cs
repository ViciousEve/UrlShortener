namespace App.Exceptions;

/// <summary>
/// Thrown when authentication fails or is missing. Maps to HTTP 401 Unauthorized.
/// </summary>
public sealed class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) 
        : base(message)
    {
    }
}
