using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SYM_CONNECT.Migrations
{
    /// <inheritdoc />
    public partial class addedtotalPoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalEarnedPoints",
                table: "GroupMembers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalEarnedPoints",
                table: "GroupMembers");
        }
    }
}
