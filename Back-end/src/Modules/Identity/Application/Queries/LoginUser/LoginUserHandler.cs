using MediatR;
using Identity.Application.Contracts;
using Identity.Application.DTOs;

namespace Identity.Application.Queries.LoginUser
{
    /// <summary>
    /// Handles the LoginUserQuery — orchestrates user authentication.
    /// 
    /// IMPLEMENTATION NOTES:
    /// 
    /// 1. This handler's job:
    ///    - Look up the user by email
    ///    - Verify the password
    ///    - Generate and return a JWT token
    ///    
    /// 2. Security considerations:
    ///    - NEVER reveal whether the email exists or the password is wrong.
    ///      Always use a generic message like "Invalid email or password"
    ///      to prevent user enumeration attacks.
    ///    - Consider adding rate limiting at the API level (not here).
    /// </summary>
    public class LoginUserHandler : IRequestHandler<LoginUserQuery, AuthResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtProvider _jwtProvider;

        public LoginUserHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IJwtProvider jwtProvider)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtProvider = jwtProvider;
        }

        public async Task<AuthResponse> Handle(LoginUserQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if(user is null)
            {
                throw new InvalidOperationException("Invalid email or password");
            }
            var isValid = _passwordHasher.Verify(request.Password, user.PasswordHash);
            if(!isValid)
            {
                throw new InvalidOperationException("Invalid email or password");
            }
            var tokenResult = _jwtProvider.GenerateToken(user);
            return new AuthResponse { Token = tokenResult.Token, ExpiresAtUtc = tokenResult.ExpiresAtUtc };
        }
    }
}
