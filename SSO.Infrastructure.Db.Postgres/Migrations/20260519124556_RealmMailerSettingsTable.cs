using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSO.Infrastructure.Db.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class RealmMailerSettingsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RealmMailerSettings",
                columns: table => new
                {
                    RealmId = table.Column<Guid>(type: "uuid", nullable: false),
                    MailerType = table.Column<short>(type: "smallint", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RealmMailerSettings", x => x.RealmId);
                    table.ForeignKey(
                        name: "FK_RealmMailerSettings_Realms_RealmId",
                        column: x => x.RealmId,
                        principalTable: "Realms",
                        principalColumn: "RealmId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RealmMailerSettings");
        }
    }
}
