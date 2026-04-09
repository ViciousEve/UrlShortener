using App.Abstractions;

namespace Identity.Domain.Events
{
    // Domain event raised when a new user is created.
    public sealed record UserCreatedDomainEvent : IDomainEvent
    {
        public Guid Id { get; }
        public DateTime OccurredOnUtc { get; }

        public Guid UserId { get; }

        public string Email { get; }

        public string Username { get; }

        public UserCreatedDomainEvent(Guid userId, string email, string username)
        {
            Id = Guid.CreateVersion7();
            OccurredOnUtc = DateTime.UtcNow;
            UserId = userId;
            Email = email;
            Username = username;
        }
    }
}
