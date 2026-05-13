namespace App.Exceptions;

/// <summary>
/// Thrown when a shortened URL has expired or its TTL has elapsed. Maps to HTTP 410 Gone.
/// </summary>
public sealed class ExpiredUrlException : Exception
{
    public string ShortCode { get; }

    public ExpiredUrlException(string shortCode)
        : base($"The shortened URL '{shortCode}' has expired.")
    {
        ShortCode = shortCode;
    }
}
