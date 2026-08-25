using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "conference_rooms",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    capacity = table.Column<int>(type: "integer", nullable: false),
                    hourly_rate = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_conference_rooms", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "services",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_services", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "bookings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    start_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    end_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    total_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    conference_room_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bookings", x => x.id);
                    table.ForeignKey(
                        name: "fk_bookings_conference_rooms_conference_room_id",
                        column: x => x.conference_room_id,
                        principalTable: "conference_rooms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "conference_room_services",
                columns: table => new
                {
                    conference_rooms_id = table.Column<Guid>(type: "uuid", nullable: false),
                    services_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_conference_room_services", x => new { x.conference_rooms_id, x.services_id });
                    table.ForeignKey(
                        name: "fk_conference_room_services_conference_rooms_conference_rooms_",
                        column: x => x.conference_rooms_id,
                        principalTable: "conference_rooms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_conference_room_services_services_services_id",
                        column: x => x.services_id,
                        principalTable: "services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "booking_services",
                columns: table => new
                {
                    bookings_id = table.Column<Guid>(type: "uuid", nullable: false),
                    selected_services_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_booking_services", x => new { x.bookings_id, x.selected_services_id });
                    table.ForeignKey(
                        name: "fk_booking_services_bookings_bookings_id",
                        column: x => x.bookings_id,
                        principalTable: "bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_booking_services_services_selected_services_id",
                        column: x => x.selected_services_id,
                        principalTable: "services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_booking_services_selected_services_id",
                table: "booking_services",
                column: "selected_services_id");

            migrationBuilder.CreateIndex(
                name: "ix_bookings_conference_room_id_start_time_end_time",
                table: "bookings",
                columns: new[] { "conference_room_id", "start_time", "end_time" });

            migrationBuilder.CreateIndex(
                name: "ix_conference_room_services_services_id",
                table: "conference_room_services",
                column: "services_id");

            migrationBuilder.CreateIndex(
                name: "ix_conference_rooms_capacity",
                table: "conference_rooms",
                column: "capacity");

            migrationBuilder.CreateIndex(
                name: "ix_conference_rooms_name",
                table: "conference_rooms",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_services_name",
                table: "services",
                column: "name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "booking_services");

            migrationBuilder.DropTable(
                name: "conference_room_services");

            migrationBuilder.DropTable(
                name: "bookings");

            migrationBuilder.DropTable(
                name: "services");

            migrationBuilder.DropTable(
                name: "conference_rooms");
        }
    }
}
