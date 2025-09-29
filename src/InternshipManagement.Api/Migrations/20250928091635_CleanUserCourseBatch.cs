using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternshipManagement.Api.Migrations
{
    /// <inheritdoc />
    public partial class CleanUserCourseBatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Batches_BatchId",
                table: "Questions");

            migrationBuilder.AddColumn<int>(
                name: "BatchId",
                table: "UserCourses",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "BatchId",
                table: "Questions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_UserCourses_BatchId",
                table: "UserCourses",
                column: "BatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Batches_BatchId",
                table: "Questions",
                column: "BatchId",
                principalTable: "Batches",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserCourses_Batches_BatchId",
                table: "UserCourses",
                column: "BatchId",
                principalTable: "Batches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Batches_BatchId",
                table: "Questions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserCourses_Batches_BatchId",
                table: "UserCourses");

            migrationBuilder.DropIndex(
                name: "IX_UserCourses_BatchId",
                table: "UserCourses");

            migrationBuilder.DropColumn(
                name: "BatchId",
                table: "UserCourses");

            migrationBuilder.AlterColumn<int>(
                name: "BatchId",
                table: "Questions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Batches_BatchId",
                table: "Questions",
                column: "BatchId",
                principalTable: "Batches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
