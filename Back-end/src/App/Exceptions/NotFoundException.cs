namespace App.Exceptions;

/// <summary>
/// Thrown when a requested entity is not found. Maps to HTTP 404.
/// </summary>
public sealed class NotFoundException : Exception
{
    public string EntityName { get; }
    public object Key { get; }

    public NotFoundException(string entityName, object key)
        : base($"{entityName} with key '{key}' was not found.")
    {
        EntityName = entityName;
        Key = key;
    }
}
