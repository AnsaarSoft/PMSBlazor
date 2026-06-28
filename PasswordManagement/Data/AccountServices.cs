using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PasswordManagement.Security;
using PMSModels.Models;

namespace PasswordManagement.Data;

public sealed class AccountServices
{
    private readonly AccountContext dbContext;
    private readonly ICredentialProtector credentials;
    private readonly IUserPasswordService userPasswords;

    public AccountServices(
        AccountContext dbContext,
        ICredentialProtector credentials,
        IUserPasswordService userPasswords)
    {
        this.dbContext = dbContext;
        this.credentials = credentials;
        this.userPasswords = userPasswords;
    }

    public async Task<List<MstCard>> GetAllCards(CancellationToken cancellationToken = default)
    {
        var cards = await dbContext.MstCards
            .AsNoTracking()
            .Where(card => !card.IsDeleted)
            .OrderBy(card => card.CardName)
            .ThenBy(card => card.Alias)
            .ToListAsync(cancellationToken);

        foreach (var card in cards)
            card.Password = credentials.Unprotect(card.Password);

        return cards;
    }

    public async Task<MstCard?> GetCard(int id, CancellationToken cancellationToken = default)
    {
        var card = await dbContext.MstCards
            .AsNoTracking()
            .FirstOrDefaultAsync(card => card.Id == id && !card.IsDeleted, cancellationToken);

        if (card is not null)
            card.Password = credentials.Unprotect(card.Password);

        return card;
    }

    public async Task<MstCard> AddCard(MstCard record, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);

        var plainPassword = record.Password;
        record.Password = credentials.Protect(plainPassword);

        try
        {
            dbContext.MstCards.Add(record);
            await dbContext.SaveChangesAsync(cancellationToken);
            return record;
        }
        finally
        {
            dbContext.Entry(record).State = EntityState.Detached;
            record.Password = plainPassword;
        }
    }

    public async Task<MstCard?> UpdateCard(MstCard record, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);

        var existing = await dbContext.MstCards
            .FirstOrDefaultAsync(card => card.Id == record.Id && !card.IsDeleted, cancellationToken);

        if (existing is null)
            return null;

        existing.CardName = record.CardName;
        existing.Alias = record.Alias;
        existing.UserCode = record.UserCode;
        existing.Password = credentials.Protect(record.Password);
        existing.Email = record.Email;
        existing.Remarks = record.Remarks;
        existing.WebLink = record.WebLink;
        existing.IsActive = record.IsActive;

        await dbContext.SaveChangesAsync(cancellationToken);
        return record;
    }

    public async Task<bool> DeleteCard(MstCard record, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);

        var existing = await dbContext.MstCards
            .FirstOrDefaultAsync(card => card.Id == record.Id && !card.IsDeleted, cancellationToken);

        if (existing is null)
            return false;

        existing.IsDeleted = true;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<MstSetting?> GetSettings(CancellationToken cancellationToken = default)
    {
        return dbContext.MstSettings
            .AsNoTracking()
            .OrderBy(setting => setting.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<MstSetting?> SaveSettings(MstSetting record, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);

        if (record.Id == 0)
        {
            dbContext.MstSettings.Add(record);
            await dbContext.SaveChangesAsync(cancellationToken);
            dbContext.Entry(record).State = EntityState.Detached;
            return record;
        }

        var existing = await dbContext.MstSettings
            .FirstOrDefaultAsync(setting => setting.Id == record.Id, cancellationToken);

        if (existing is null)
            return null;

        existing.PasswordLength = record.PasswordLength;
        existing.UseUppercase = record.UseUppercase;
        existing.UseLowercase = record.UseLowercase;
        existing.UseNumbers = record.UseNumbers;
        existing.UseSymbols = record.UseSymbols;

        await dbContext.SaveChangesAsync(cancellationToken);
        return record;
    }

    public Task<List<MstUser>> GetAllUsers(CancellationToken cancellationToken = default)
    {
        return dbContext.MstUsers
            .AsNoTracking()
            .Where(user => !user.IsDeleted)
            .OrderBy(user => user.FullName)
            .ToListAsync(cancellationToken);
    }

    public async Task<MstUser?> AuthenticateUser(
        string userCode,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userCode) || string.IsNullOrEmpty(password))
            return null;

        var normalizedUserCode = userCode.Trim();
        var user = await dbContext.MstUsers.FirstOrDefaultAsync(
            candidate => candidate.UserCode == normalizedUserCode
                && candidate.IsActive
                && !candidate.IsDeleted,
            cancellationToken);

        if (user is null)
            return null;

        var result = userPasswords.Verify(user, password);
        if (result == PasswordVerificationResult.Failed)
            return null;

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.Password = userPasswords.Hash(user, password);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return new MstUser
        {
            Id = user.Id,
            UserCode = user.UserCode,
            FullName = user.FullName,
            Email = user.Email,
            IsActive = user.IsActive,
            IsDeleted = user.IsDeleted
        };
    }

    public async Task<MstUser> AddUser(MstUser record, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);
        record.Password = userPasswords.Hash(record, record.Password);
        dbContext.MstUsers.Add(record);
        await dbContext.SaveChangesAsync(cancellationToken);
        dbContext.Entry(record).State = EntityState.Detached;
        record.Password = string.Empty;
        return record;
    }

    public async Task<bool> UpdateUser(MstUser record, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);

        var existing = await dbContext.MstUsers.FirstOrDefaultAsync(user => user.Id == record.Id, cancellationToken);
        if (existing is null)
            return false;

        existing.UserCode = record.UserCode;
        existing.FullName = record.FullName;
        existing.Email = record.Email;
        existing.IsActive = record.IsActive;
        existing.IsDeleted = record.IsDeleted;

        if (!string.IsNullOrWhiteSpace(record.Password))
            existing.Password = userPasswords.Hash(existing, record.Password);

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteUser(MstUser record, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);

        var existing = await dbContext.MstUsers
            .FirstOrDefaultAsync(user => user.Id == record.Id && !user.IsDeleted, cancellationToken);

        if (existing is null)
            return false;

        existing.IsDeleted = true;
        existing.IsActive = false;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
