using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Infrastructure.Migrations;

public partial class AddProcessedStripeEvents : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "processed_stripe_events",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                stripe_event_id = table.Column<string>(type: "text", nullable: false),
                event_type = table.Column<string>(type: "text", nullable: false),
                order_public_id = table.Column<Guid>(type: "uuid", nullable: true),
                payment_intent_id = table.Column<string>(type: "text", nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
            },
            constraints: table =>
            {
                table.PrimaryKey("processed_stripe_events_pkey", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "uq_processed_stripe_events_stripe_event_id",
            table: "processed_stripe_events",
            column: "stripe_event_id",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "processed_stripe_events");
    }
}
