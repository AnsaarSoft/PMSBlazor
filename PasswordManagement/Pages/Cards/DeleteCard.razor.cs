using Microsoft.AspNetCore.Components;
using MudBlazor;
using PasswordManagement.Data;
using PMSModels.Models;

namespace PasswordManagement.Pages.Cards
{
    public partial class DeleteCard
    {
        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private bool IsDeleting;
        private string? ErrorMessage;

        [CascadingParameter] private MudDialogInstance MudDialog { get; set; } = default!;
        [Parameter, EditorRequired] public MstCard Account { get; set; } = default!;

        [Inject] private AccountServices AccountServices { get; set; } = default!;

        private async Task Submit()
        {
            if (IsDeleting)
                return;

            IsDeleting = true;
            ErrorMessage = null;

            try
            {
                var deleted = await AccountServices.DeleteCard(Account);
                if (!deleted)
                {
                    ErrorMessage = "This account could not be found. It may already have been removed.";
                    return;
                }

                MudDialog.Close(DialogResult.Ok(Account.Id));
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                ErrorMessage = "The account could not be deleted. Check the connection and try again.";
            }
            finally
            {
                IsDeleting = false;
            }
        }

        private void Cancel()
        {
            if (!IsDeleting)
                MudDialog.Cancel();
        }
    }
}
