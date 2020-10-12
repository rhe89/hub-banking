using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Sbanken.Data.Migrations
{
    public partial class AddTransactionCategories : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                INSERT INTO dbo.TransactionCategory (Name, Items, CreatedDate)
                VALUES ('Daglivarer og snus', 'Kiwi,Bunnpris,Rema,Joker,Makroboks,Matkroken,Rimi,ICA', '{DateTime.Now}'),
                       ('Byliv', 'Tilt,Syng,Elmers,Justisen,Dattera,John Dee,Bruunlarsen,Oche,The Highbury Pub,Dubliner,Debatten,Albatross,Bohemen,Oslo Camping,Couch', '{DateTime.Now}'),
                       ('Kollektivt', 'ENTUR,Ruter', '{DateTime.Now}'),
                       ('Elektrisk sparkesykkel', 'Tier,Voi,Lime', '{DateTime.Now}'),
                       ('Betting', 'Norsk tipping', '{DateTime.Now}'),
                       ('Kebab/Fastfood', 'Bislett Kebab,Burger King,Max', '{DateTime.Now}'),
                       ('Hjem', 'Ikea,Clas', '{DateTime.Now}'),
                       ('Klær, sport og friluftsliv', 'XXL,Zalando,Fjellsport,Dressmann,G Max,G Sport', '{DateTime.Now}'),
                       ('Streaming', 'Viaplay,Eurosport,HBO,Netflix,Amazon Video,Strive,Discoverynetworks', '{DateTime.Now}'),
                       ('Elektronikk', 'Komplett,Elkjoep,Power', '{DateTime.Now}'),
                       ('Kultur', 'Kino,Billettservice,Odeon,Det andre teatret', '{DateTime.Now}'),
                       ('Mikrospar', 'Mikrospar', '{DateTime.Now}')
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
