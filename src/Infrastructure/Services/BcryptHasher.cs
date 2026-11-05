using Application.Interfaces;

namespace Infrastructure.Services;

public class BcryptHasher : IHasher
{
    public string Hash(string plainText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plainText);
        return BCrypt.Net.BCrypt.HashPassword(plainText);
    }

    public bool Verify(string plainText, string hash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plainText);
        ArgumentException.ThrowIfNullOrWhiteSpace(hash);
        return BCrypt.Net.BCrypt.Verify(plainText, hash);
    }
}
