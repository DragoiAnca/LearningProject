using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningProject.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Claim",
                columns: table => new
                {
                    IdClaim = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Claim", x => x.IdClaim);
                });

            migrationBuilder.CreateTable(
                name: "Departamente",
                columns: table => new
                {
                    id_departamente = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Denumire_departament = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departamente", x => x.id_departamente);
                });

            migrationBuilder.CreateTable(
                name: "Roluri",
                columns: table => new
                {
                    IdRol = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Denumire_rol = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roluri", x => x.IdRol);
                });

            migrationBuilder.CreateTable(
                name: "ClaimRoluri",
                columns: table => new
                {
                    ClaimsIdClaim = table.Column<int>(type: "int", nullable: false),
                    RolesIdRol = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClaimRoluri", x => new { x.ClaimsIdClaim, x.RolesIdRol });
                    table.ForeignKey(
                        name: "FK_ClaimRoluri_Claim_ClaimsIdClaim",
                        column: x => x.ClaimsIdClaim,
                        principalTable: "Claim",
                        principalColumn: "IdClaim",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClaimRoluri_Roluri_RolesIdRol",
                        column: x => x.RolesIdRol,
                        principalTable: "Roluri",
                        principalColumn: "IdRol",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    IdUser = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    id_departament = table.Column<int>(type: "int", nullable: false),
                    roluriID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.IdUser);
                    table.ForeignKey(
                        name: "FK_User_Departamente_id_departament",
                        column: x => x.id_departament,
                        principalTable: "Departamente",
                        principalColumn: "id_departamente",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_User_Roluri_roluriID",
                        column: x => x.roluriID,
                        principalTable: "Roluri",
                        principalColumn: "IdRol",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClaimRoluri_RolesIdRol",
                table: "ClaimRoluri",
                column: "RolesIdRol");

            migrationBuilder.CreateIndex(
                name: "IX_User_id_departament",
                table: "User",
                column: "id_departament");

            migrationBuilder.CreateIndex(
                name: "IX_User_roluriID",
                table: "User",
                column: "roluriID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClaimRoluri");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Claim");

            migrationBuilder.DropTable(
                name: "Departamente");

            migrationBuilder.DropTable(
                name: "Roluri");
        }
    }
}
