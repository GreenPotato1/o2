using Microsoft.EntityFrameworkCore.Migrations;

namespace O2.ArenaS.Migrations
{
    public partial class addedAutoIncrement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Items",
                newName: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Items",
                newName: "ID");
        }
    }
}
