using System.Security.Cryptography;
using System.Text;
using Base.UserManagement.Domain.Services.Interfaces;

namespace Base.UserManagement.Domain.Services;

public class TokenGeneratorService : ITokenGeneratorService
{
    private const string AllowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    
    public string GenerateVerificationToken(string email, int length = 6)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));
        
        if (length < 4 || length > 10)
            throw new ArgumentException("Token length must be between 4 and 10 characters", nameof(length));

        // Use email as seed for consistent generation (optional - for testing purposes)
        var emailHash = GetEmailHash(email);
        var random = new Random(emailHash);
        
        // Generate token with mixed alphanumeric characters
        var token = new StringBuilder();
        for (int i = 0; i < length; i++)
        {
            // Add some randomness based on current time and email
            var timeComponent = (int)(DateTime.UtcNow.Ticks % AllowedChars.Length);
            var index = (random.Next(AllowedChars.Length) + timeComponent + i) % AllowedChars.Length;
            token.Append(AllowedChars[index]);
        }
        
        return token.ToString();
    }

    public string GenerateSecureRandomToken(int length = 32)
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[length];
        rng.GetBytes(bytes);
        var base64 = Convert.ToBase64String(bytes).Replace("+", "").Replace("/", "").Replace("=", "");
        return base64[..Math.Min(length, base64.Length)];
    }

    private int GetEmailHash(string email)
    {
        // Simple hash function for email
        int hash = 0;
        foreach (char c in email.ToLowerInvariant())
        {
            hash = (hash * 31 + c) % int.MaxValue;
        }
        return Math.Abs(hash);
    }
}
