using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sbanken.Data.Migrations
{
    public partial class SettingPopulate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                $@"INSERT INTO dbo.Setting ([Key], [Value], [CreatedDate]) 
                VALUES ('SbankenApiClientId', '', '{DateTime.Now}'),
                       ('SbankenApiSecret', '', '{DateTime.Now}'),
                       ('SbankenApiCustomerId', '', '{DateTime.Now}'),
                       ('SbankenApiDiscoveryEndpoint', '', '{DateTime.Now}'),
                       ('SbankenApiBaseAdress', '', '{DateTime.Now}'),
                       ('SbankenApiBankBasePath', '', '{DateTime.Now}'),
                       ('StorageAccount', '', '{DateTime.Now}'),
                       ('SbankenWorkerAccountRunInterval', '', '{DateTime.Now}')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
