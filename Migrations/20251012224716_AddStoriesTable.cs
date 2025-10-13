using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexusGram.Migrations
{
    /// <inheritdoc />
    public partial class AddStoriesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "Stories");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Stories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Comments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Stories");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Comments");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "Stories",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
