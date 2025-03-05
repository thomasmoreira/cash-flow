using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CashFlow.Infraestructure.Migrations
{
    /// <inheritdoc />
    public partial class DateWithoutTZ : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "transaction",
                type: "varchar",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "transaction",
                type: "timestamp",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamptz");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "dailyconsolidation",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "transaction",
                type: "VARCHAR",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "transaction",
                type: "timestamptz",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "dailyconsolidation",
                type: "DATE",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");
        }
    }
}
