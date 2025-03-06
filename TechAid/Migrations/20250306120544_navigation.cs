using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechAid.Migrations
{
    /// <inheritdoc />
    public partial class navigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Tickets_It_PersonnelId",
                table: "Tickets",
                column: "It_PersonnelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Employees_It_PersonnelId",
                table: "Tickets",
                column: "It_PersonnelId",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Employees_It_PersonnelId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_It_PersonnelId",
                table: "Tickets");
        }
    }
}
