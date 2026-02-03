using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Infrastructure.Migrations;

public partial class AddIdempotencyKeys : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "idempotency_keys",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                idempotency_key = table.Column<string>(type: "text", nullable: false),
                scope = table.Column<string>(type: "text", nullable: false),
                request_hash = table.Column<string>(type: "text", nullable: false),
                status = table.Column<string>(type: "text", nullable: false),
                response_code = table.Column<int>(type: "integer", nullable: true),
                response_body = table.Column<string>(type: "jsonb", nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("idempotency_keys_pkey", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "uq_idempotency_keys_key_scope",
            table: "idempotency_keys",
            columns: new[] { "idempotency_key", "scope" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "idempotency_keys");
    }
}
