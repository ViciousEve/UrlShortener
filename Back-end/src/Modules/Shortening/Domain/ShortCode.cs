using System;
using System.Text.RegularExpressions;

namespace Shortening.Domain
{
    public class ShortCode
    {
        public string Value { get; }

        public ShortCode(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Short code cannot be empty", nameof(value));
            if(Value.Length != 8)
                throw new ArgumentException("Short code must be 8 characters long.");
            if (!Regex.IsMatch(value, "^[a-zA-Z0-9]+$"))
            throw new ArgumentException("Short code contains invalid characters.");
            
            Value = value;
        }
    }

}