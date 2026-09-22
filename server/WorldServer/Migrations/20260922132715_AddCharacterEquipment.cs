using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorldServer.Migrations
{
    /// <inheritdoc />
    public partial class AddCharacterEquipment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "character_equipment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CharacterId = table.Column<Guid>(type: "uuid", nullable: false),
                    Slot = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    ItemCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RefinementLevel = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsTradable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    EquippedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_character_equipment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_character_equipment_characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_character_equipment_CharacterId_Slot",
                table: "character_equipment",
                columns: new[] { "CharacterId", "Slot" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "character_equipment");
        }
    }
}
