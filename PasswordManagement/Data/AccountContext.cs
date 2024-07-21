using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PMSModels.Models;
using System.Runtime.CompilerServices;

namespace PasswordManagement.Data
{
    public class AccountContext: DbContext
    {
        //protected override void OnConfiguring(DbContextOptionsBuilder builder) => builder.UseSqlite(@"Data Source=Data\accounts.db");

        public AccountContext(DbContextOptions<AccountContext> options):base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MstUser>().HasData(AdminUser());
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<MstCard> MstCards { get; set; }
        public DbSet<MstUser> MstUsers { get; set; }
        public DbSet<MstSetting> MstSettings { get; set; }
        

        private MstUser AdminUser()
        {
            MstUser user = new();
            user.Id = -1;
            user.UserCode = "mfm";
            user.Password = "bazinga*123";
            user.FullName = "Muhammad Faisal Maqsood";
            user.Email = "mfmlive@gmail.com";
            return user;
        }


    }
}
