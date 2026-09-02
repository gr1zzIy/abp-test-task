using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "bookings",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            
            // Скасовані бронювання не повинні блокувати часовий проміжок,
            // тому constraint застосовується лише до активних бронювань.
            migrationBuilder.Sql(
                """
                ALTER TABLE bookings
                DROP CONSTRAINT IF EXISTS ex_bookings_no_overlap;

                ALTER TABLE bookings
                ADD CONSTRAINT ex_bookings_no_overlap
                EXCLUDE USING gist
                (
                    conference_room_id WITH =,
                    tstzrange(start_time, end_time, '[)') WITH &&
                )
                WHERE (status = 0);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE bookings
                DROP CONSTRAINT IF EXISTS ex_bookings_no_overlap;

                ALTER TABLE bookings
                ADD CONSTRAINT ex_bookings_no_overlap
                EXCLUDE USING gist
                (
                    conference_room_id WITH =,
                    tstzrange(start_time, end_time, '[)') WITH &&
                );
                """);
            
            migrationBuilder.DropColumn(
                name: "status",
                table: "bookings");
        }
    }
}
