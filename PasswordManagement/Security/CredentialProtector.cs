using Microsoft.AspNetCore.DataProtection;

namespace PasswordManagement.Security;

public interface ICredentialProtector
{
    bool IsProtected(string value);
    string Protect(string value);
    string Unprotect(string value);
}

public sealed class CredentialProtector : ICredentialProtector
{
    private const string Prefix = "pms:v1:";
    private readonly IDataProtector protector;

    public CredentialProtector(IDataProtectionProvider provider)
    {
        protector = provider.CreateProtector("PasswordManagement.StoredCredentials.v1");
    }

    public bool IsProtected(string value) => value.StartsWith(Prefix, StringComparison.Ordinal);

    public string Protect(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return IsProtected(value) ? value : Prefix + protector.Protect(value);
    }

    public string Unprotect(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return IsProtected(value) ? protector.Unprotect(value[Prefix.Length..]) : value;
    }
}
