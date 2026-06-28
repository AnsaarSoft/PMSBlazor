using Microsoft.EntityFrameworkCore;
using PMSModels.Models;

namespace PasswordManagement.Data;

public sealed class AccountContext : DbContext
{
    public AccountContext(DbContextOptions<AccountContext> options) : base(options)
    {
    }

    public DbSet<MstCard> MstCards => Set<MstCard>();
    public DbSet<MstUser> MstUsers => Set<MstUser>();
    public DbSet<MstSetting> MstSettings => Set<MstSetting>();
}
