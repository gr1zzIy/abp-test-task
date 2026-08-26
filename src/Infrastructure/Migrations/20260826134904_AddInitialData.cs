using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "conference_rooms",
                columns: new[] { "id", "capacity", "hourly_rate", "name" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), 50, 2000m, "Зал A" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), 100, 3500m, "Зал B" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), 30, 1500m, "Зал C" }
                });

            migrationBuilder.InsertData(
                table: "services",
                columns: new[] { "id", "name", "price" },
                values: new object[,]
                {
                    { 1, "Проєктор", 500m },
                    { 2, "Wi-Fi", 300m },
                    { 3, "Звук", 700m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "conference_rooms",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "conference_rooms",
                keyColumn: "id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "conference_rooms",
                keyColumn: "id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "services",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "services",
                keyColumn: "id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "services",
                keyColumn: "id",
                keyValue: 3);
        }
    }
}
