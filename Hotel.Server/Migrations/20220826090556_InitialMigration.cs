using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecnl.Server.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    password = table.Column<string>(type: "text", nullable: false),
                    first_name = table.Column<string>(type: "text", nullable: false),
                    last_name = table.Column<string>(type: "text", nullable: false),
                    activated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    role = table.Column<string>(type: "text", nullable: true),
                    activation_code = table.Column<string>(type: "text", nullable: true),
                    password_reset_code = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "hotel",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    geo_lat = table.Column<double>(type: "double precision", nullable: false),
                    geo_lng = table.Column<double>(type: "double precision", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp"),
                    user_created_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp"),
                    user_updated_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hotel", x => x.id);
                    table.ForeignKey(
                        name: "fk_hotel_user_user_created_id",
                        column: x => x.user_created_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_hotel_user_user_updated_id",
                        column: x => x.user_updated_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "user",
                columns: new[] { "id", "activated_at", "activation_code", "email", "first_name", "last_name", "password", "password_reset_code", "role" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), DateTime.UtcNow, null, "admin@hoteltest.hr", "Hotel", "Admin", "AQAAAAEAACcQAAAAENLAaKQ9RTKMDD6R6isiYgPSzuk/urB/co49UGgZ4RfDTMYXZncKNBNEry2wwNM/wQ==", null, "admin" });

            migrationBuilder.CreateIndex(
                name: "ix_hotel_user_created_id",
                table: "hotel",
                column: "user_created_id");

            migrationBuilder.CreateIndex(
                name: "ix_hotel_user_updated_id",
                table: "hotel",
                column: "user_updated_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hotel");

            migrationBuilder.DropTable(
                name: "user");
        }
    }
}
