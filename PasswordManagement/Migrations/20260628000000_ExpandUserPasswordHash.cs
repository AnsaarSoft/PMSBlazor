using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PasswordManagement.Data;

#nullable disable

namespace PasswordManagement.Migrations;

[DbContext(typeof(AccountContext))]
[Migration("20260628000000_ExpandUserPasswordHash")]
public sealed class ExpandUserPasswordHash : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // SQLite stores these values as TEXT and does not enforce StringLength.
        // The expanded limits are represented in the EF model snapshot without
        // requiring a destructive table rebuild.
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
