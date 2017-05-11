using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KaCake.Data.Migrations
{
    public partial class Tester : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TesterArchivePath",
                table: "TaskVariants",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PickedForTestingTimeUtc",
                table: "Submissions",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Submissions",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<byte[]>(
                name: "Summary",
                table: "Submissions",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TesterArchivePath",
                table: "TaskVariants");

            migrationBuilder.DropColumn(
                name: "PickedForTestingTimeUtc",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "Summary",
                table: "Submissions");
        }
    }
}
