using LiveExpert.Application.Interfaces;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace LiveExpert.Infrastructure.Services;

/// <summary>
/// Service for encrypting/decrypting sensitive data at rest
/// Uses AES-256 encryption
/// </summary>
public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;
    private readonly byte[] _iv;

    public EncryptionService(IConfiguration configuration)
    {
        // Get encryption key from configuration or generate one
        var encryptionKey = configuration["Encryption:Key"] 
            ?? throw new InvalidOperationException("Encryption key not configured");
        
        // Derive key and IV from the encryption key
        using var sha256 = SHA256.Create();
        _key = sha256.ComputeHash(Encoding.UTF8.GetBytes(encryptionKey));
        
        using var md5 = MD5.Create();
        _iv = md5.ComputeHash(Encoding.UTF8.GetBytes(encryptionKey));
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);

        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        {
            cs.Write(plainBytes, 0, plainBytes.Length);
        }

        return Convert.ToBase64String(ms.ToArray());
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return string.Empty;

        try
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            var decryptor = aes.CreateDecryptor();
            var cipherBytes = Convert.FromBase64String(cipherText);

            using var ms = new MemoryStream(cipherBytes);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);

            return sr.ReadToEnd();
        }
        catch
        {
            // Return empty string if decryption fails
            return string.Empty;
        }
    }

    /// <summary>
    /// Hash a password using BCrypt (for password storage)
    /// </summary>
    public string Hash(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        // Use BCrypt for password hashing (matches DbInitializer)
        return BCrypt.Net.BCrypt.HashPassword(text);
    }

    /// <summary>
    /// Verify a password against a BCrypt hash
    /// </summary>
    public bool VerifyHash(string text, string hash)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(hash))
            return false;

        try
        {
            // Use BCrypt for password verification (matches DbInitializer)
            // BCrypt.Net.BCrypt.Verify returns true if password matches hash
            return BCrypt.Net.BCrypt.Verify(text, hash);
        }
        catch (Exception ex)
        {
            // If BCrypt verification fails (e.g., invalid hash format), return false
            System.Diagnostics.Debug.WriteLine($"BCrypt verification failed: {ex.Message}");
            return false;
        }
    }
}
