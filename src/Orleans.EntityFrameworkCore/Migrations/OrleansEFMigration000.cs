using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Orleans.EntityFrameworkCore.Migrations
{
    [DbContext(typeof(OrleansEFContext))]
    [Migration(nameof(OrleansEFMigration000))]
    public partial class OrleansEFMigration000 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "orleans_ef_membership",
                columns: table => new
                {
                    deployment_id = table.Column<string>(nullable: false),
                    address = table.Column<string>(nullable: false),
                    port = table.Column<int>(nullable: false),
                    generation = table.Column<int>(nullable: false),
                    silo_name = table.Column<string>(nullable: false),
                    host_name = table.Column<string>(nullable: false),
                    status = table.Column<int>(nullable: false),
                    proxy_port = table.Column<int>(nullable: false),
                    suspect_times = table.Column<string>(nullable: true),
                    start_time = table.Column<DateTime>(),
                    i_am_alive_time = table.Column<DateTime>(),
                    fault_zone = table.Column<int>(nullable: false),
                    update_zone = table.Column<int>(nullable: false),
                    created_at = table.Column<DateTime>(nullable: false),
                    updated_at = table.Column<DateTime>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orleans_ef_membership", a => new
                    {
                        a.deployment_id,
                        a.address,
                        a.port,
                        a.generation
                    });
                }
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "orleans_ef_membership"
            );
        }
    }
}