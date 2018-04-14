using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace URLShortener.Migrations
{
    public partial class UsersAndUrlsConnect : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Id",
                table: "Url",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Url_Id",
                table: "Url",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Url_AspNetUsers_Id",
                table: "Url",
                column: "Id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Url_AspNetUsers_Id",
                table: "Url");

            migrationBuilder.DropIndex(
                name: "IX_Url_Id",
                table: "Url");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Url");
        }
    }
}
