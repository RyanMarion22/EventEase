using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventEase.Migrations
{
    /// <inheritdoc />
    public partial class AddEventTypeToEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EventType",
                table: "Event",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventType",
                table: "Event");
        }
    }
}
