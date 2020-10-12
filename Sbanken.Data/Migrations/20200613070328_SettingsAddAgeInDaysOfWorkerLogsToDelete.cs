using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sbanken.Data.Migrations
{
    public partial class SettingsAddAgeInDaysOfWorkerLogsToDelete : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                $@"INSERT INTO dbo.Setting ([Key], [Value], [CreatedDate], [UpdatedDate]) 
                VALUES ('AgeInDaysOfWorkerLogsToDelete', '30', '{DateTime.Now}', '{DateTime.Now}')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        
        }
    }
}
