using Microsoft.AspNetCore.Components;
using MudBlazor;
using PasswordManagement.Data;
using PMSModels.Models;
using System.Runtime.Serialization;

namespace PasswordManagement.Pages.Setting
{
    public partial class Settings
    {

        #region Variables

        MstSetting oModel = new();
        string SearchValue = string.Empty;
        List<BreadcrumbItem> BreadCrumItems;


        [Inject] NavigationManager oNavigation { get; set; }
        [Inject] ISnackbar oToast { get; set; }
        [Inject] AccountServices oServices { get; set; }


        #endregion

        #region Functions

        public async Task InitiallizeForm()
        {
            try
            {
                await Task.Delay(1000);
                BreadCrumItems = new List<BreadcrumbItem>
                {
                    new BreadcrumbItem("Settings", href: "#"),
                    new BreadcrumbItem("Configuration", href: "#")
                };
                oModel = await oServices.GetSettings();
            }
            catch (Exception ex)
            {

            }
        }

        public async Task SaveSetting()
        {
            try
            {
                
                if (oModel.PasswodLenght == 0)
                {
                    InfoMessage("Password lenght is mandatory.");
                    return;
                }
                var result = await oServices.AddSetting(oModel);
                if (result is not null)
                {
                    //oNavigation.NavigateTo("/cardlist");
                    SuccessMessage("Document Updated successfully.");
                }
                else
                {
                    ErrorMessage("Some error occured, Record didn't added.");
                }
            }
            catch (Exception ex)
            {

            }
        }

        public async Task CancelSetting()
        {
            try
            {
                await Task.Delay(50);
                oNavigation.NavigateTo("/cardlist");
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

        #region Events

        protected async override Task OnInitializedAsync()
        {
            await InitiallizeForm();
        }

        #endregion
    }
}
