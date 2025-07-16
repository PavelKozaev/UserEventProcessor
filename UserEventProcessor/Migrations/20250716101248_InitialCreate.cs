using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserEventProcessor.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_event_stats",
                columns: table => new
                {
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    event_type = table.Column<string>(type: "text", nullable: false),
                    count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_event_stats", x => new { x.user_id, x.event_type });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_event_stats");
        }
    }
}
