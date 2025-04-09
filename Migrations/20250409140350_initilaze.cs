using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AspnetCoreMvcFull.Migrations
{
    /// <inheritdoc />
    public partial class initilaze : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requesters_AspNetUsers_UserId",
                table: "Requesters");

            migrationBuilder.DropForeignKey(
                name: "FK_Requesters_RequestStatuses_RequestStatusId",
                table: "Requesters");

            migrationBuilder.DropForeignKey(
                name: "FK_Requesters_RequestTypes_RequestTypeId",
                table: "Requesters");

            migrationBuilder.DropForeignKey(
                name: "FK_Requesters_RequestUnits_RequestUnitId",
                table: "Requesters");

            migrationBuilder.DropForeignKey(
                name: "FK_RequestFilePaths_Requesters_RequestDetailsId",
                table: "RequestFilePaths");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Requesters",
                table: "Requesters");

            migrationBuilder.RenameTable(
                name: "Requesters",
                newName: "Requests");

            migrationBuilder.RenameIndex(
                name: "IX_Requesters_UserId",
                table: "Requests",
                newName: "IX_Requests_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Requesters_RequestUnitId",
                table: "Requests",
                newName: "IX_Requests_RequestUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_Requesters_RequestTypeId",
                table: "Requests",
                newName: "IX_Requests_RequestTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Requesters_RequestStatusId",
                table: "Requests",
                newName: "IX_Requests_RequestStatusId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Requests",
                table: "Requests",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RequestFilePaths_Requests_RequestDetailsId",
                table: "RequestFilePaths",
                column: "RequestDetailsId",
                principalTable: "Requests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_AspNetUsers_UserId",
                table: "Requests",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_RequestStatuses_RequestStatusId",
                table: "Requests",
                column: "RequestStatusId",
                principalTable: "RequestStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_RequestTypes_RequestTypeId",
                table: "Requests",
                column: "RequestTypeId",
                principalTable: "RequestTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_RequestUnits_RequestUnitId",
                table: "Requests",
                column: "RequestUnitId",
                principalTable: "RequestUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequestFilePaths_Requests_RequestDetailsId",
                table: "RequestFilePaths");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_AspNetUsers_UserId",
                table: "Requests");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_RequestStatuses_RequestStatusId",
                table: "Requests");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_RequestTypes_RequestTypeId",
                table: "Requests");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_RequestUnits_RequestUnitId",
                table: "Requests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Requests",
                table: "Requests");

            migrationBuilder.RenameTable(
                name: "Requests",
                newName: "Requesters");

            migrationBuilder.RenameIndex(
                name: "IX_Requests_UserId",
                table: "Requesters",
                newName: "IX_Requesters_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Requests_RequestUnitId",
                table: "Requesters",
                newName: "IX_Requesters_RequestUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_Requests_RequestTypeId",
                table: "Requesters",
                newName: "IX_Requesters_RequestTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Requests_RequestStatusId",
                table: "Requesters",
                newName: "IX_Requesters_RequestStatusId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Requesters",
                table: "Requesters",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Requesters_AspNetUsers_UserId",
                table: "Requesters",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Requesters_RequestStatuses_RequestStatusId",
                table: "Requesters",
                column: "RequestStatusId",
                principalTable: "RequestStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Requesters_RequestTypes_RequestTypeId",
                table: "Requesters",
                column: "RequestTypeId",
                principalTable: "RequestTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Requesters_RequestUnits_RequestUnitId",
                table: "Requesters",
                column: "RequestUnitId",
                principalTable: "RequestUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RequestFilePaths_Requesters_RequestDetailsId",
                table: "RequestFilePaths",
                column: "RequestDetailsId",
                principalTable: "Requesters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
