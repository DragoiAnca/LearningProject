using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningProject.Migrations
{
    /// <inheritdoc />
    public partial class Inittest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClaimIdClaim",
                table: "User",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_User_ClaimIdClaim",
                table: "User",
                column: "ClaimIdClaim");

            migrationBuilder.AddForeignKey(
                name: "FK_User_Claim_ClaimIdClaim",
                table: "User",
                column: "ClaimIdClaim",
                principalTable: "Claim",
                principalColumn: "IdClaim",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_Claim_ClaimIdClaim",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_User_ClaimIdClaim",
                table: "User");

            migrationBuilder.DropColumn(
                name: "ClaimIdClaim",
                table: "User");
        }
    }
}
