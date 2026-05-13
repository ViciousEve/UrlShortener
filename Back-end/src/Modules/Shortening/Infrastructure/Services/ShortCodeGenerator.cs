using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shortening.Application.Contracts;
using Shortening.Domain;

namespace Shortening.Infrastructure.Services
{
    public class ShortCodeGenerator : IShortCodeGenerator
    {
        private const int MAX_CODE_LENGTH = ShortCode.RequiredLength;
        public string GenerateShortCode()
        {           
            // Generate a random short code using a combination of characters and numbers
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var shortCode = new char[MAX_CODE_LENGTH];
            for(int i = 0; i < MAX_CODE_LENGTH; i++)
            {
                shortCode[i] = chars[Random.Shared.Next(chars.Length)];
            }

            return new string(shortCode);
        }
    }
}
