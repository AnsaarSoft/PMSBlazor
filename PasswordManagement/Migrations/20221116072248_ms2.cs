using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PasswordManagement.Migrations
{
    public partial class ms2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "MstUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "MstUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "MstCards",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "MstCards",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "MstUsers",
                columns: new[] { "Id", "Email", "FullName", "IsActive", "IsDeleted", "Password", "UserCode" },
                values: new object[] { -1, "mfmlive@gmail.com", "Muhammad Faisal Maqsood", true, false, "admin", "admin" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MstUsers",
                keyColumn: "Id",
                keyValue: -1);

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "MstUsers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "MstUsers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "MstCards");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "MstCards");
        }
    }
}
