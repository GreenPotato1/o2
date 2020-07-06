using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace O2.Certificate.Data.Migrations
{
    public partial class InitDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "O2CCertificate",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    added_date = table.Column<long>(nullable: false),
                    modified_date = table.Column<long>(nullable: false),
                    serial = table.Column<string>(maxLength: 1, nullable: true),
                    short_number = table.Column<int>(nullable: false),
                    number = table.Column<string>(maxLength: 10, nullable: true),
                    date_of_cert = table.Column<long>(nullable: true),
                    visible = table.Column<bool>(nullable: true),
                    firstname = table.Column<string>(maxLength: 255, nullable: true),
                    lastname = table.Column<string>(maxLength: 255, nullable: true),
                    middlename = table.Column<string>(maxLength: 255, nullable: true),
                    education = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_O2CCertificate", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "O2CLocation",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    added_date = table.Column<long>(nullable: false),
                    modified_date = table.Column<long>(nullable: false),
                    location_country = table.Column<string>(maxLength: 255, nullable: true),
                    location_region = table.Column<string>(maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_O2CLocation", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "O2EvEvent",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    added_date = table.Column<long>(nullable: false),
                    modified_date = table.Column<long>(nullable: false),
                    title = table.Column<string>(nullable: true),
                    short_description = table.Column<string>(nullable: true),
                    start_date = table.Column<long>(nullable: false),
                    end_date = table.Column<long>(nullable: false),
                    all_day = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_O2EvEvent", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "O2CContact",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    added_date = table.Column<long>(nullable: false),
                    modified_date = table.Column<long>(nullable: false),
                    contact_key = table.Column<string>(nullable: true),
                    contact_value = table.Column<string>(nullable: true),
                    O2CCertificateId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_O2CContact", x => x.id);
                    table.ForeignKey(
                        name: "FK_O2CContact_O2CCertificate_O2CCertificateId",
                        column: x => x.O2CCertificateId,
                        principalTable: "O2CCertificate",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "O2CPhoto",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    added_date = table.Column<long>(nullable: false),
                    modified_date = table.Column<long>(nullable: false),
                    url = table.Column<string>(nullable: true),
                    isMain = table.Column<bool>(nullable: false),
                    O2CCertificateId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_O2CPhoto", x => x.id);
                    table.ForeignKey(
                        name: "FK_O2CPhoto_O2CCertificate_O2CCertificateId",
                        column: x => x.O2CCertificateId,
                        principalTable: "O2CCertificate",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "O2CCertificateLocation",
                columns: table => new
                {
                    O2CCertificateId = table.Column<Guid>(nullable: false),
                    O2CLocationId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_O2CCertificateLocation", x => new { x.O2CLocationId, x.O2CCertificateId });
                    table.ForeignKey(
                        name: "FK_O2CCertificateLocation_O2CCertificate_O2CCertificateId",
                        column: x => x.O2CCertificateId,
                        principalTable: "O2CCertificate",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_O2CCertificateLocation_O2CLocation_O2CLocationId",
                        column: x => x.O2CLocationId,
                        principalTable: "O2CLocation",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "O2EvMeta",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    added_date = table.Column<long>(nullable: false),
                    modified_date = table.Column<long>(nullable: false),
                    country = table.Column<string>(nullable: true),
                    region = table.Column<string>(nullable: true),
                    event_id = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_O2EvMeta", x => x.id);
                    table.ForeignKey(
                        name: "FK_O2EvMeta_O2EvEvent_event_id",
                        column: x => x.event_id,
                        principalTable: "O2EvEvent",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "O2EvPhoto",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    added_date = table.Column<long>(nullable: false),
                    modified_date = table.Column<long>(nullable: false),
                    url = table.Column<string>(nullable: true),
                    isMain = table.Column<bool>(nullable: false),
                    O2EvEventId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_O2EvPhoto", x => x.id);
                    table.ForeignKey(
                        name: "FK_O2EvPhoto_O2EvEvent_O2EvEventId",
                        column: x => x.O2EvEventId,
                        principalTable: "O2EvEvent",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_O2CCertificateLocation_O2CCertificateId",
                table: "O2CCertificateLocation",
                column: "O2CCertificateId");

            migrationBuilder.CreateIndex(
                name: "IX_O2CContact_O2CCertificateId",
                table: "O2CContact",
                column: "O2CCertificateId");

            migrationBuilder.CreateIndex(
                name: "IX_O2CPhoto_O2CCertificateId",
                table: "O2CPhoto",
                column: "O2CCertificateId");

            migrationBuilder.CreateIndex(
                name: "IX_O2EvMeta_event_id",
                table: "O2EvMeta",
                column: "event_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_O2EvPhoto_O2EvEventId",
                table: "O2EvPhoto",
                column: "O2EvEventId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "O2CCertificateLocation");

            migrationBuilder.DropTable(
                name: "O2CContact");

            migrationBuilder.DropTable(
                name: "O2CPhoto");

            migrationBuilder.DropTable(
                name: "O2EvMeta");

            migrationBuilder.DropTable(
                name: "O2EvPhoto");

            migrationBuilder.DropTable(
                name: "O2CLocation");

            migrationBuilder.DropTable(
                name: "O2CCertificate");

            migrationBuilder.DropTable(
                name: "O2EvEvent");
        }
    }
}
