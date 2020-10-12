using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sbanken.Data.Migrations
{
    public partial class RemoveTransactionCategory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_TransactionCategory_TransactionCategoryId",
                schema: "dbo",
                table: "Transaction");

            migrationBuilder.DropTable(
                name: "TransactionCategory",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_TransactionCategoryId",
                schema: "dbo",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "TransactionCategoryId",
                schema: "dbo",
                table: "Transaction");

            migrationBuilder.AddColumn<int>(
                name: "TransactionType",
                schema: "dbo",
                table: "Transaction",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TransactionType",
                schema: "dbo",
                table: "Transaction");

            migrationBuilder.AddColumn<long>(
                name: "TransactionCategoryId",
                schema: "dbo",
                table: "Transaction",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "TransactionCategory",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Items = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionCategory", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_TransactionCategoryId",
                schema: "dbo",
                table: "Transaction",
                column: "TransactionCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_TransactionCategory_TransactionCategoryId",
                schema: "dbo",
                table: "Transaction",
                column: "TransactionCategoryId",
                principalSchema: "dbo",
                principalTable: "TransactionCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
