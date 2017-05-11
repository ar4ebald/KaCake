using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KaCake.Data.Migrations
{
    public partial class RemoveTesterPath : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TesterArchivePath",
                table: "TaskVariants");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TesterArchivePath",
                table: "TaskVariants",
                nullable: true);
        }
    }
}
