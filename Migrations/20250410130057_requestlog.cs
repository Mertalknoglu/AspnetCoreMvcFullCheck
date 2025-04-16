using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AspnetCoreMvcFull.Migrations
{
    /// <inheritdoc />
    public partial class requestlog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequestFilePaths_FilePaths_FilePathsId",
                table: "RequestFilePaths");

            migrationBuilder.DropTable(
                name: "FilePaths");

            migrationBuilder.DropIndex(
                name: "IX_RequestFilePaths_FilePathsId",
                table: "RequestFilePaths");

            migrationBuilder.DropColumn(
                name: "FilePathsId",
                table: "RequestFilePaths");

            migrationBuilder.AddColumn<string>(
                name: "FilePaths",
                table: "RequestFilePaths",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "RequestLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestLogs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RequestLogs");

            migrationBuilder.DropColumn(
                name: "FilePaths",
                table: "RequestFilePaths");

            migrationBuilder.AddColumn<int>(
                name: "FilePathsId",
                table: "RequestFilePaths",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "FilePaths",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Filepath = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilePaths", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RequestFilePaths_FilePathsId",
                table: "RequestFilePaths",
                column: "FilePathsId");

            migrationBuilder.AddForeignKey(
                name: "FK_RequestFilePaths_FilePaths_FilePathsId",
                table: "RequestFilePaths",
                column: "FilePathsId",
                principalTable: "FilePaths",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
