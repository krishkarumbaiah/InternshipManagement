using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternshipManagement.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBatchToMeetings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BatchId",
                table: "Meetings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_BatchId",
                table: "Meetings",
                column: "BatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_Meetings_Batches_BatchId",
                table: "Meetings",
                column: "BatchId",
                principalTable: "Batches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Meetings_Batches_BatchId",
                table: "Meetings");

            migrationBuilder.DropIndex(
                name: "IX_Meetings_BatchId",
                table: "Meetings");

            migrationBuilder.DropColumn(
                name: "BatchId",
                table: "Meetings");
        }
    }
}
