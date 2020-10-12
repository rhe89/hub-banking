using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sbanken.Data.Migrations
{
    public partial class UpdateSettingsAddTransactionDatesForWorker : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                $@"INSERT INTO dbo.Setting ([Key], [Value], [CreatedDate], [UpdatedDate]) 
                VALUES ('TransactionsStartDate', '{DateTime.Now.AddDays(-365)}', '{DateTime.Now}', '{DateTime.Now}'),
                       ('TransactionsEndDate', NULL, '{DateTime.Now}', '{DateTime.Now}')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
