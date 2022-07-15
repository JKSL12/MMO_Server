using Microsoft.EntityFrameworkCore.Migrations;

namespace MMO_Server.Migrations
{
    public partial class stat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BonusPoint",
                table: "Player",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Dex",
                table: "Player",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Mag",
                table: "Player",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Str",
                table: "Player",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Vit",
                table: "Player",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BonusPoint",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "Dex",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "Mag",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "Str",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "Vit",
                table: "Player");
        }
    }
}
