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
            if (value.Length > 10)
                throw new ArgumentException("Short code cannot be longer than 10 characters", nameof(value));
            if (value.Length < 5)
                throw new ArgumentException("Short code cannot be shorter than 5 characters", nameof(value));
            if (!Regex.IsMatch(value, "^[a-zA-Z0-9]+$"))
            throw new ArgumentException("Short code contains invalid characters.");
            
            Value = value;
        }
    }

}