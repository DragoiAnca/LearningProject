using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningProject.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDraftInCereri : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isDraft",
                table: "Cereri",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isDraft",
                table: "Cereri");
        }
    }
}
