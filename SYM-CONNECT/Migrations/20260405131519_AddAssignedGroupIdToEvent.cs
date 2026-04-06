using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SYM_CONNECT.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignedGroupIdToEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_SYMGroup_AssignedGroupGroupId",
                table: "Events");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddForeignKey(
                name: "FK_Events_SYMGroup_AssignedGroupGroupId",
                table: "Events",
                column: "AssignedGroupGroupId",
                principalTable: "SYMGroup",
                principalColumn: "GroupId");
        }
    }
}
