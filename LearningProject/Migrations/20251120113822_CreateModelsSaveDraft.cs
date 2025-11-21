using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningProject.Migrations
{
    /// <inheritdoc />
    public partial class CreateModelsSaveDraft : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CerereDrafts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CerereDrafts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CerereFile",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CereriId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CerereFile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CerereFile_Cereri_CereriId",
                        column: x => x.CereriId,
                        principalTable: "Cereri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CerereFileDrafts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CerereDraftId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CerereFileDrafts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CerereFileDrafts_CerereDrafts_CerereDraftId",
                        column: x => x.CerereDraftId,
                        principalTable: "CerereDrafts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CerereFile_CereriId",
                table: "CerereFile",
                column: "CereriId");

            migrationBuilder.CreateIndex(
                name: "IX_CerereFileDrafts_CerereDraftId",
                table: "CerereFileDrafts",
                column: "CerereDraftId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CerereFile");

            migrationBuilder.DropTable(
                name: "CerereFileDrafts");

            migrationBuilder.DropTable(
                name: "CerereDrafts");
        }
    }
}
