using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sbanken.Data.Migrations
{
    public partial class SettingAddRunIntervalsForWorkers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM dbo.Setting WHERE [Key] = 'SbankenWorkerAccountRunInterval'");
            migrationBuilder.Sql("DELETE FROM dbo.Setting WHERE [Key] = 'StorageAccount'");

            migrationBuilder.Sql(
                $@"INSERT INTO dbo.Setting ([Key], [Value], [CreatedDate]) 
                VALUES ('SbankenApiCustomersBasePath', '', '{DateTime.Now}'),
                       ('AccountWorkerRunInterval', '60', '{DateTime.Now}'),
                       ('TransactionWorkerRunInterval', '1440', '{DateTime.Now}')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
