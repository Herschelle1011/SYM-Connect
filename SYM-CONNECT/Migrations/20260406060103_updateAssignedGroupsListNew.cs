using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SYM_CONNECT.Migrations
{
    /// <inheritdoc />
    public partial class updateAssignedGroupsListNew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_SYMGroup_AssignedGroupGroupId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_AssignedGroupGroupId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "AssignedGroupGroupId",
                table: "Events");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssignedGroupGroupId",
                table: "Events",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_AssignedGroupGroupId",
                table: "Events",
                column: "AssignedGroupGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_SYMGroup_AssignedGroupGroupId",
                table: "Events",
                column: "AssignedGroupGroupId",
                principalTable: "SYMGroup",
                principalColumn: "GroupId");
        }
    }
}
