using MediatR;

namespace App.Abstractions
{
    /// <summary>
    /// Marker interface for domain events.
    /// Extends INotification so MediatR can dispatch them.
    /// </summary>
    public interface IDomainEvent : INotification
    {
        Guid Id { get; }
        DateTime OccurredOnUtc { get; }
    }
}
