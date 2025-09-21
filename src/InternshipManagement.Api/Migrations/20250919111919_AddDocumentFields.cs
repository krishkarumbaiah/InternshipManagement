using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternshipManagement.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UploadedById",
                table: "Documents");

            migrationBuilder.AddColumn<string>(
                name: "UploadedBy",
                table: "Documents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UploadedBy",
                table: "Documents");

            migrationBuilder.AddColumn<int>(
                name: "UploadedById",
                table: "Documents",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
