using System;
using System.Security.Cryptography;
using System.Text;

namespace ExhibitionEntrySystem
{
    public static class PasswordHelper
    {
        public static string Hash(string input)
        {
            using var sha = SHA256.Create();
            return Convert.ToBase64String(
                sha.ComputeHash(Encoding.UTF8.GetBytes(input))
            );
        }
    }
}
