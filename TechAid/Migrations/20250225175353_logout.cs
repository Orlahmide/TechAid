using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechAid.Migrations
{
    /// <inheritdoc />
    public partial class logout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefreshTokenExpiryTime",
                table: "Employees",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "RefreshTokenExpiryTime",
                table: "Employees");
        }
    }
}
