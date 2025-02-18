using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechAid.Migrations
{
    /// <inheritdoc />
    public partial class FixITPersonelRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ITPersonelId",
                table: "Tickets",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ITPersonels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone_number = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    First_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Last_name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ITPersonels", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ITPersonelId",
                table: "Tickets",
                column: "ITPersonelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_ITPersonels_ITPersonelId",
                table: "Tickets",
                column: "ITPersonelId",
                principalTable: "ITPersonels",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_ITPersonels_ITPersonelId",
                table: "Tickets");

            migrationBuilder.DropTable(
                name: "ITPersonels");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_ITPersonelId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "ITPersonelId",
                table: "Tickets");
        }
    }
}
