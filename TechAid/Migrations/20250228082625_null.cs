using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechAid.Migrations
{
    /// <inheritdoc />
    public partial class @null : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Attachment",
                table: "Tickets",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

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

            migrationBuilder.AlterColumn<string>(
                name: "Attachment",
                table: "Tickets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
