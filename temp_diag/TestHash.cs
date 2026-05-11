using System;
using System.Security.Cryptography;
using System.Text;

public class TestHash {
    public static void Main() {
        string pass = "Buss1ness.2026";
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(pass);
        var hash = sha.ComputeHash(bytes);
        var sb = new StringBuilder(hash.Length * 2);
        foreach (var b in hash)
            sb.Append(b.ToString("X2"));
        
        Console.WriteLine($"Password: {pass}");
        Console.WriteLine($"Hash (X2): {sb.ToString()}");
        
        string expected = "5166099B80693A74112E52E935A8BC762699E1AC74D4719B4CFB088E894C25F7";
        Console.WriteLine($"Match: {sb.ToString() == expected}");
    }
}
