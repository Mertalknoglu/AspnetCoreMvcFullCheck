using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AspnetCoreMvcFull.Migrations
{
    /// <inheritdoc />
    public partial class requestlogModify : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "RequestLogs");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "RequestLogs");

            migrationBuilder.RenameColumn(
                name: "Unit",
                table: "RequestLogs",
                newName: "ChangedBy");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "RequestLogs",
                newName: "ActionType");

            migrationBuilder.CreateIndex(
                name: "IX_RequestLogs_RequestId",
                table: "RequestLogs",
                column: "RequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_RequestLogs_Requests_RequestId",
                table: "RequestLogs",
                column: "RequestId",
                principalTable: "Requests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequestLogs_Requests_RequestId",
                table: "RequestLogs");

            migrationBuilder.DropIndex(
                name: "IX_RequestLogs_RequestId",
                table: "RequestLogs");

            migrationBuilder.RenameColumn(
                name: "ChangedBy",
                table: "RequestLogs",
                newName: "Unit");

            migrationBuilder.RenameColumn(
                name: "ActionType",
                table: "RequestLogs",
                newName: "Type");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "RequestLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "RequestLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
