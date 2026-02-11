using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevHabit.Api.Migrations.Application;

/// <inheritdoc />
public partial class AddUserIdReference : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DELETE FROM "DevHabit"."HabitTags";
            DELETE FROM "DevHabit"."Habits";
            DELETE FROM "DevHabit"."Tags";
            """);

        migrationBuilder.DropIndex(
            name: "IX_Tags_Name",
            schema: "DevHabit",
            table: "Tags");

        migrationBuilder.AddColumn<string>(
            name: "UserId",
            schema: "DevHabit",
            table: "Tags",
            type: "character varying(500)",
            maxLength: 500,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "UserId",
            schema: "DevHabit",
            table: "Habits",
            type: "character varying(500)",
            maxLength: 500,
            nullable: false,
            defaultValue: "");

        migrationBuilder.CreateIndex(
            name: "IX_Tags_UserId_Name",
            schema: "DevHabit",
            table: "Tags",
            columns: ["UserId", "Name"],
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Habits_UserId",
            schema: "DevHabit",
            table: "Habits",
            column: "UserId");

        migrationBuilder.AddForeignKey(
            name: "FK_Habits_Users_UserId",
            schema: "DevHabit",
            table: "Habits",
            column: "UserId",
            principalSchema: "DevHabit",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_Tags_Users_UserId",
            schema: "DevHabit",
            table: "Tags",
            column: "UserId",
            principalSchema: "DevHabit",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Habits_Users_UserId",
            schema: "DevHabit",
            table: "Habits");

        migrationBuilder.DropForeignKey(
            name: "FK_Tags_Users_UserId",
            schema: "DevHabit",
            table: "Tags");

        migrationBuilder.DropIndex(
            name: "IX_Tags_UserId_Name",
            schema: "DevHabit",
            table: "Tags");

        migrationBuilder.DropIndex(
            name: "IX_Habits_UserId",
            schema: "DevHabit",
            table: "Habits");

        migrationBuilder.DropColumn(
            name: "UserId",
            schema: "DevHabit",
            table: "Tags");

        migrationBuilder.DropColumn(
            name: "UserId",
            schema: "DevHabit",
            table: "Habits");

        migrationBuilder.CreateIndex(
            name: "IX_Tags_Name",
            schema: "DevHabit",
            table: "Tags",
            column: "Name",
            unique: true);
    }
}
