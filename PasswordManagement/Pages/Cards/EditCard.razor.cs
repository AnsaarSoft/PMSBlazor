using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using MudBlazor;
using PasswordManagement.Data;
using PasswordManagement.Services;
using PMSModels.Models;

namespace PasswordManagement.Pages.Cards
{
    public partial class EditCard : IDisposable
    {
        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly List<string> AccountTypes = new();

        private AccountFormModel oModel = new();
        private MstCard? ExistingRecord;
        private EditContext FormContext = default!;
        private MstSetting? PasswordSettings;
        private string? LoadErrorMessage;
        private bool IsLoading = true;
        private bool IsLoadingFormData = true;
        private bool IsSaving;
        private bool IsGenerating;
        private bool IsPasswordVisible;
        private bool HasUnsavedChanges;

        private AccountFormFields? AccountForm;

        private string PasswordSettingsDescription => IsLoadingFormData
            ? "Loading password settings…"
            : PasswordSettings is null
                ? "Password generation is unavailable until settings are configured."
                : PasswordGenerator.Describe(PasswordSettings);

        private List<BreadcrumbItem> BreadcrumbItems { get; } = new()
        {
            new BreadcrumbItem("Accounts", href: "/cardlist"),
            new BreadcrumbItem("Edit account", href: null, disabled: true)
        };

        [Parameter] public int Id { get; set; }

        [Inject] private NavigationManager oNavigation { get; set; } = default!;
        [Inject] private ISnackbar oToast { get; set; } = default!;
        [Inject] private AccountServices oServices { get; set; } = default!;
        [Inject] private IPasswordGenerator PasswordGenerator { get; set; } = default!;
        [Inject] private IJSRuntime oJS { get; set; } = default!;

        protected override async Task OnParametersSetAsync()
        {
            await LoadAccountAsync();
        }

        private async Task LoadAccountAsync()
        {
            IsLoading = true;
            IsLoadingFormData = true;
            LoadErrorMessage = null;
            PasswordSettings = null;
            ExistingRecord = null;
            AccountTypes.Clear();
            HasUnsavedChanges = false;

            if (FormContext is not null)
                FormContext.OnFieldChanged -= HandleFieldChanged;

            if (Id <= 0)
            {
                LoadErrorMessage = "No account was selected for editing.";
                IsLoading = false;
                IsLoadingFormData = false;
                return;
            }

            try
            {
                ExistingRecord = await oServices.GetCard(Id);
                if (ExistingRecord is null || ExistingRecord.IsDeleted)
                {
                    LoadErrorMessage = "This account no longer exists or is unavailable.";
                    IsLoading = false;
                    IsLoadingFormData = false;
                    return;
                }

                oModel = new AccountFormModel
                {
                    CardName = ExistingRecord.CardName,
                    Alias = ExistingRecord.Alias,
                    UserCode = ExistingRecord.UserCode,
                    Password = ExistingRecord.Password,
                    Email = ExistingRecord.Email,
                    WebLink = ExistingRecord.WebLink,
                    Remarks = ExistingRecord.Remarks
                };

                FormContext = new EditContext(oModel);
                FormContext.OnFieldChanged += HandleFieldChanged;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                LoadErrorMessage = "The account could not be loaded. Check the connection and try again.";
                IsLoading = false;
                IsLoadingFormData = false;
                return;
            }

            try
            {
                PasswordSettings = await oServices.GetSettings();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                WarningMessage("Password-generation settings could not be loaded.");
            }

            try
            {
                var accounts = await oServices.GetAllCards();
                AccountTypes.AddRange(accounts
                    .Select(account => account.CardName.Trim())
                    .Where(type => !string.IsNullOrWhiteSpace(type))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(type => type));
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                WarningMessage("Existing account types could not be loaded.");
            }
            finally
            {
                IsLoading = false;
                IsLoadingFormData = false;
            }
        }

        private void HandleFieldChanged(object? sender, FieldChangedEventArgs args)
        {
            HasUnsavedChanges = true;
        }

        private Task<IEnumerable<string>> SearchAccountTypes(string value)
        {
            IEnumerable<string> matches = AccountTypes;
            if (!string.IsNullOrWhiteSpace(value))
            {
                matches = matches.Where(type =>
                    type.Contains(value.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            return Task.FromResult(matches);
        }

        private async Task SaveCard()
        {
            if (IsSaving || ExistingRecord is null)
                return;

            IsSaving = true;
            MstCard? result;

            try
            {
                ExistingRecord.CardName = oModel.CardName.Trim();
                ExistingRecord.Alias = oModel.Alias.Trim();
                ExistingRecord.UserCode = oModel.UserCode.Trim();
                ExistingRecord.Password = oModel.Password;
                ExistingRecord.Email = oModel.Email.Trim();
                ExistingRecord.WebLink = AccountFormUtilities.NormalizeWebLink(oModel.WebLink);
                ExistingRecord.Remarks = oModel.Remarks.Trim();

                result = await oServices.UpdateCard(ExistingRecord);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                ErrorMessage("The account could not be updated. Please try again.");
                return;
            }
            finally
            {
                IsSaving = false;
            }

            if (result is null)
            {
                ErrorMessage("The account could not be updated. It may no longer exist.");
                return;
            }

            HasUnsavedChanges = false;
            SuccessMessage("Account updated successfully.");
            oNavigation.NavigateTo("/cardlist");
        }

        private async Task HandleInvalidSubmit(EditContext editContext)
        {
            await FocusFirstInvalidFieldAsync(editContext);
            ErrorMessage("Review the highlighted fields and try again.");
        }

        private async Task FocusFirstInvalidFieldAsync(EditContext editContext)
        {
            if (AccountForm is not null)
                await AccountForm.FocusFirstInvalidFieldAsync(editContext);
        }

        private async Task CancelCard()
        {
            if (HasUnsavedChanges)
            {
                var discard = await oJS.InvokeAsync<bool>(
                    "confirm",
                    "Discard your changes and return to Accounts?");

                if (!discard)
                    return;
            }

            HasUnsavedChanges = false;
            oNavigation.NavigateTo("/cardlist");
        }

        private async Task ConfirmInternalNavigation(LocationChangingContext context)
        {
            if (!HasUnsavedChanges || IsSaving)
                return;

            var discard = await oJS.InvokeAsync<bool>(
                "confirm",
                "You have unsaved changes. Do you want to leave this page?");

            if (!discard)
            {
                context.PreventNavigation();
                return;
            }

            HasUnsavedChanges = false;
        }

        private void TogglePasswordVisibility()
        {
            IsPasswordVisible = !IsPasswordVisible;
        }

        private async Task GeneratePassword()
        {
            if (IsGenerating || PasswordSettings is null)
                return;

            IsGenerating = true;

            try
            {
                await Task.Yield();
                oModel.Password = PasswordGenerator.Generate(PasswordSettings);
                FormContext.NotifyFieldChanged(new FieldIdentifier(oModel, nameof(oModel.Password)));
                SuccessMessage("Password generated.");
            }
            catch (InvalidOperationException ex)
            {
                ErrorMessage(ex.Message);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                ErrorMessage("A password could not be generated. Please try again.");
            }
            finally
            {
                IsGenerating = false;
            }
        }

        private async Task CopyPassword()
        {
            if (string.IsNullOrEmpty(oModel.Password))
                return;

            try
            {
                await oJS.InvokeVoidAsync("navigator.clipboard.writeText", oModel.Password);
                SuccessMessage("Password copied.");
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                WarningMessage("The password could not be copied.");
            }
        }

        private void SuccessMessage(string message) => oToast.Add(message, Severity.Success);
        private void ErrorMessage(string message) => oToast.Add(message, Severity.Error);
        private void WarningMessage(string message) => oToast.Add(message, Severity.Warning);

        public void Dispose()
        {
            if (FormContext is not null)
                FormContext.OnFieldChanged -= HandleFieldChanged;
        }

    }
}
