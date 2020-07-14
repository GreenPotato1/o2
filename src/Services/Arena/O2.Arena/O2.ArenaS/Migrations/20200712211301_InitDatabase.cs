using Microsoft.EntityFrameworkCore.Migrations;

namespace O2.ArenaS.Migrations
{
    public partial class InitDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Category = table.Column<string>(nullable: true),
                    Position = table.Column<int>(nullable: false),
                    Room = table.Column<int>(nullable: false),
                    RoomDescription = table.Column<string>(nullable: true),
                    RoomNumber = table.Column<int>(nullable: false),
                    SpecialNumber = table.Column<int>(nullable: false),
                    KeyCount = table.Column<int>(nullable: false),
                    Note = table.Column<string>(nullable: true),
                    RootType = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Items");
        }
    }
}
