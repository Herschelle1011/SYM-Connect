using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SYM_CONNECT.Migrations
{
    /// <inheritdoc />
    public partial class updateEventModelAddGroupList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_SYMGroup_AssignedGroupId",
                table: "Events");

            migrationBuilder.RenameColumn(
                name: "AssignedGroupId",
                table: "Events",
                newName: "AssignedGroupGroupId");

            migrationBuilder.RenameIndex(
                name: "IX_Events_AssignedGroupId",
                table: "Events",
                newName: "IX_Events_AssignedGroupGroupId");

            migrationBuilder.AddColumn<int>(
                name: "EventId",
                table: "SYMGroup",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SYMGroup_EventId",
                table: "SYMGroup",
                column: "EventId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_SYMGroup_AssignedGroupGroupId",
                table: "Events",
                column: "AssignedGroupGroupId",
                principalTable: "SYMGroup",
                principalColumn: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_SYMGroup_Events_EventId",
                table: "SYMGroup",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "EventId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_SYMGroup_AssignedGroupGroupId",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_SYMGroup_Events_EventId",
                table: "SYMGroup");

            migrationBuilder.DropIndex(
                name: "IX_SYMGroup_EventId",
                table: "SYMGroup");

            migrationBuilder.DropColumn(
                name: "EventId",
                table: "SYMGroup");

            migrationBuilder.RenameColumn(
                name: "AssignedGroupGroupId",
                table: "Events",
                newName: "AssignedGroupId");

            migrationBuilder.RenameIndex(
                name: "IX_Events_AssignedGroupGroupId",
                table: "Events",
                newName: "IX_Events_AssignedGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_SYMGroup_AssignedGroupId",
                table: "Events",
                column: "AssignedGroupId",
                principalTable: "SYMGroup",
                principalColumn: "GroupId");
        }
    }
}
