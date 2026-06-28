using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using PMSModels.Models;

namespace PasswordManagement.Authentication;

public sealed class CustomAuthentication : AuthenticationStateProvider
{
    private const string SessionKey = "LoginUser";
    private static readonly ClaimsPrincipal Anonymous = new(new ClaimsIdentity());

    private readonly ProtectedSessionStorage sessionStorage;
    private readonly ILogger<CustomAuthentication> logger;

    public CustomAuthentication(
        ProtectedSessionStorage sessionStorage,
        ILogger<CustomAuthentication> logger)
    {
        this.sessionStorage = sessionStorage;
        this.logger = logger;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var stored = await sessionStorage.GetAsync<AuthenticatedUserSession>(SessionKey);
            return stored.Success && stored.Value is not null
                ? new AuthenticationState(CreatePrincipal(stored.Value))
                : new AuthenticationState(Anonymous);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "The saved authentication session could not be restored.");
            return new AuthenticationState(Anonymous);
        }
    }

    public async Task UpdateAuthenticationState(MstUser? user)
    {
        ClaimsPrincipal principal;

        if (user is null)
        {
            await sessionStorage.DeleteAsync(SessionKey);
            principal = Anonymous;
        }
        else
        {
            var session = new AuthenticatedUserSession
            {
                UserCode = user.UserCode,
                FullName = user.FullName,
                Email = user.Email,
                Role = "Admin"
            };

            await sessionStorage.SetAsync(SessionKey, session);
            principal = CreatePrincipal(session);
        }

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
    }

    private static ClaimsPrincipal CreatePrincipal(AuthenticatedUserSession user)
    {
        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.Name, user.UserCode),
                new Claim(ClaimTypes.Surname, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            },
            authenticationType: "PmsSession");

        return new ClaimsPrincipal(identity);
    }
}

public sealed class AuthenticatedUserSession
{
    public string UserCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "Admin";
}
