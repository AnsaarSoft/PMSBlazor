using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.Filters;
using MudBlazor;
using PasswordManagement.Data;
using PMSModels.Models;
using System.Data;

namespace PasswordManagement.Pages.Cards
{
    public partial class EditCard
    {
        #region Variables

        MstCard oModel = new();
        string SearchValue = string.Empty;
        List<BreadcrumbItem> BreadCrumItems;

        [Parameter] public int Id { get; set; }
        [Inject] NavigationManager oNavigation { get; set; }
        [Inject] ISnackbar oToast { get; set; }
        [Inject] AccountServices oServices { get; set; }


        #endregion

        #region Functions

        public async Task InitiallizeForm()
        {
            try
            {
                await Task.Delay(2000);
                BreadCrumItems = new List<BreadcrumbItem>
                {
                    new BreadcrumbItem("Cards", href: "#"),
                    new BreadcrumbItem("Edit Card", href: "#")
                };
                oModel = await oServices.GetCard(Id);
                if (oModel is null)
                {
                    ErrorMessage("Document didn't load successfully.");
                    oNavigation.NavigateTo("/editcard");
                }

            }
            catch (Exception ex)
            {

            }
        }

        public async Task SaveCard()
        {
            try
            {
                if (string.IsNullOrEmpty(oModel.CardName))
                {
                    InfoMessage("Type is mandatory.");
                }
                if (string.IsNullOrEmpty(oModel.Alias))
                {
                    InfoMessage("Alias is mandatory.");
                }
                if (string.IsNullOrEmpty(oModel.UserCode))
                {
                    InfoMessage("Usercode is mandatory.");
                }
                if (string.IsNullOrEmpty(oModel.Password))
                {
                    InfoMessage("Password is mandatory.");
                }
                if (string.IsNullOrEmpty(oModel.Email))
                {
                    InfoMessage("Email is mandatory.");
                }
                if (string.IsNullOrEmpty(oModel.WebLink))
                {
                    InfoMessage("Weblink is mandatory.");
                }
                var result = await oServices.UpdateCard(oModel);
                if (result is not null)
                {
                    oNavigation.NavigateTo("/cardlist");
                }
                else
                {
                    ErrorMessage("Some error occured, Record didn't updated.");
                }
            }
            catch (Exception ex)
            {

            }
        }

        public async Task CancelCard()
        {
            try
            {
                await Task.Delay(1000);
                oNavigation.NavigateTo("/cardlist");
            }
            catch (Exception ex)
            {

            }
        }

        public async Task GeneratePassword()
        {

            try
            {
                var oPasswordSettings = await oServices.GetSettings();
                if (oPasswordSettings is null) return;
                string password = string.Empty;
                int PasswordStrenght = oPasswordSettings.PasswodLenght;
                for (int i = 0; i < 6; i++)
                {
                    if (PasswordStrenght <= password.Length) break;
                    //Small Letter
                    if (oPasswordSettings.flgSmallLetter)
                    {
                        Random smallLetter = new Random(i + DateTime.Now.Millisecond);
                        password += Convert.ToChar(smallLetter.Next(97, 122));
                    }
                    if (PasswordStrenght <= password.Length) break;
                    //Big Letter
                    if (oPasswordSettings.flgCapitalLetter)
                    {
                        Random bigLetter = new Random(i + DateTime.Now.Millisecond + 2);
                        password += Convert.ToChar(bigLetter.Next(65, 90));
                    }
                    if (PasswordStrenght <= password.Length) break;
                    //Special Letter
                    if(oPasswordSettings.flgSpecial)
                    {
                        char[] chars = new char[7];
                        chars[0] = '!'; chars[1] = '#'; chars[2] = '@'; chars[3] = '$';
                        chars[4] = '*'; chars[5] = '^'; chars[6] = '%';
                        //if (i % 2 == 0)
                        if (!password.Contains(chars.ToString()))
                        {
                            Random specialLetter = new Random(i + DateTime.Now.Millisecond);
                            password += Convert.ToChar(chars[specialLetter.Next(0, 6)]);
                        }
                    }
                    if (PasswordStrenght <= password.Length) break;
                    //Number Letter
                    if(oPasswordSettings.flgNumbers)
                    {
                        Random numberLetter = new Random(i + DateTime.Now.Millisecond);
                        password += Convert.ToChar(numberLetter.Next(48, 57));
                    }
                }
                if (oModel.Id != 0)
                {
                    oModel.Remarks += $"{Environment.NewLine}Password changed from {oModel.Password} to {password}";
                }
                oModel.Password = password;
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
