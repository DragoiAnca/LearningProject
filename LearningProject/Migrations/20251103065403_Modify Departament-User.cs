using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningProject.Migrations
{
    /// <inheritdoc />
    public partial class ModifyDepartamentUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "roluriID",
                table: "User",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "data_dezactivare",
                table: "User",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "data_time",
                table: "User",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "data_dezactivare",
                table: "Departamente",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "data_inceput",
                table: "Departamente",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "isActive",
                table: "Departamente",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "data_dezactivare",
                table: "User");

            migrationBuilder.DropColumn(
                name: "data_time",
                table: "User");

            migrationBuilder.DropColumn(
                name: "data_dezactivare",
                table: "Departamente");

            migrationBuilder.DropColumn(
                name: "data_inceput",
                table: "Departamente");

            migrationBuilder.DropColumn(
                name: "isActive",
                table: "Departamente");

            migrationBuilder.AlterColumn<int>(
                name: "roluriID",
                table: "User",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
