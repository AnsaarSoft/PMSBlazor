using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using MudBlazor;
using PasswordManagement.Authentication;
using PasswordManagement.Data;
using PMSModels.Models;
using System.Collections;

namespace PasswordManagement.Pages.Login
{
    public partial class Login
    {

        #region Variable

        bool PasswordVisibility;
        bool flgClicked = false;
        InputType PasswordInput = InputType.Password;
        string PasswordInputIcon = Icons.Material.Filled.VisibilityOff;
        MstUser oModel = new();
        bool flgLoading = false;
        
        [Inject] NavigationManager oNavigation { get; set; }
        [Inject] ISnackbar oToast { get; set; }
        [Inject] AccountServices oServices { get; set; }
        [Inject] AuthenticationStateProvider oAuthService { get; set; }

        #endregion

        #region Functions

        protected async override Task OnInitializedAsync()
        {
            //return base.OnInitializedAsync();
            await InitiallizeForm();
        }

        public async Task InitiallizeForm()
        {
            try
            {
                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
            }
        }

        void TogglePasswordVisibility()
        {
            if (PasswordVisibility)
            {
                PasswordVisibility = false;
                PasswordInputIcon = Icons.Material.Filled.VisibilityOff;
                PasswordInput = InputType.Password;
            }
            else
            {
                PasswordVisibility = true;
                PasswordInputIcon = Icons.Material.Filled.Visibility;
                PasswordInput = InputType.Text;
            }
        }

        public async Task CheckLogin()
        {
            flgClicked = true;
            try
            {
                if (string.IsNullOrEmpty(oModel.UserCode))
                {
                    InfoMessage("Usercode is mandatory.");
                }
                if(string.IsNullOrEmpty(oModel.Password))
                {
                    InfoMessage("Password is mandatory.");
                }
                var result = await oServices.CheckUser(oModel);
                if(result is not null)
                {
                    SuccessMessage("You logged in successfully.");
                    //await oLocalStorage.SetItemAsync<MstUser>("User", result); 
                    //await Task.Delay(3000);
                    var CustomAuth = (CustomAuthentication)oAuthService;
                    await CustomAuth.UpdateAuthenticationState(new MstUser
                    {
                        UserCode = result.UserCode,
                        Password = result.Password, 
                        FullName = result.FullName,
                        Email = result.Email
                    });
                    await Task.Delay(2000);
                    oNavigation.NavigateTo("/cardlist", true);
                }
                else
                {
                    ErrorMessage("Wrong Credentials, try again.");
                }
            }
            catch (Exception ex)
            {
                
            }
            flgClicked = false;
        }

        public async Task PasswordKeypress(KeyboardEventArgs args)
        {
            try
            {
               if(args.Code == "Enter" || args.Code == "NumpadEnter")
                   await CheckLogin();
            }
            catch (Exception ex)
            {
                
            }
        }

        public void SuccessMessage(string message)
        {
            oToast.Add(message, Severity.Success);
        }

        public void ErrorMessage(string message)
        {
            oToast.Add(message, Severity.Error);
        }

        public void InfoMessage(string message)
        {
            oToast.Add(message, Severity.Info);
        }

        #endregion
    }
}
