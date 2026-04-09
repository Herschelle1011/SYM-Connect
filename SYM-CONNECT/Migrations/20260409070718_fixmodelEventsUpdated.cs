using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SYM_CONNECT.Migrations
{
    /// <inheritdoc />
    public partial class fixmodelEventsUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Users_EventHandlerById",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_EventHandlerById",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "EventHandlerById",
                table: "Events");

            migrationBuilder.RenameColumn(
                name: "EventHandlerByUser",
                table: "Events",
                newName: "EventHandlerId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_EventHandlerId",
                table: "Events",
                column: "EventHandlerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Users_EventHandlerId",
                table: "Events",
                column: "EventHandlerId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Users_EventHandlerId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_EventHandlerId",
                table: "Events");

            migrationBuilder.RenameColumn(
                name: "EventHandlerId",
                table: "Events",
                newName: "EventHandlerByUser");

            migrationBuilder.AddColumn<int>(
                name: "EventHandlerById",
                table: "Events",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_EventHandlerById",
                table: "Events",
                column: "EventHandlerById");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Users_EventHandlerById",
                table: "Events",
                column: "EventHandlerById",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
