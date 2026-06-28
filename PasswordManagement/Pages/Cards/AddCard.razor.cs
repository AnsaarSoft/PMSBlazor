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
    public partial class AddCard : IDisposable
    {
        private readonly AccountFormModel oModel = new();
        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly List<string> AccountTypes = new();

        private EditContext FormContext = default!;
        private MstSetting? PasswordSettings;
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
            new BreadcrumbItem("Add account", href: null, disabled: true)
        };

        [Inject] private NavigationManager oNavigation { get; set; } = default!;
        [Inject] private ISnackbar oToast { get; set; } = default!;
        [Inject] private AccountServices oServices { get; set; } = default!;
        [Inject] private IPasswordGenerator PasswordGenerator { get; set; } = default!;
        [Inject] private IJSRuntime oJS { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            FormContext = new EditContext(oModel);
            FormContext.OnFieldChanged += HandleFieldChanged;
            await LoadFormDataAsync();
        }

        private void HandleFieldChanged(object? sender, FieldChangedEventArgs args)
        {
            HasUnsavedChanges = true;
        }

        private async Task LoadFormDataAsync()
        {
            IsLoadingFormData = true;

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
                IsLoadingFormData = false;
            }
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
            if (IsSaving)
                return;

            IsSaving = true;
            MstCard? result;

            try
            {
                var record = new MstCard
                {
                    CardName = oModel.CardName.Trim(),
                    Alias = oModel.Alias.Trim(),
                    UserCode = oModel.UserCode.Trim(),
                    Password = oModel.Password,
                    Email = oModel.Email.Trim(),
                    WebLink = AccountFormUtilities.NormalizeWebLink(oModel.WebLink),
                    Remarks = oModel.Remarks.Trim()
                };

                result = await oServices.AddCard(record);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                ErrorMessage("The account could not be added. Please try again.");
                return;
            }
            finally
            {
                IsSaving = false;
            }

            if (result is null)
            {
                ErrorMessage("The account could not be added. Please try again.");
                return;
            }

            // Persistence has succeeded. Navigation cleanup must not report a false save failure.
            HasUnsavedChanges = false;
            SuccessMessage("Account added successfully.");
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
