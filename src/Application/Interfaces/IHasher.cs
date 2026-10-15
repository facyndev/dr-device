namespace Application.Interfaces;

public interface IHasher
{
    string Hash(string plainText);
    bool Verify(string plainText, string hash);
}
