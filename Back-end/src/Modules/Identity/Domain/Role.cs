namespace Identity.Domain
{
    /// <summary>
    /// Enum representing user roles for authorization.
    /// 
    /// IMPLEMENTATION NOTES:
    /// 
    /// 1. This is stored as a string in the database via EF Core's
    ///    HasConversion<string>() in UserConfiguration.cs — same pattern
    ///    as UrlStatus in the Shortening module.
    ///    
    /// 2. The JWT token should include the user's role as a claim,
    ///    so the API can use [Authorize(Roles = "Admin")] or policy-based
    ///    authorization on protected endpoints.
    ///    
    /// 3. You can extend this enum later (e.g., add Moderator, Premium)
    ///    without breaking existing data since it's stored as a string.
    /// </summary>
    public enum Role
    {
        User,
        Admin
    }
}
