using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingOverlapConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // btree_gist дозволяє поєднати перевірку рівності conference_room_id
            // з перевіркою перетину часових діапазонів у GiST constraint.
            migrationBuilder.Sql(
            """
            CREATE EXTENSION IF NOT EXISTS btree_gist;

            ALTER TABLE bookings
            ADD CONSTRAINT ck_bookings_valid_time_range
            CHECK (end_time > start_time);

            ALTER TABLE bookings
            ADD CONSTRAINT ex_bookings_no_overlap
            EXCLUDE USING gist
            (
                conference_room_id WITH =,
                tstzrange(start_time, end_time, '[)') WITH &&
            );
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
            DROP CONSTRAINT IF EXISTS ck_bookings_valid_time_range;
            """);
        }
    }
}
