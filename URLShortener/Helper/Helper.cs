using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace URLShortener
{
    public static class Helper
    {
        public static string GenerateRandomUrlName()
        {
            // brak kontroli nad zawartoscia generowanego ciagu
            // return Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 10);

            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[8];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new String(stringChars);
        }

        public static string GetUrlDomain(string url)
        {
            string[] parts = url.Replace("www.", "").Split('/');

            return parts[2];
        }
    }
}
