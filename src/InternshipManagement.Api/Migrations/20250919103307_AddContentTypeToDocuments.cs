using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternshipManagement.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddContentTypeToDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UploadedBy",
                table: "Documents",
                newName: "ContentType");

            migrationBuilder.AddColumn<int>(
                name: "UploadedById",
                table: "Documents",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UploadedById",
                table: "Documents");

            migrationBuilder.RenameColumn(
                name: "ContentType",
                table: "Documents",
                newName: "UploadedBy");
        }
    }
}
