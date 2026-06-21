using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetShow.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizerToShow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OrganizerId",
                table: "Shows",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shows_OrganizerId",
                table: "Shows",
                column: "OrganizerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Shows_AspNetUsers_OrganizerId",
                table: "Shows",
                column: "OrganizerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shows_AspNetUsers_OrganizerId",
                table: "Shows");

            migrationBuilder.DropIndex(
                name: "IX_Shows_OrganizerId",
                table: "Shows");

            migrationBuilder.DropColumn(
                name: "OrganizerId",
                table: "Shows");
        }
    }
}
