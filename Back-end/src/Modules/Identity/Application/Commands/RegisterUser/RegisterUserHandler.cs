using MediatR;
using Identity.Application.Contracts;
using Identity.Application.DTOs;
using Identity.Domain;
using App.Exceptions;

namespace Identity.Application.Commands.RegisterUser
{
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
                throw new ConflictException("Email already registered");
            }
            var passwordHash = _passwordHasher.Hash(request.Password);
            var email = new Email(request.Email);
            var user = new User(email, request.Username, passwordHash);
            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();
            var tokenResult = _jwtProvider.GenerateToken(user);
            return new AuthResponse { AccessToken = tokenResult.AccessToken, ExpiresAtUtc = tokenResult.ExpiresAtUtc };
        }
    }
}
