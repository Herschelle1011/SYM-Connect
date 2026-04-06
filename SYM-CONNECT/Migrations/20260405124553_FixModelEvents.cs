using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SYM_CONNECT.Migrations
{
    /// <inheritdoc />
    public partial class FixModelEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssignedGroupGroupId",
                table: "Events",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "Events",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CancelledBy",
                table: "Events",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CancelledByUserId",
                table: "Events",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_AssignedGroupGroupId",
                table: "Events",
                column: "AssignedGroupGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_CancelledByUserId",
                table: "Events",
                column: "CancelledByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_SYMGroup_AssignedGroupGroupId",
                table: "Events",
                column: "AssignedGroupGroupId",
                principalTable: "SYMGroup",
                principalColumn: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Users_CancelledByUserId",
                table: "Events",
                column: "CancelledByUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_SYMGroup_AssignedGroupGroupId",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_Events_Users_CancelledByUserId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_AssignedGroupGroupId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_CancelledByUserId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "AssignedGroupGroupId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "CancelledBy",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "CancelledByUserId",
                table: "Events");
        }
    }
}
