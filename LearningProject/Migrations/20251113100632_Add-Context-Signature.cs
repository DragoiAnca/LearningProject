using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningProject.Migrations
{
    /// <inheritdoc />
    public partial class AddContextSignature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Signature_Cereri_CerereId",
                table: "Signature");

            migrationBuilder.DropForeignKey(
                name: "FK_Signature_Claim_ClaimCanSignId",
                table: "Signature");

            migrationBuilder.DropForeignKey(
                name: "FK_Signature_User_SignByUserId",
                table: "Signature");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Signature",
                table: "Signature");

            migrationBuilder.RenameTable(
                name: "Signature",
                newName: "Signatures");

            migrationBuilder.RenameIndex(
                name: "IX_Signature_SignByUserId",
                table: "Signatures",
                newName: "IX_Signatures_SignByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Signature_ClaimCanSignId",
                table: "Signatures",
                newName: "IX_Signatures_ClaimCanSignId");

            migrationBuilder.RenameIndex(
                name: "IX_Signature_CerereId",
                table: "Signatures",
                newName: "IX_Signatures_CerereId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Signatures",
                table: "Signatures",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Signatures_Cereri_CerereId",
                table: "Signatures",
                column: "CerereId",
                principalTable: "Cereri",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Signatures_Claim_ClaimCanSignId",
                table: "Signatures",
                column: "ClaimCanSignId",
                principalTable: "Claim",
                principalColumn: "IdClaim",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Signatures_User_SignByUserId",
                table: "Signatures",
                column: "SignByUserId",
                principalTable: "User",
                principalColumn: "IdUser");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Signatures_Cereri_CerereId",
                table: "Signatures");

            migrationBuilder.DropForeignKey(
                name: "FK_Signatures_Claim_ClaimCanSignId",
                table: "Signatures");

            migrationBuilder.DropForeignKey(
                name: "FK_Signatures_User_SignByUserId",
                table: "Signatures");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Signatures",
                table: "Signatures");

            migrationBuilder.RenameTable(
                name: "Signatures",
                newName: "Signature");

            migrationBuilder.RenameIndex(
                name: "IX_Signatures_SignByUserId",
                table: "Signature",
                newName: "IX_Signature_SignByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Signatures_ClaimCanSignId",
                table: "Signature",
                newName: "IX_Signature_ClaimCanSignId");

            migrationBuilder.RenameIndex(
                name: "IX_Signatures_CerereId",
                table: "Signature",
                newName: "IX_Signature_CerereId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Signature",
                table: "Signature",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Signature_Cereri_CerereId",
                table: "Signature",
                column: "CerereId",
                principalTable: "Cereri",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Signature_Claim_ClaimCanSignId",
                table: "Signature",
                column: "ClaimCanSignId",
                principalTable: "Claim",
                principalColumn: "IdClaim",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Signature_User_SignByUserId",
                table: "Signature",
                column: "SignByUserId",
                principalTable: "User",
                principalColumn: "IdUser");
        }
    }
}
