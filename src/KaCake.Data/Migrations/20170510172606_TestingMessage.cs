using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KaCake.Data.Migrations
{
    public partial class TestingMessage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Summary",
                table: "Submissions");

            migrationBuilder.AddColumn<string>(
                name: "ReviewMessage",
                table: "Submissions",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReviewMessage",
                table: "Submissions");

            migrationBuilder.AddColumn<byte[]>(
                name: "Summary",
                table: "Submissions",
                nullable: true);
        }
    }
}
