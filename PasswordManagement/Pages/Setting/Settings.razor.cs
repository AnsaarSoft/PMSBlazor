using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using MudBlazor;
using PasswordManagement.Data;
using PasswordManagement.Services;
using PMSModels.Models;

namespace PasswordManagement.Pages.Setting
{
    public partial class Settings : IDisposable
    {
        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private SettingsFormModel oModel = new();
        private MstSetting? ExistingSettings;
        private EditContext FormContext = default!;
        private MudNumericField<int>? PasswordLengthField;
        private string? LoadErrorMessage;
        private bool IsLoading = true;
        private bool IsSaving;
        private bool HasUnsavedChanges;

        private bool IsPolicyValid => oModel.PasswordLength is >= 6 and <= 20
            && (oModel.UseUppercase || oModel.UseLowercase || oModel.UseNumbers || oModel.UseSymbols)
            && (!oModel.UseSymbols || oModel.UseUppercase || oModel.UseLowercase || oModel.UseNumbers);

        private string PolicyValidationMessage => oModel.PasswordLength is < 6 or > 20
            ? "Password length must be between 6 and 20 characters."
            : !oModel.UseUppercase && !oModel.UseLowercase && !oModel.UseNumbers && !oModel.UseSymbols
                ? "Select at least one character type."
                : "Select uppercase, lowercase, or numbers when symbols are enabled.";

        private string PolicyDescription => PasswordGenerator.Describe(ToSetting(oModel));

        [Inject] private AccountServices AccountServices { get; set; } = default!;
        [Inject] private IPasswordGenerator PasswordGenerator { get; set; } = default!;
        [Inject] private ISnackbar Snackbar { get; set; } = default!;
        [Inject] private IJSRuntime JS { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            await LoadSettingsAsync();
        }

        private async Task LoadSettingsAsync()
        {
            IsLoading = true;
            LoadErrorMessage = null;
            HasUnsavedChanges = false;

            if (FormContext is not null)
                FormContext.OnFieldChanged -= HandleFieldChanged;

            try
            {
                ExistingSettings = await AccountServices.GetSettings();
                ExistingSettings ??= new MstSetting
                {
                    PasswordLength = 16,
                    UseUppercase = true,
                    UseLowercase = true,
                    UseNumbers = true,
                    UseSymbols = true
                };

                InitializeForm(ExistingSettings);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                LoadErrorMessage = "Password settings could not be loaded. Check the connection and try again.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void InitializeForm(MstSetting settings)
        {
            if (FormContext is not null)
                FormContext.OnFieldChanged -= HandleFieldChanged;

            oModel = new SettingsFormModel
            {
                PasswordLength = settings.PasswordLength,
                UseUppercase = settings.UseUppercase,
                UseLowercase = settings.UseLowercase,
                UseNumbers = settings.UseNumbers,
                UseSymbols = settings.UseSymbols
            };

            FormContext = new EditContext(oModel);
            FormContext.OnFieldChanged += HandleFieldChanged;
            HasUnsavedChanges = false;
        }

        private void HandleFieldChanged(object? sender, FieldChangedEventArgs args)
        {
            HasUnsavedChanges = true;
        }

        private async Task SaveSettings()
        {
            if (IsSaving || !IsPolicyValid || ExistingSettings is null)
                return;

            IsSaving = true;
            MstSetting? result;

            try
            {
                ExistingSettings.PasswordLength = oModel.PasswordLength;
                ExistingSettings.UseUppercase = oModel.UseUppercase;
                ExistingSettings.UseLowercase = oModel.UseLowercase;
                ExistingSettings.UseNumbers = oModel.UseNumbers;
                ExistingSettings.UseSymbols = oModel.UseSymbols;

                result = await AccountServices.SaveSettings(ExistingSettings);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                ErrorMessage("Settings could not be saved. Please try again.");
                return;
            }
            finally
            {
                IsSaving = false;
            }

            if (result is null)
            {
                ErrorMessage("Settings could not be saved. Please reload the page and try again.");
                return;
            }

            ExistingSettings = result;
            InitializeForm(result);
            SuccessMessage("Password settings saved.");
        }

        private async Task HandleInvalidSubmit(EditContext editContext)
        {
            if (editContext.GetValidationMessages(new FieldIdentifier(oModel, nameof(oModel.PasswordLength))).Any()
                && PasswordLengthField is not null)
            {
                await PasswordLengthField.FocusAsync();
            }

            ErrorMessage("Review the password policy and try again.");
        }

        private async Task DiscardChanges()
        {
            if (!HasUnsavedChanges || ExistingSettings is null)
                return;

            var discard = await JS.InvokeAsync<bool>(
                "confirm",
                "Discard your unsaved password-setting changes?");

            if (discard)
                InitializeForm(ExistingSettings);
        }

        private async Task ConfirmInternalNavigation(LocationChangingContext context)
        {
            if (!HasUnsavedChanges || IsSaving)
                return;

            var discard = await JS.InvokeAsync<bool>(
                "confirm",
                "You have unsaved settings. Do you want to leave this page?");

            if (!discard)
            {
                context.PreventNavigation();
                return;
            }

            HasUnsavedChanges = false;
        }

        private static MstSetting ToSetting(SettingsFormModel model)
        {
            return new MstSetting
            {
                PasswordLength = model.PasswordLength,
                UseUppercase = model.UseUppercase,
                UseLowercase = model.UseLowercase,
                UseNumbers = model.UseNumbers,
                UseSymbols = model.UseSymbols
            };
        }

        private void SuccessMessage(string message) => Snackbar.Add(message, Severity.Success);
        private void ErrorMessage(string message) => Snackbar.Add(message, Severity.Error);

        public void Dispose()
        {
            if (FormContext is not null)
                FormContext.OnFieldChanged -= HandleFieldChanged;
        }

        private sealed class SettingsFormModel : IValidatableObject
        {
            [Range(6, 20, ErrorMessage = "Password length must be between 6 and 20 characters.")]
            public int PasswordLength { get; set; } = 16;

            public bool UseUppercase { get; set; } = true;
            public bool UseLowercase { get; set; } = true;
            public bool UseNumbers { get; set; } = true;
            public bool UseSymbols { get; set; } = true;

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (!UseUppercase && !UseLowercase && !UseNumbers && !UseSymbols)
                {
                    yield return new ValidationResult(
                        "Select at least one character type.",
                        new[] { nameof(UseUppercase), nameof(UseLowercase), nameof(UseNumbers), nameof(UseSymbols) });
                }

                if (UseSymbols && !UseUppercase && !UseLowercase && !UseNumbers)
                {
                    yield return new ValidationResult(
                        "Select uppercase, lowercase, or numbers when symbols are enabled.",
                        new[] { nameof(UseUppercase), nameof(UseLowercase), nameof(UseNumbers), nameof(UseSymbols) });
                }
            }
        }
    }
}
