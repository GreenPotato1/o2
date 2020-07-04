using Microsoft.EntityFrameworkCore.Migrations;

namespace O2.Business.Data.Migrations
{
    public partial class InitCertificateFix7 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "lock",
                table: "O2CCertificate",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "lock",
                table: "O2CCertificate");
        }
    }
}
