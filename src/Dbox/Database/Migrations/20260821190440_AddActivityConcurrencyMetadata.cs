using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dbox.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddActivityConcurrencyMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "activities",
                type: "TEXT",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<long>(
                name: "version",
                table: "activities",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "activities");

            migrationBuilder.DropColumn(
                name: "version",
                table: "activities");
        }
    }
}
