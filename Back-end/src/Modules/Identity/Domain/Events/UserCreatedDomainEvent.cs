using App.Abstractions;

namespace Identity.Domain.Events
{
    /// <summary>
    /// Domain event raised when a new user is created.
    /// Follows the same pattern as UrlCreatedDomainEvent in the Shortening module.
    /// 
    /// IMPLEMENTATION NOTES:
    /// 
    /// 1. Implements IDomainEvent from the App.Abstractions project.
    ///    IDomainEvent requires: Guid Id, DateTime OccurredOnUtc.
    ///    
    /// 2. This event is raised inside User's constructor via Raise().
    ///    It will be dispatched after the DbContext saves changes 
    ///    (once you implement a domain event dispatcher).
    ///    
    /// 3. Other modules (like Analytics) can subscribe to this event
    ///    to react to new user registrations — that's the power of
    ///    the modular monolith pattern.
    ///    
    /// 4. Using 'sealed record' makes this immutable and gives you
    ///    value equality, ToString(), and deconstruction for free.
    /// </summary>
    public sealed record UserCreatedDomainEvent : IDomainEvent
    {
        public Guid Id { get; }
        public DateTime OccurredOnUtc { get; }

        public Guid UserId { get; }

        public string Email { get; }

        public string Username { get; }

        public UserCreatedDomainEvent(Guid userId, string email, string username)
        {
            Id = Guid.NewGuid();
            OccurredOnUtc = DateTime.UtcNow;
            UserId = userId;
            Email = email;
            Username = username;
        }
    }
}
