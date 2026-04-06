using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SYM_CONNECT.Migrations
{
    /// <inheritdoc />
    public partial class FixManyToManyEventGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SYMGroup_Events_EventId",
                table: "SYMGroup");

            migrationBuilder.DropIndex(
                name: "IX_SYMGroup_EventId",
                table: "SYMGroup");

            migrationBuilder.DropColumn(
                name: "EventId",
                table: "SYMGroup");

            migrationBuilder.CreateTable(
                name: "EventSYMGroup",
                columns: table => new
                {
                    AssignedGroupsGroupId = table.Column<int>(type: "int", nullable: false),
                    EventsEventId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventSYMGroup", x => new { x.AssignedGroupsGroupId, x.EventsEventId });
                    table.ForeignKey(
                        name: "FK_EventSYMGroup_Events_EventsEventId",
                        column: x => x.EventsEventId,
                        principalTable: "Events",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventSYMGroup_SYMGroup_AssignedGroupsGroupId",
                        column: x => x.AssignedGroupsGroupId,
                        principalTable: "SYMGroup",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventSYMGroup_EventsEventId",
                table: "EventSYMGroup",
                column: "EventsEventId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventSYMGroup");

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
                name: "FK_SYMGroup_Events_EventId",
                table: "SYMGroup",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "EventId");
        }
    }
}
