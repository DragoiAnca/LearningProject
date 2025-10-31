using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningProject.Migrations
{
    /// <inheritdoc />
    public partial class ModifyprimarykeywithoutRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
    }
}
