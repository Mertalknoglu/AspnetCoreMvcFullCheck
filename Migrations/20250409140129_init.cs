using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AspnetCoreMvcFull.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Requesters",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Requesters_UserId",
                table: "Requesters",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Requesters_AspNetUsers_UserId",
                table: "Requesters",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requesters_AspNetUsers_UserId",
                table: "Requesters");

            migrationBuilder.DropIndex(
                name: "IX_Requesters_UserId",
                table: "Requesters");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Requesters");
        }
    }
}
