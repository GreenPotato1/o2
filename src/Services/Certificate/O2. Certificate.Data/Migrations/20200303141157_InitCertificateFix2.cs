using Microsoft.EntityFrameworkCore.Migrations;

namespace O2.Business.Data.Migrations
{
    public partial class InitCertificateFix2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "location_region",
                table: "O2CLocation",
                newName: "region");

            migrationBuilder.RenameColumn(
                name: "location_country",
                table: "O2CLocation",
                newName: "country");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "region",
                table: "O2CLocation",
                newName: "location_region");

            migrationBuilder.RenameColumn(
                name: "country",
                table: "O2CLocation",
                newName: "location_country");
        }
    }
}
