using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SYM_CONNECT.Migrations
{
    /// <inheritdoc />
    public partial class fixe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventSYMGroup_Events_EventsEventId",
                table: "EventSYMGroup");

            migrationBuilder.DropForeignKey(
                name: "FK_EventSYMGroup_SYMGroup_AssignedGroupsGroupId",
                table: "EventSYMGroup");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EventSYMGroup",
                table: "EventSYMGroup");

            migrationBuilder.RenameTable(
                name: "EventSYMGroup",
                newName: "EventGroups");

            migrationBuilder.RenameIndex(
                name: "IX_EventSYMGroup_EventsEventId",
                table: "EventGroups",
                newName: "IX_EventGroups_EventsEventId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventGroups",
                table: "EventGroups",
                columns: new[] { "AssignedGroupsGroupId", "EventsEventId" });

            migrationBuilder.AddForeignKey(
                name: "FK_EventGroups_Events_EventsEventId",
                table: "EventGroups",
                column: "EventsEventId",
                principalTable: "Events",
                principalColumn: "EventId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventGroups_SYMGroup_AssignedGroupsGroupId",
                table: "EventGroups",
                column: "AssignedGroupsGroupId",
                principalTable: "SYMGroup",
                principalColumn: "GroupId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventGroups_Events_EventsEventId",
                table: "EventGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_EventGroups_SYMGroup_AssignedGroupsGroupId",
                table: "EventGroups");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EventGroups",
                table: "EventGroups");

            migrationBuilder.RenameTable(
                name: "EventGroups",
                newName: "EventSYMGroup");

            migrationBuilder.RenameIndex(
                name: "IX_EventGroups_EventsEventId",
                table: "EventSYMGroup",
                newName: "IX_EventSYMGroup_EventsEventId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventSYMGroup",
                table: "EventSYMGroup",
                columns: new[] { "AssignedGroupsGroupId", "EventsEventId" });

            migrationBuilder.AddForeignKey(
                name: "FK_EventSYMGroup_Events_EventsEventId",
                table: "EventSYMGroup",
                column: "EventsEventId",
                principalTable: "Events",
                principalColumn: "EventId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventSYMGroup_SYMGroup_AssignedGroupsGroupId",
                table: "EventSYMGroup",
                column: "AssignedGroupsGroupId",
                principalTable: "SYMGroup",
                principalColumn: "GroupId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
