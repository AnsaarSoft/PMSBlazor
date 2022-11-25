using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PasswordManagement.Migrations
{
    public partial class ms1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstCards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CardName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Alias = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    UserCode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Password = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Remarks = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    WebLink = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstCards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MstSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    flgCapitalLetter = table.Column<bool>(type: "INTEGER", nullable: false),
                    flgSmallLetter = table.Column<bool>(type: "INTEGER", nullable: false),
                    flgNumbers = table.Column<bool>(type: "INTEGER", nullable: false),
                    flgSpecial = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswodLenght = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MstUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    UserCode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Password = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstUsers", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstCards");

            migrationBuilder.DropTable(
                name: "MstSettings");

            migrationBuilder.DropTable(
                name: "MstUsers");
        }
    }
}
