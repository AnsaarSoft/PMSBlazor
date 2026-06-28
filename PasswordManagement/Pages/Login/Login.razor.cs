using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using MudBlazor;
using PasswordManagement.Authentication;
using PasswordManagement.Data;

namespace PasswordManagement.Pages.Login;

public partial class Login
{
    private const string RememberedUserCodeKey = "remembered-user-code";

    private bool PasswordVisibility;
    private bool IsSigningIn;
    private InputType PasswordInput = InputType.Password;
    private string PasswordInputIcon = Icons.Material.Filled.VisibilityOff;
    private readonly LoginModel Model = new();
    private bool RememberMe;

    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private AccountServices Accounts { get; set; } = default!;
    [Inject] private AuthenticationStateProvider Authentication { get; set; } = default!;
    [Inject] private ILogger<Login> Logger { get; set; } = default!;
    [Inject] private ProtectedLocalStorage LocalStorage { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var state = await Authentication.GetAuthenticationStateAsync();
            if (state.User.Identity?.IsAuthenticated == true)
            {
                Navigation.NavigateTo("/cardlist", forceLoad: true);
                return;
            }

            var savedUserCode = await LocalStorage.GetAsync<string>(RememberedUserCodeKey);
            if (savedUserCode.Success && !string.IsNullOrWhiteSpace(savedUserCode.Value))
            {
                Model.UserCode = savedUserCode.Value;
                RememberMe = true;
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "The remembered login could not be restored.");
        }
    }

    private void TogglePasswordVisibility()
    {
        PasswordVisibility = !PasswordVisibility;
        PasswordInput = PasswordVisibility ? InputType.Text : InputType.Password;
        PasswordInputIcon = PasswordVisibility
            ? Icons.Material.Filled.Visibility
            : Icons.Material.Filled.VisibilityOff;
    }

    private async Task CheckLogin()
    {
        if (IsSigningIn)
            return;

        IsSigningIn = true;

        try
        {
            var user = await Accounts.AuthenticateUser(Model.UserCode, Model.Password);
            if (user is null)
            {
                Snackbar.Add("The user code or password is incorrect.", Severity.Error);
                return;
            }

            var customAuthentication = (CustomAuthentication)Authentication;
            await customAuthentication.UpdateAuthenticationState(user);

            if (RememberMe)
                await LocalStorage.SetAsync(RememberedUserCodeKey, user.UserCode);
            else
                await LocalStorage.DeleteAsync(RememberedUserCodeKey);

            Snackbar.Add("You logged in successfully.", Severity.Success);
            Navigation.NavigateTo("/cardlist", forceLoad: true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Login failed unexpectedly.");
            Snackbar.Add("Unable to sign in right now. Please try again.", Severity.Error);
        }
        finally
        {
            IsSigningIn = false;
        }
    }

    private sealed class LoginModel
    {
        [Required(ErrorMessage = "User code is required.")]
        public string UserCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty;
    }
}
