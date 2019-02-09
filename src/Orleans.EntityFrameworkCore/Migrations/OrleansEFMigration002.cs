using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Orleans.EntityFrameworkCore.Migrations
{
    [DbContext(typeof(OrleansEFContext))]
    [Migration(nameof(OrleansEFMigration002))]
    public partial class OrleansEFMigration002 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "orleans_ef_reminder",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    service_id = table.Column<string>(nullable: false),
                    grain_id = table.Column<string>(nullable: false),
                    reminder_name = table.Column<string>(nullable: false),
                    start_time = table.Column<DateTime>(nullable: false),
                    period = table.Column<double>(nullable: false),
                    grain_hash = table.Column<int>(nullable: false),
                    etag = table.Column<string>(nullable: false),
                    created_at = table.Column<DateTime>(nullable: false),
                    updated_at = table.Column<DateTime>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orleans_ef_reminder", a => new
                    {
                        a.id,
                    });
                }
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}