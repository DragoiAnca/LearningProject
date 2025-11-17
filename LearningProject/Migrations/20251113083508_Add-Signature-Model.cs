using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningProject.Migrations
{
    /// <inheritdoc />
    public partial class AddSignatureModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Signature",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    order = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DataSemnarii = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CerereId = table.Column<int>(type: "int", nullable: false),
                    ClaimCanSignId = table.Column<int>(type: "int", nullable: false),
                    SignByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Signature", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Signature_Cereri_CerereId",
                        column: x => x.CerereId,
                        principalTable: "Cereri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Signature_Claim_ClaimCanSignId",
                        column: x => x.ClaimCanSignId,
                        principalTable: "Claim",
                        principalColumn: "IdClaim",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Signature_User_SignByUserId",
                        column: x => x.SignByUserId,
                        principalTable: "User",
                        principalColumn: "IdUser");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Signature_CerereId",
                table: "Signature",
                column: "CerereId");

            migrationBuilder.CreateIndex(
                name: "IX_Signature_ClaimCanSignId",
                table: "Signature",
                column: "ClaimCanSignId");

            migrationBuilder.CreateIndex(
                name: "IX_Signature_SignByUserId",
                table: "Signature",
                column: "SignByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Signature");
        }
    }
}
