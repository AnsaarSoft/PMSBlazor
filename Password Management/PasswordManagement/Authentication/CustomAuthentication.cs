using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Mvc.Filters;
using PMSModels.Models;
using System.Security.Claims;

namespace PasswordManagement.Authentication
{
    public class CustomAuthentication : AuthenticationStateProvider
    {
        private readonly ProtectedSessionStorage oSessionStorage;
        private ClaimsPrincipal oAnonymous = new ClaimsPrincipal(new ClaimsIdentity());
        public CustomAuthentication(ProtectedSessionStorage oSessionStorage)
        {
            this.oSessionStorage = oSessionStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var oSaveUserDataResult = await oSessionStorage.GetAsync<MstUser>("LoginUser");
                var oSaveUserData = oSaveUserDataResult.Success ? oSaveUserDataResult.Value : null;
                if (oSaveUserData is null)
                    return await Task.FromResult(new AuthenticationState(oAnonymous));
                var ClaimedPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new Claim(ClaimTypes.Name, oSaveUserData.UserCode),
                new Claim(ClaimTypes.Surname, oSaveUserData.FullName),
                new Claim(ClaimTypes.Email, oSaveUserData.Email),
                new Claim(ClaimTypes.Role, "Admin")
            }, "CustomAuth"));
                return await Task.FromResult(new AuthenticationState(ClaimedPrincipal));
            }
            catch
            {
                return await Task.FromResult(new AuthenticationState(oAnonymous));
            }
        }

        public async Task UpdateAuthenticationState(MstUser oUser)
        {
            ClaimsPrincipal claimsPrincipal;
            if(oUser is null)
            {
                await oSessionStorage.DeleteAsync("LoginUser");
                claimsPrincipal = oAnonymous;
            }
            else
            {
                await oSessionStorage.SetAsync("LoginUser", oUser);
                claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                {   
                    new Claim(ClaimTypes.Name, oUser.UserCode),
                    new Claim(ClaimTypes.Surname, oUser.FullName),
                    new Claim(ClaimTypes.Email, oUser.Email),
                    new Claim(ClaimTypes.Role, "Admin")
                }));
            }
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
        }
    }
}
