using Microsoft.EntityFrameworkCore.Migrations;

namespace Sbanken.Data.Migrations
{
    public partial class CommandRefactor3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkerLog",
                schema: "dbo",
                table: "WorkerLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BackgroundTaskConfiguration",
                schema: "dbo",
                table: "BackgroundTaskConfiguration");

            migrationBuilder.RenameTable(
                name: "WorkerLog",
                schema: "dbo",
                newName: "CommandLog",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "BackgroundTaskConfiguration",
                schema: "dbo",
                newName: "CommandConfiguration",
                newSchema: "dbo");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CommandLog",
                schema: "dbo",
                table: "CommandLog",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CommandConfiguration",
                schema: "dbo",
                table: "CommandConfiguration",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CommandLog",
                schema: "dbo",
                table: "CommandLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CommandConfiguration",
                schema: "dbo",
                table: "CommandConfiguration");

            migrationBuilder.RenameTable(
                name: "CommandLog",
                schema: "dbo",
                newName: "WorkerLog",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "CommandConfiguration",
                schema: "dbo",
                newName: "BackgroundTaskConfiguration",
                newSchema: "dbo");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkerLog",
                schema: "dbo",
                table: "WorkerLog",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BackgroundTaskConfiguration",
                schema: "dbo",
                table: "BackgroundTaskConfiguration",
                column: "Id");
        }
    }
}
