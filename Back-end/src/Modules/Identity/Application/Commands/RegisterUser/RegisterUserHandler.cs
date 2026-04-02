using MediatR;
using Identity.Application.Contracts;
using Identity.Application.DTOs;
using Identity.Domain;

namespace Identity.Application.Commands.RegisterUser
{
    /// <summary>
    /// Handles the RegisterUserCommand — orchestrates user registration.
    /// Same pattern as CreateShortenedUrlHandler in the Shortening module.
    /// 
    /// IMPLEMENTATION NOTES:
    /// 
    /// 1. Dependencies are injected via constructor (DI pattern).
    ///    The DI container resolves these from IdentityModule.cs registrations.
    ///    
    /// 2. The handler is the "use case" in Clean Architecture — it coordinates
    ///    between domain objects and infrastructure services, but contains
    ///    no business rules itself (those live in the User entity).
    /// </summary>
    public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, AuthResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtProvider _jwtProvider;

        public RegisterUserHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IJwtProvider jwtProvider)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtProvider = jwtProvider;
        }

        public async Task<AuthResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var exists = await _userRepository.ExistsByEmailAsync(request.Email);
            if(exists)
            {
                throw new InvalidOperationException("Email already registered");
            }
            var passwordHash = _passwordHasher.Hash(request.Password);
            var email = new Email(request.Email);
            var user = new User(email, request.Username, passwordHash);
            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();
            var tokenResult = _jwtProvider.GenerateToken(user);
            return new AuthResponse { Token = tokenResult.Token, ExpiresAtUtc = tokenResult.ExpiresAtUtc };
        }
    }
}
