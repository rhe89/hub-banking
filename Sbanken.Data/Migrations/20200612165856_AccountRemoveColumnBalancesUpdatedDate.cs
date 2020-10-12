using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sbanken.Data.Migrations
{
    public partial class AccountRemoveColumnBalancesUpdatedDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentBalanceUpdated",
                schema: "dbo",
                table: "Account");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CurrentBalanceUpdated",
                schema: "dbo",
                table: "Account",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
