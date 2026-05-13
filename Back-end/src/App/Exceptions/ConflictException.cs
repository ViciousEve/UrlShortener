namespace App.Exceptions;

/// <summary>
/// Thrown when a request conflicts with the current state of the server. Maps to HTTP 409 Conflict.
/// </summary>
public sealed class ConflictException : Exception
{
    public ConflictException(string message)
        : base(message)
    {
    }
}
