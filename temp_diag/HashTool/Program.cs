using System;
using System.Security.Cryptography;
using System.Text;

namespace HashTool
{
    class Program
    {
        static void Main(string[] args)
        {
            string p1 = "Buss1ness.2026";
            string p2 = "Bussiness.2026";
            
            Console.WriteLine($"Hash de '{p1}': {GetHash(p1)}");
            Console.WriteLine($"Hash de '{p2}': {GetHash(p2)}");
        }

        static string GetHash(string input)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha.ComputeHash(bytes);
            var sb = new StringBuilder(hash.Length * 2);
            foreach (var b in hash)
                sb.Append(b.ToString("X2"));
            return sb.ToString();
        }
    }
}
