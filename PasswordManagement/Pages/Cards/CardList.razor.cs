using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using MudBlazor.Services;
using PasswordManagement.Data;
using PMSModels.Models;
using static MudBlazor.CategoryTypes;

namespace PasswordManagement.Pages.Cards
{
    public partial class CardList
    {
        #region Variables

        List<MstCard> Cards = new List<MstCard>();
        string SearchValue = string.Empty;
        MstCard SelectedItem = null;
        List<BreadcrumbItem> BreadCrumItems;
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        [Inject] NavigationManager oNavigation { get; set; }
        [Inject] IDialogService DialogService { get; set; }
        [Inject] ISnackbar oToast { get; set; }
        [Inject] AccountServices oServices { get; set; }


        #endregion

        #region Functions
        
        public async Task InitiallizeForm()
        {
            try
            {
                //await Task.Delay(1000);
                BreadCrumItems = new List<BreadcrumbItem>
                {
                    new BreadcrumbItem("Cards", href: "#"),
                    new BreadcrumbItem("Card Details", href: "#")
                };

                Cards = await oServices.GetAllCards();

            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public void SendtoAddCard()
        {
            oNavigation.NavigateTo("/addcard");
        }

        public void CopyUser()
        {
             
        }

        public void CopyPassword()
        {
            
        }

        async Task OpenDialogEdit(int Id)
        {
            try
            {
                await Task.Delay(5);
                oNavigation.NavigateTo($"/editcard/{Id}");
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        async Task OpenDialogDelete(MstCard card)
        {
            try
            {
                var Options = new DialogOptions
                {
                    Position = DialogPosition.Center,
                    DisableBackdropClick = true,
                    CloseOnEscapeKey = false,
                    NoHeader = true,
                    MaxWidth = MaxWidth.Small,
                    FullWidth = true
                };
                var parameters = new DialogParameters();
                parameters.Add("ContentText", "Do you really want to delete this records? This process cannot be undone.");
                parameters.Add("ButtonText", "Delete");
                parameters.Add("Color", Color.Error);
                var dialog = DialogService.Show<DeleteCard>("Delete Card", parameters, Options);
                var result = await dialog.Result;
                if (!result.Cancelled)
                {
                    var record = oServices.DeleteCard(card);
                    if(record is null)
                    {
                        ErrorMessage("Record didn't deleted successfully.");
                    }
                    else
                    {
                        oNavigation.NavigateTo("/cardlist", true);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
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

        private bool FilterFuncCards(MstCard Record) => FilterFunc(Record, SearchValue);

        private bool FilterFunc(MstCard Record, string SearchString)
        {
            if (string.IsNullOrWhiteSpace(SearchString))
                return true;
            if (Record.CardName.Contains(SearchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (Record.UserCode.Contains(SearchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (Record.Alias.Contains(SearchString, StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
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
