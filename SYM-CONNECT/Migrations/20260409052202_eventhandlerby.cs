using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SYM_CONNECT.Migrations
{
    /// <inheritdoc />
    public partial class eventhandlerby : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EventHandlerById",
                table: "Events",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EventHandlerByUser",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "EventHandlerByUser",
                table: "Events");
        }
    }
}
