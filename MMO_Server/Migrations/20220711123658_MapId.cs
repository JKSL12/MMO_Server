using Microsoft.EntityFrameworkCore.Migrations;

namespace MMO_Server.Migrations
{
    public partial class MapId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MapId",
                table: "Player",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MapId",
                table: "Player");
        }
    }
}
