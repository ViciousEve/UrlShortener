using Identity.Application.Contracts;

namespace Identity.Infrastructure.Services
{
    /// <summary>
    /// Password hashing implementation using BCrypt.
    /// Implements IPasswordHasher from the Application layer.
    /// 
    /// IMPLEMENTATION NOTES:
    /// 
    /// 1. NuGet package needed: BCrypt.Net-Next
    ///    Add to Identity.csproj: <PackageReference Include="BCrypt.Net-Next" />
    ///    (and add version to Directory.Packages.props if using central package management)
    ///    
    /// 2. Why BCrypt?
    ///    - Designed specifically for passwords (unlike SHA256/MD5)
    ///    - Intentionally slow — makes brute-force attacks impractical
    ///    - Includes automatic salt generation — no manual salt management
    ///    - Work factor is configurable (default 11 is fine for most apps)
    ///    
    /// 3. Implementation is dead simple:
    ///    - Hash:   BCrypt.Net.BCrypt.HashPassword(password)
    ///    - Verify: BCrypt.Net.BCrypt.Verify(password, hash)
    ///    
    /// 4. Alternative: Use Microsoft's built-in PasswordHasher<T> from
    ///    Microsoft.AspNetCore.Identity, which uses PBKDF2.
    ///    Both are secure — BCrypt is more widely used in non-Microsoft ecosystems.
    /// </summary>
    public class PasswordHasher : IPasswordHasher
    {
        /// <summary>
        /// IMPLEMENT: Hash a raw password.
        /// return BCrypt.Net.BCrypt.HashPassword(password);
        /// </summary>
        public string Hash(string password)
        {
            // TODO: Implement
            throw new NotImplementedException();
        }

        /// <summary>
        /// IMPLEMENT: Verify a raw password against a stored hash.
        /// return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        /// </summary>
        public bool Verify(string password, string passwordHash)
        {
            // TODO: Implement
            throw new NotImplementedException();
        }
    }
}
