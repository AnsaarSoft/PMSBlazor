using System.Security.Cryptography;
using System.Text;

namespace PasswordManagement.Security;

public interface ICredentialProtector
{
    bool IsProtected(string value);
    bool IsLegacyProtected(string value);
    string Protect(string value);
    string Unprotect(string value);
}

public sealed class CredentialProtector : ICredentialProtector, IDisposable
{
    private const string Prefix = "pms:v2:";
    private const string LegacyPrefix = "pms:v1:";
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private static readonly byte[] AssociatedData = Encoding.UTF8.GetBytes("PasswordManagement.StoredCredentials.v2");

    private readonly byte[] encryptionKey;

    public CredentialProtector(IConfiguration configuration)
    {
        var encodedKey = configuration["VaultEncryption:Key"];
        if (string.IsNullOrWhiteSpace(encodedKey))
            throw new InvalidOperationException("VaultEncryption:Key is not configured.");

        try
        {
            encryptionKey = Convert.FromBase64String(encodedKey);
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException("VaultEncryption:Key must be a Base64-encoded 32-byte key.", ex);
        }

        if (encryptionKey.Length != 32)
            throw new InvalidOperationException("VaultEncryption:Key must decode to exactly 32 bytes.");
    }

    public bool IsProtected(string value) => value.StartsWith(Prefix, StringComparison.Ordinal);

    public bool IsLegacyProtected(string value) => value.StartsWith(LegacyPrefix, StringComparison.Ordinal);

    public string Protect(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (IsProtected(value))
            return value;

        var plaintext = Encoding.UTF8.GetBytes(value);
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[TagSize];

        try
        {
            using var aes = new AesGcm(encryptionKey, TagSize);
            aes.Encrypt(nonce, plaintext, ciphertext, tag, AssociatedData);

            return string.Concat(
                Prefix,
                Convert.ToBase64String(nonce), ".",
                Convert.ToBase64String(tag), ".",
                Convert.ToBase64String(ciphertext));
        }
        finally
        {
            CryptographicOperations.ZeroMemory(plaintext);
        }
    }

    public string Unprotect(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        if (IsLegacyProtected(value))
            throw new InvalidOperationException("This credential still uses legacy encryption and must be migrated.");

        if (!IsProtected(value))
            return value;

        var parts = value[Prefix.Length..].Split('.', StringSplitOptions.None);
        if (parts.Length != 3)
            throw new CryptographicException("The encrypted credential payload is invalid.");

        try
        {
            var nonce = Convert.FromBase64String(parts[0]);
            var tag = Convert.FromBase64String(parts[1]);
            var ciphertext = Convert.FromBase64String(parts[2]);

            if (nonce.Length != NonceSize || tag.Length != TagSize)
                throw new CryptographicException("The encrypted credential payload has invalid parameters.");

            var plaintext = new byte[ciphertext.Length];
            try
            {
                using var aes = new AesGcm(encryptionKey, TagSize);
                aes.Decrypt(nonce, ciphertext, tag, plaintext, AssociatedData);
                return Encoding.UTF8.GetString(plaintext);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(plaintext);
            }
        }
        catch (FormatException ex)
        {
            throw new CryptographicException("The encrypted credential payload is invalid.", ex);
        }
    }

    public void Dispose()
    {
        CryptographicOperations.ZeroMemory(encryptionKey);
    }
}
