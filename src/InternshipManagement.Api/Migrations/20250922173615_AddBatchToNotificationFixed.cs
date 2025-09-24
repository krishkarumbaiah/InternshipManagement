using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternshipManagement.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBatchToNotificationFixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Batches_BatchId",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<int>(
                name: "BatchId",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_BatchId",
                table: "Notifications",
                column: "BatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Batches_BatchId",
                table: "AspNetUsers",
                column: "BatchId",
                principalTable: "Batches",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Batches_BatchId",
                table: "Notifications",
                column: "BatchId",
                principalTable: "Batches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Batches_BatchId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Batches_BatchId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_BatchId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "BatchId",
                table: "Notifications");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Batches_BatchId",
                table: "AspNetUsers",
                column: "BatchId",
                principalTable: "Batches",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
