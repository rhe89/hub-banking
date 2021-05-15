using Microsoft.EntityFrameworkCore.Migrations;

namespace Sbanken.Data.Migrations
{
    public partial class CommandRefactor2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommandType",
                schema: "dbo",
                table: "BackgroundTaskConfiguration");

            migrationBuilder.DropColumn(
                name: "RunIntervalType",
                schema: "dbo",
                table: "BackgroundTaskConfiguration");

            migrationBuilder.AddColumn<string>(
                name: "RunInterval",
                schema: "dbo",
                table: "BackgroundTaskConfiguration",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RunInterval",
                schema: "dbo",
                table: "BackgroundTaskConfiguration");

            migrationBuilder.AddColumn<string>(
                name: "CommandType",
                schema: "dbo",
                table: "BackgroundTaskConfiguration",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RunIntervalType",
                schema: "dbo",
                table: "BackgroundTaskConfiguration",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
