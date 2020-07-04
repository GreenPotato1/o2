using Microsoft.EntityFrameworkCore.Migrations;

namespace O2.Business.Data.Migrations
{
    public partial class InitCertificateFix5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_O2CPhoto",
                table: "O2CPhoto");

            migrationBuilder.AddColumn<string>(
                name: "fileName",
                table: "O2EvPhoto",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "fileName",
                table: "O2CPhoto",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_O2CPhoto",
                table: "O2CPhoto",
                columns: new[] { "id", "O2CCertificateId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_O2CPhoto",
                table: "O2CPhoto");

            migrationBuilder.DropColumn(
                name: "fileName",
                table: "O2EvPhoto");

            migrationBuilder.DropColumn(
                name: "fileName",
                table: "O2CPhoto");

            migrationBuilder.AddPrimaryKey(
                name: "PK_O2CPhoto",
                table: "O2CPhoto",
                column: "id");
        }
    }
}
