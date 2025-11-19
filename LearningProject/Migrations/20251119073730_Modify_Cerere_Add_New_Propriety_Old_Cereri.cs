using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningProject.Migrations
{
    /// <inheritdoc />
    public partial class Modify_Cerere_Add_New_Propriety_Old_Cereri : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OldCereriId",
                table: "Cereri",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VersionNumber",
                table: "Cereri",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cereri_OldCereriId",
                table: "Cereri",
                column: "OldCereriId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cereri_Cereri_OldCereriId",
                table: "Cereri",
                column: "OldCereriId",
                principalTable: "Cereri",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cereri_Cereri_OldCereriId",
                table: "Cereri");

            migrationBuilder.DropIndex(
                name: "IX_Cereri_OldCereriId",
                table: "Cereri");

            migrationBuilder.DropColumn(
                name: "OldCereriId",
                table: "Cereri");

            migrationBuilder.DropColumn(
                name: "VersionNumber",
                table: "Cereri");
        }
    }
}
