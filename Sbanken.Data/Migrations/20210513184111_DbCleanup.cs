using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sbanken.Data.Migrations
{
    public partial class DbCleanup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommandLog",
                schema: "dbo");
            
            migrationBuilder.Sql("DELETE FROM dbo.CommandConfiguration");
            
            migrationBuilder.Sql(@"DELETE FROM dbo.Account
            WHERE Name IS NULL");


            migrationBuilder.Sql(@"UPDATE dbo.Setting
                                   SET [Key] = 'SbankenApiBasePath'
                                   WHERE [Key] = 'SbankenApiBankBasePath'");
            migrationBuilder.Sql(@"
             DELETE FROM dbo.Setting
             WHERE [Key] = 'AccountWorkerRunInterval' OR
                   [Key] = 'TransactionWorkerRunInterval' OR
                   [Key] = 'WorkerLogMaintenanceRunInterval' OR
                   [Key] = 'SbankenApiCustomersBasePath' OR
                   [Key] = 'StorageAccount'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommandLog",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommandName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InitiatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Success = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommandLog", x => x.Id);
                });
        }
    }
}
