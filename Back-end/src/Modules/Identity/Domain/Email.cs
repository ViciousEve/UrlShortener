using System.Text.RegularExpressions;

namespace Identity.Domain
{
    /// <summary>
    /// Value object for validated email addresses.
    /// Same pattern as ShortCode in the Shortening module.
    /// 
    /// IMPLEMENTATION NOTES:
    /// 
    /// 1. Value objects are immutable — once created, they cannot change.
    ///    This is why Value has only a getter, set through the constructor.
    ///    
    /// 3. EF Core will use a value conversion (in UserConfiguration.cs) to store 
    ///    this as a plain string in the database. The conversion maps:
    ///    - Write: Email → Email.Value (string)
    ///    - Read:  string → new Email(string)
    /// </summary>
    public class Email
    {
        public string Value { get; }

        public Email(string email)
        {
            if(!IsValidEmail(email))
            {
                throw new ArgumentException("Invalid email address");
            }
            Value = email.Trim().ToLowerInvariant();
        }

        private static bool IsValidEmail(string email) 
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }
            //use regex
            if(Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                return true;
            }
            return false;
        }

        public override bool Equals(object? obj)
        {
            return obj is Email email &&
                   Value == email.Value;
        }
    }
}
