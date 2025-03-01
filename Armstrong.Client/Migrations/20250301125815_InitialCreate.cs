using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Armstrong.Client.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "channels",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    channel_id = table.Column<int>(type: "integer", nullable: false),
                    server_id = table.Column<int>(type: "integer", nullable: false),
                    name_controlpoint = table.Column<string>(type: "text", nullable: true),
                    on_off = table.Column<int>(type: "integer", nullable: false),
                    state_for_threeview = table.Column<int>(type: "integer", nullable: false),
                    consumption = table.Column<double>(type: "double precision", nullable: false),
                    special_control = table.Column<bool>(type: "boolean", nullable: false),
                    name_db = table.Column<string>(type: "text", nullable: true),
                    type = table.Column<int>(type: "integer", nullable: false),
                    min_nuclid_value = table.Column<double>(type: "double precision", nullable: false),
                    max_nuclid_value = table.Column<double>(type: "double precision", nullable: false),
                    background = table.Column<double>(type: "double precision", nullable: false),
                    name_location = table.Column<string>(type: "text", nullable: true),
                    event_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    event_value = table.Column<double>(type: "double precision", nullable: false),
                    unit = table.Column<string>(type: "text", nullable: true),
                    value_cu = table.Column<double>(type: "double precision", nullable: false),
                    value_impulses = table.Column<double>(type: "double precision", nullable: false),
                    coefficient = table.Column<double>(type: "double precision", nullable: false),
                    pre_accident = table.Column<double>(type: "double precision", nullable: false),
                    accident = table.Column<double>(type: "double precision", nullable: false),
                    count = table.Column<int>(type: "integer", nullable: false),
                    error_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_channels", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "histories",
                columns: table => new
                {
                    channel_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    event_value = table.Column<double>(type: "double precision", nullable: false),
                    event_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_histories", x => x.channel_id);
                    table.ForeignKey(
                        name: "FK_histories_channels_channel_id",
                        column: x => x.channel_id,
                        principalTable: "channels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_histories_channel_id",
                table: "histories",
                column: "channel_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "histories");

            migrationBuilder.DropTable(
                name: "channels");
        }
    }
}
