using System.ComponentModel.DataAnnotations;

namespace PasswordManagement.Pages.Cards;

public sealed class AccountFormModel
{
    [Required(ErrorMessage = "Account type is required.")]
    [StringLength(50)]
    public string CardName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Account name is required.")]
    [StringLength(50)]
    public string Alias { get; set; } = string.Empty;

    [Required(ErrorMessage = "User code is required.")]
    [StringLength(50)]
    public string UserCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(50)]
    public string Password { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [WebAddress]
    [StringLength(300)]
    public string WebLink { get; set; } = string.Empty;

    [StringLength(500)]
    public string Remarks { get; set; } = string.Empty;
}

public static class AccountFormUtilities
{
    public static string NormalizeWebLink(string value)
    {
        var trimmed = value.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return string.Empty;

        return Uri.TryCreate(trimmed, UriKind.Absolute, out _)
            ? trimmed
            : $"https://{trimmed}";
    }
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class WebAddressAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var address = value as string;
        if (string.IsNullOrWhiteSpace(address))
            return ValidationResult.Success;

        var normalized = AccountFormUtilities.NormalizeWebLink(address);
        return Uri.TryCreate(normalized, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            && !string.IsNullOrWhiteSpace(uri.Host)
                ? ValidationResult.Success
                : new ValidationResult("Enter a valid website address.");
    }
}
