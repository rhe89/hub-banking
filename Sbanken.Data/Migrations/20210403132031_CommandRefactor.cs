using Microsoft.EntityFrameworkCore.Migrations;

namespace Sbanken.Data.Migrations
{
    public partial class CommandRefactor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                schema: "dbo",
                table: "WorkerLog");

            migrationBuilder.AddColumn<string>(
                name: "CommandName",
                schema: "dbo",
                table: "WorkerLog",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CommandType",
                schema: "dbo",
                table: "BackgroundTaskConfiguration",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommandName",
                schema: "dbo",
                table: "WorkerLog");

            migrationBuilder.DropColumn(
                name: "CommandType",
                schema: "dbo",
                table: "BackgroundTaskConfiguration");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "dbo",
                table: "WorkerLog",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
