using Microsoft.EntityFrameworkCore;
using PasswordManagement.Data;

namespace PasswordManagement.Security;

public sealed class StoredDataSecurityMigrator
{
    private readonly AccountContext dbContext;
    private readonly IUserPasswordService userPasswords;
    private readonly ICredentialProtector credentials;
    private readonly ILogger<StoredDataSecurityMigrator> logger;

    public StoredDataSecurityMigrator(
        AccountContext dbContext,
        IUserPasswordService userPasswords,
        ICredentialProtector credentials,
        ILogger<StoredDataSecurityMigrator> logger)
    {
        this.dbContext = dbContext;
        this.userPasswords = userPasswords;
        this.credentials = credentials;
        this.logger = logger;
    }

    public async Task UpgradeAsync(CancellationToken cancellationToken = default)
    {
        var changed = false;
        var users = await dbContext.MstUsers.ToListAsync(cancellationToken);

        foreach (var user in users.Where(user => !userPasswords.IsHashed(user.Password)))
        {
            user.Password = userPasswords.Hash(user, user.Password);
            changed = true;
        }

        var cards = await dbContext.MstCards.ToListAsync(cancellationToken);
        foreach (var card in cards.Where(card => !credentials.IsProtected(card.Password)))
        {
            card.Password = credentials.Protect(card.Password);
            changed = true;
        }

        if (!changed)
            return;

        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Existing authentication and vault credentials were upgraded to protected storage.");
    }
}
