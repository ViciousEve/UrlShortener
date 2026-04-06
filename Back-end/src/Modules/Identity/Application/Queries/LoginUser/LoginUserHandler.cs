using MediatR;
using Identity.Application.Contracts;
using Identity.Application.DTOs;

namespace Identity.Application.Queries.LoginUser
{
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
            return new AuthResponse { AccessToken = tokenResult.AccessToken, ExpiresAtUtc = tokenResult.ExpiresAtUtc };
        }
    }
}
