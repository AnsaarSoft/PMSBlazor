using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PasswordManagement.Data;

#nullable disable

namespace PasswordManagement.Migrations
{
    [DbContext(typeof(AccountContext))]
    [Migration("20260627000000_RenameSettingsProperties")]
    public partial class RenameSettingsProperties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PasswodLenght",
                table: "MstSettings",
                newName: "PasswordLength");

            migrationBuilder.RenameColumn(
                name: "flgCapitalLetter",
                table: "MstSettings",
                newName: "UseUppercase");

            migrationBuilder.RenameColumn(
                name: "flgSmallLetter",
                table: "MstSettings",
                newName: "UseLowercase");

            migrationBuilder.RenameColumn(
                name: "flgNumbers",
                table: "MstSettings",
                newName: "UseNumbers");

            migrationBuilder.RenameColumn(
                name: "flgSpecial",
                table: "MstSettings",
                newName: "UseSymbols");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PasswordLength",
                table: "MstSettings",
                newName: "PasswodLenght");

            migrationBuilder.RenameColumn(
                name: "UseUppercase",
                table: "MstSettings",
                newName: "flgCapitalLetter");

            migrationBuilder.RenameColumn(
                name: "UseLowercase",
                table: "MstSettings",
                newName: "flgSmallLetter");

            migrationBuilder.RenameColumn(
                name: "UseNumbers",
                table: "MstSettings",
                newName: "flgNumbers");

            migrationBuilder.RenameColumn(
                name: "UseSymbols",
                table: "MstSettings",
                newName: "flgSpecial");
        }
    }
}
