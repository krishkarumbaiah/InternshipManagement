using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternshipManagement.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBatchToQuestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BatchId",
                table: "Questions",
                type: "int",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Questions_BatchId",
                table: "Questions",
                column: "BatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Batches_BatchId",
                table: "Questions",
                column: "BatchId",
                principalTable: "Batches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Batches_BatchId",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_BatchId",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "BatchId",
                table: "Questions");
        }
    }
}
