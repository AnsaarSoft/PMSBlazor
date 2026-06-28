using Microsoft.AspNetCore.Identity;
using PMSModels.Models;
using System.Security.Cryptography;

namespace PasswordManagement.Security;

public interface IUserPasswordService
{
    string Hash(MstUser user, string password);
    PasswordVerificationResult Verify(MstUser user, string password);
    bool IsHashed(string value);
}

public sealed class UserPasswordService : IUserPasswordService
{
    private readonly PasswordHasher<MstUser> hasher = new();

    public string Hash(MstUser user, string password)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        return hasher.HashPassword(user, password);
    }

    public PasswordVerificationResult Verify(MstUser user, string password)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (!IsHashed(user.Password))
        {
            return CryptographicOperations.FixedTimeEquals(
                System.Text.Encoding.UTF8.GetBytes(user.Password),
                System.Text.Encoding.UTF8.GetBytes(password))
                ? PasswordVerificationResult.SuccessRehashNeeded
                : PasswordVerificationResult.Failed;
        }

        return hasher.VerifyHashedPassword(user, user.Password, password);
    }

    public bool IsHashed(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        try
        {
            var decoded = Convert.FromBase64String(value);
            return decoded.Length > 0 && decoded[0] is 0x00 or 0x01;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
