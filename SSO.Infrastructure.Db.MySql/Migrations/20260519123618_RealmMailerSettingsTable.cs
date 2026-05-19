using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSO.Infrastructure.Db.MySql.Migrations
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
                    RealmId = table.Column<Guid>(type: "char(36)", nullable: false),
                    MailerType = table.Column<sbyte>(type: "tinyint", nullable: false),
                    Value = table.Column<string>(type: "longtext", nullable: false)
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
                })
                .Annotation("MySQL:Charset", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RealmMailerSettings");
        }
    }
}
