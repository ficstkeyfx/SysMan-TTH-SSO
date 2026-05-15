using System;
using System.Security.Cryptography;
using System.Text;

namespace SysMan.Services
{
    public class HashingServices
    {
        public static string HashPassword(string password)
        {
            var bytes = Encoding.UTF8.GetBytes(password);
            using (SHA256 hash = SHA256.Create())
            {
                var passHash = hash.ComputeHash(bytes);
                StringBuilder builder = new StringBuilder();
                foreach (byte b in passHash)
                {
                    builder.Append(b.ToString("x2"));
                }

                return builder.ToString();
            }
        }
    }
}
