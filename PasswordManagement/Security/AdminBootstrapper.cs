using Microsoft.EntityFrameworkCore;
using PasswordManagement.Data;
using PMSModels.Models;

namespace PasswordManagement.Security;

public sealed class AdminBootstrapper
{
    private readonly AccountContext dbContext;
    private readonly IUserPasswordService passwords;
    private readonly IConfiguration configuration;
    private readonly ILogger<AdminBootstrapper> logger;

    public AdminBootstrapper(
        AccountContext dbContext,
        IUserPasswordService passwords,
        IConfiguration configuration,
        ILogger<AdminBootstrapper> logger)
    {
        this.dbContext = dbContext;
        this.passwords = passwords;
        this.configuration = configuration;
        this.logger = logger;
    }

    public async Task EnsureAdminAsync(CancellationToken cancellationToken = default)
    {
        if (await dbContext.MstUsers.AnyAsync(cancellationToken))
            return;

        var userCode = configuration["BootstrapAdmin:UserCode"];
        var password = configuration["BootstrapAdmin:Password"];
        var fullName = configuration["BootstrapAdmin:FullName"];
        var email = configuration["BootstrapAdmin:Email"];

        if (string.IsNullOrWhiteSpace(userCode)
            || string.IsNullOrWhiteSpace(password)
            || string.IsNullOrWhiteSpace(fullName)
            || string.IsNullOrWhiteSpace(email))
        {
            logger.LogWarning(
                "No users exist. Configure BootstrapAdmin__UserCode, BootstrapAdmin__Password, " +
                "BootstrapAdmin__FullName, and BootstrapAdmin__Email before first login.");
            return;
        }

        var admin = new MstUser
        {
            UserCode = userCode.Trim(),
            FullName = fullName.Trim(),
            Email = email.Trim(),
            IsActive = true,
            IsDeleted = false
        };
        admin.Password = passwords.Hash(admin, password);

        dbContext.MstUsers.Add(admin);
        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("The initial administrator account was provisioned from secure configuration.");
    }
}
