using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Orleans.EntityFrameworkCore.Migrations
{
    [DbContext(typeof(OrleansEFContext))]
    [Migration(nameof(OrleansEFMigration001))]
    public partial class OrleansEFMigration001 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "orleans_ef_storage",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    primary_key = table.Column<string>(nullable: false),
                    type = table.Column<string>(nullable: false),
                    data = table.Column<string>(nullable: false),
                    etag = table.Column<string>(nullable: false),
                    created_at = table.Column<DateTime>(nullable: false),
                    updated_at = table.Column<DateTime>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orleans_ef_storage", a => new
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