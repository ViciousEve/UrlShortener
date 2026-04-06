using App.Abstractions;
using Identity.Domain.Events;

namespace Identity.Domain
{
    /// <summary>
    /// Aggregate root representing a registered user.
    /// Inherits from Entity to support domain events.
    /// 
    /// IMPLEMENTATION NOTES:
    /// 
    /// 1. All setters are private — state changes only happen through
    ///    the constructor (for creation) or explicit methods (for updates).
    ///    This protects invariants and ensures events are raised properly.
    /// 
    /// 2. The constructor should:
    ///    - Validate all inputs (throw ArgumentException for invalid data)
    ///    - Set all properties
    ///    - Raise a UserCreatedDomainEvent
    ///    
    /// 3. PasswordHash is stored here, but hashing happens in the Application layer
    ///    via IPasswordHasher. The domain never sees raw passwords.
    ///    
    /// 4. The private parameterless constructor is required by EF Core
    ///    for materialization (reading from DB). EF uses reflection to call it.
    /// </summary>
    public class User : Entity
    {
        public Guid Id { get; private set; }

        public Email Email { get; private set; }

        public string PasswordHash { get; private set; }

        public string Username { get; private set; }

        public DateTime CreatedAtUtc { get; private set; }

        /// <summary>
        /// Soft-delete / account deactivation flag.
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// User's role for authorization. Defaults to Role.User.
        /// </summary>
        public Role Role { get; private set; }

        //User rules
        public const int MinUsernameLength = 3;
        public const int MaxUsernameLength = 50;
        public const int MinPasswordLength = 8;
        public const int MaxPasswordLength = 128;

        // Required by EF Core for materialization
        private User() { }

        public User(Email email, string username, string passwordHash)
        {
            if(!IsValidUser(email, username, passwordHash))
            throw new ArgumentException("Invalid user data");
            Id = Guid.CreateVersion7();
            Email = email;
            Username = username;
            PasswordHash = passwordHash;
            CreatedAtUtc = DateTime.UtcNow;
            IsActive = true;
            Role = Role.User;
            Raise(new UserCreatedDomainEvent(Id, email.Value, username));
        }

        public void Deactivate()
        {
            if(!IsActive)
            throw new InvalidOperationException("User is already deactivated");
            IsActive = false;
            //Raise(new UserDeactivatedDomainEvent(Id));
        }

        private static bool IsValidUser(Email email, string username, string passwordHash)
        {
            if(email == null)
            {
                return false;
            }
            if(string.IsNullOrWhiteSpace(username))
            {
                return false;
            }
            if(string.IsNullOrWhiteSpace(passwordHash))
            {
                return false;
            }
            if(username.Length < MinUsernameLength || username.Length > MaxUsernameLength)
            {
                return false;
            }
            if(passwordHash.Length < MinPasswordLength || passwordHash.Length > MaxPasswordLength)
            {
                return false;
            }
            return true;
        }
    }
}
