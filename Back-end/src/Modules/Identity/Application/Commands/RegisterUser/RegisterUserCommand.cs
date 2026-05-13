using MediatR;
using Identity.Application.DTOs;

namespace Identity.Application.Commands.RegisterUser
{
    /// <summary>
    /// MediatR command for user registration.
    /// Same pattern as CreateShortenedUrlCommand in the Shortening module.
    /// 
    /// IMPLEMENTATION NOTES:
    /// 
    /// 1. This is a record (immutable) — once created, values can't change.
    ///    Records get value equality, deconstruction, and ToString() for free.
    ///    
    /// 2. Implements IRequest<AuthResponse> meaning:
    ///    - MediatR will route this to RegisterUserHandler
    ///    - The handler must return an AuthResponse (contains the JWT token)
    ///    
    /// 3. This carries RAW password from the endpoint to the handler.
    ///    The handler will hash it via IPasswordHasher before storing.
    ///    The password never leaves the Application layer in plain text.
    ///    
    /// 4. Validation happens BEFORE the handler runs, via RegisterUserValidator
    ///    and the ValidationBehavior pipeline (already set up in App project).
    /// </summary>
    public record RegisterUserCommand(
        string Email,
        string Username,
        string Password
    ) : IRequest<AuthResponse>;
}
