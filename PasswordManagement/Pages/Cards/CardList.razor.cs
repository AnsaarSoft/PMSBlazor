using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using PasswordManagement.Data;
using PMSModels.Models;


namespace PasswordManagement.Pages.Cards
{
    public partial class CardList
    {
        #region Variables

        List<MstCard> Cards = new();
        bool IsLoading = true;
        string? LoadErrorMessage;
        string SearchValue = string.Empty;
        HashSet<int> VisiblePasswords = new();
        readonly int[] PageSizeOptions = { 10, 25, 50 };
        readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        [Inject] NavigationManager oNavigation { get; set; } = default!;

        

        [Inject] IDialogService DialogService { get; set; } = default!;
        [Inject] ISnackbar oToast { get; set; } = default!;
        [Inject] AccountServices oServices { get; set; } = default!;
        [Inject] IJSRuntime oJS { get; set; } = default!;


        #endregion

        #region Functions
        
        public async Task LoadAccountsAsync()
        {
            try
            {
                IsLoading = true;
                LoadErrorMessage = null;
                Cards = await oServices.GetAllCards();

            }
            catch (Exception ex)
            {
                logger.Error(ex);
                LoadErrorMessage = "Unable to load accounts. Check the connection and try again.";
                ErrorMessage("Unable to load accounts.");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void SendtoAddCard()
        {
            oNavigation.NavigateTo("/addcard");
        }

        public async Task CopyUser(string value)
        {
            try
            {
                await oJS.InvokeVoidAsync("navigator.clipboard.writeText", value);
                SuccessMessage("User code copied.");
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                ErrorMessage("Unable to copy the user code.");
            }
        }

        public async Task CopyPassword(string value)
        {
            try
            {
                await oJS.InvokeVoidAsync("navigator.clipboard.writeText", value);
                SuccessMessage("Password copied.");
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                ErrorMessage("Unable to copy the password.");
            }
        }

        private void ClearSearch()
        {
            SearchValue = string.Empty;
        }

        private bool IsPasswordVisible(int cardId)
        {
            return VisiblePasswords.Contains(cardId);
        }

        private void TogglePasswordVisibility(int cardId)
        {
            if (!VisiblePasswords.Add(cardId))
            {
                VisiblePasswords.Remove(cardId);
            }
        }

        void OpenDialogEdit(int Id)
        {
            try
            {
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
                    DisableBackdropClick = false,
                    CloseOnEscapeKey = true,
                    NoHeader = true,
                    MaxWidth = MaxWidth.Small,
                    FullWidth = true
                };
                var parameters = new DialogParameters
                {
                    [nameof(DeleteCard.Account)] = card
                };
                var dialog = DialogService.Show<DeleteCard>("Delete account", parameters, Options);
                var result = await dialog.Result;
                if (!result.Cancelled)
                {
                    Cards.Remove(card);
                    VisiblePasswords.Remove(card.Id);
                    SuccessMessage("Account deleted successfully.");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                ErrorMessage("Unable to delete the account.");
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

            var search = SearchString.Trim();

            if (Record.CardName.Contains(search, StringComparison.OrdinalIgnoreCase))
                return true;
            if (Record.UserCode.Contains(search, StringComparison.OrdinalIgnoreCase))
                return true;
            if (Record.Alias.Contains(search, StringComparison.OrdinalIgnoreCase))
                return true;
            if (Record.Email.Contains(search, StringComparison.OrdinalIgnoreCase))
                return true;
            if (Record.WebLink.Contains(search, StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        }



        #endregion

        #region Events

        //public CardList(NavigationManager Navigation, IDialogService DialogService, ISnackbar Snackbar, AccountServices pAccountServices, IJSRuntime pJSRuntime)
        //{
        //    this.oNavigation = Navigation;
        //    this.DialogService = DialogService;
        //    this.oToast = Snackbar;
        //    this.oServices = pAccountServices;
        //    this.oJS = pJSRuntime;
        //}


        protected async override Task OnInitializedAsync()
        {
            await LoadAccountsAsync();
        }

        #endregion
    }
}
