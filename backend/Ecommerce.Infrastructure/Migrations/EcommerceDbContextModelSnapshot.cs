using System;
using Ecommerce.Infrastructure.Data;
using Ecommerce.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ecommerce.Infrastructure.Migrations;

[DbContext(typeof(EcommerceDbContext))]
partial class EcommerceDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "8.0.3")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

        modelBuilder.Entity("Ecommerce.Infrastructure.Data.Models.category", b =>
            {
                b.Property<int>("id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("integer")
                    .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                b.Property<bool>("delete_flag")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("boolean")
                    .HasDefaultValue(false);

                b.Property<string>("description")
                    .HasColumnType("text");

                b.Property<string>("name")
                    .IsRequired()
                    .HasMaxLength(150)
                    .HasColumnType("character varying(150)");

                b.HasKey("id")
                    .HasName("categories_pkey");

                b.HasIndex("delete_flag")
                    .HasDatabaseName("ix_categories_delete_flag");

                b.ToTable("categories");
            });

        modelBuilder.Entity("Ecommerce.Infrastructure.Data.Models.idempotency_key", b =>
            {
                b.Property<int>("id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("integer")
                    .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                b.Property<DateTime?>("completed_at")
                    .HasColumnType("timestamp with time zone");

                b.Property<DateTime>("created_at")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");

                b.Property<string>("idempotency_key_value")
                    .IsRequired()
                    .HasColumnType("text");

                b.Property<string>("request_hash")
                    .IsRequired()
                    .HasColumnType("text");

                b.Property<string?>("response_body")
                    .HasColumnType("jsonb");

                b.Property<int?>("response_code")
                    .HasColumnType("integer");

                b.Property<string>("scope")
                    .IsRequired()
                    .HasColumnType("text");

                b.Property<string>("status")
                    .IsRequired()
                    .HasColumnType("text");

                b.HasKey("id")
                    .HasName("idempotency_keys_pkey");

                b.HasIndex("idempotency_key_value", "scope")
                    .IsUnique()
                    .HasDatabaseName("uq_idempotency_keys_key_scope");

                b.ToTable("idempotency_keys");
            });

        modelBuilder.Entity("Ecommerce.Infrastructure.Data.Models.order", b =>
            {
                b.Property<int>("id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("integer")
                    .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                b.Property<DateTime>("created_at")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");

                b.Property<string>("checkout_session_id")
                    .HasMaxLength(255)
                    .HasColumnType("character varying(255)");

                b.Property<string>("currency")
                    .IsRequired()
                    .HasMaxLength(3)
                    .IsFixedLength()
                    .HasColumnType("character(3)");

                b.Property<string>("customer_email")
                    .HasMaxLength(255)
                    .HasColumnType("character varying(255)");

                b.Property<decimal>("discount_amount")
                    .HasPrecision(12, 2)
                    .HasColumnType("numeric(12,2)");

                b.Property<Guid>("public_id")
                    .HasColumnType("uuid");

                b.Property<short>("status")
                    .HasColumnType("smallint");

                b.Property<decimal>("subtotal_amount")
                    .HasPrecision(12, 2)
                    .HasColumnType("numeric(12,2)");

                b.Property<decimal>("tax_amount")
                    .HasPrecision(12, 2)
                    .HasColumnType("numeric(12,2)");

                b.Property<decimal>("total_amount")
                    .HasPrecision(12, 2)
                    .HasColumnType("numeric(12,2)");

                b.Property<DateTime>("updated_at")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");

                b.HasKey("id")
                    .HasName("orders_pkey");

                b.HasIndex("created_at")
                    .HasDatabaseName("ix_orders_created_at");

                b.HasIndex("status")
                    .HasDatabaseName("ix_orders_status");

                b.HasIndex("public_id")
                    .IsUnique()
                    .HasDatabaseName("uq_orders_public_id");

                b.ToTable("orders");
            });

        modelBuilder.Entity("Ecommerce.Infrastructure.Data.Models.order_item", b =>
            {
                b.Property<int>("id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("integer")
                    .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                b.Property<decimal>("line_total")
                    .HasPrecision(12, 2)
                    .HasColumnType("numeric(12,2)");

                b.Property<int>("order_id")
                    .HasColumnType("integer");

                b.Property<int?>("product_id")
                    .HasColumnType("integer");

                b.Property<string>("product_name")
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnType("character varying(200)");

                b.Property<int>("quantity")
                    .HasColumnType("integer");

                b.Property<decimal>("unit_price")
                    .HasPrecision(12, 2)
                    .HasColumnType("numeric(12,2)");

                b.HasKey("id")
                    .HasName("order_items_pkey");

                b.HasIndex("order_id")
                    .HasDatabaseName("ix_order_items_order_id");

                b.HasIndex("product_id")
                    .HasDatabaseName("ix_order_items_product_id");

                b.ToTable("order_items");
            });

        modelBuilder.Entity("Ecommerce.Infrastructure.Data.Models.processed_stripe_event", b =>
            {
                b.Property<int>("id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("integer")
                    .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                b.Property<DateTime>("created_at")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");

                b.Property<string>("event_type")
                    .IsRequired()
                    .HasColumnType("text");

                b.Property<Guid?>("order_public_id")
                    .HasColumnType("uuid");

                b.Property<string>("payment_intent_id")
                    .HasColumnType("text");

                b.Property<string>("stripe_event_id")
                    .IsRequired()
                    .HasColumnType("text");

                b.HasKey("id")
                    .HasName("processed_stripe_events_pkey");

                b.HasIndex("stripe_event_id")
                    .IsUnique()
                    .HasDatabaseName("uq_processed_stripe_events_stripe_event_id");

                b.ToTable("processed_stripe_events");
            });

        modelBuilder.Entity("Ecommerce.Infrastructure.Data.Models.product", b =>
            {
                b.Property<int>("id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("integer")
                    .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                b.Property<int>("category_id")
                    .HasColumnType("integer");

                b.Property<bool>("delete_flag")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("boolean")
                    .HasDefaultValue(false);

                b.Property<string>("description")
                    .HasColumnType("text");

                b.Property<string>("image_url")
                    .HasColumnType("text");

                b.Property<string>("name")
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnType("character varying(200)");

                b.Property<decimal>("price")
                    .HasPrecision(10, 2)
                    .HasColumnType("numeric(10,2)");

                b.HasKey("id")
                    .HasName("products_pkey");

                b.HasIndex("category_id")
                    .HasDatabaseName("ix_products_category_id");

                b.HasIndex("delete_flag")
                    .HasDatabaseName("ix_products_delete_flag");

                b.ToTable("products");
            });

        modelBuilder.Entity("Ecommerce.Infrastructure.Data.Models.order_item", b =>
            {
                b.HasOne("Ecommerce.Infrastructure.Data.Models.order", "order")
                    .WithMany("order_items")
                    .HasForeignKey("order_id")
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("fk_order_items_orders")
                    .IsRequired();

                b.HasOne("Ecommerce.Infrastructure.Data.Models.product", "product")
                    .WithMany("order_items")
                    .HasForeignKey("product_id")
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("fk_order_items_products");

                b.Navigation("order");

                b.Navigation("product");
            });

        modelBuilder.Entity("Ecommerce.Infrastructure.Data.Models.product", b =>
            {
                b.HasOne("Ecommerce.Infrastructure.Data.Models.category", "category")
                    .WithMany("products")
                    .HasForeignKey("category_id")
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("fk_products_categories")
                    .IsRequired();

                b.Navigation("category");
            });

        modelBuilder.Entity("Ecommerce.Infrastructure.Data.Models.category", b =>
            {
                b.Navigation("products");
            });

        modelBuilder.Entity("Ecommerce.Infrastructure.Data.Models.order", b =>
            {
                b.Navigation("order_items");
            });
#pragma warning restore 612, 618
    }
}
