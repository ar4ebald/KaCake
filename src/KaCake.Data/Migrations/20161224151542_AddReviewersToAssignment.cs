using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KaCake.Data.Migrations
{
    public partial class AddReviewersToAssignment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReviewerId",
                table: "Assignments",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_ReviewerId",
                table: "Assignments",
                column: "ReviewerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assignments_AspNetUsers_ReviewerId",
                table: "Assignments",
                column: "ReviewerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assignments_AspNetUsers_ReviewerId",
                table: "Assignments");

            migrationBuilder.DropIndex(
                name: "IX_Assignments_ReviewerId",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "ReviewerId",
                table: "Assignments");
        }
    }
}
