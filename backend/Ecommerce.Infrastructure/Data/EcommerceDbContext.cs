using System;
using System.Collections.Generic;
using Ecommerce.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Data;

public partial class EcommerceDbContext : DbContext
{
    public EcommerceDbContext(DbContextOptions<EcommerceDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<category> categories { get; set; }

    public virtual DbSet<order> orders { get; set; }

    public virtual DbSet<order_item> order_items { get; set; }

    public virtual DbSet<product> products { get; set; }

    public virtual DbSet<processed_stripe_event> processed_stripe_events { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<category>(entity =>
        {
            entity.HasKey(e => e.id).HasName("categories_pkey");

            entity.HasIndex(e => e.delete_flag, "ix_categories_delete_flag");

            entity.Property(e => e.delete_flag).HasDefaultValue(false);
            entity.Property(e => e.name).HasMaxLength(150);
        });

        modelBuilder.Entity<order>(entity =>
        {
            entity.HasKey(e => e.id).HasName("orders_pkey");

            entity.HasIndex(e => e.created_at, "ix_orders_created_at");

            entity.HasIndex(e => e.status, "ix_orders_status");

            entity.HasIndex(e => e.public_id, "uq_orders_public_id").IsUnique();

            entity.Property(e => e.created_at).HasDefaultValueSql("now()");
            entity.Property(e => e.currency)
                .HasMaxLength(3)
                .IsFixedLength();
            entity.Property(e => e.customer_email).HasMaxLength(255);
            entity.Property(e => e.discount_amount).HasPrecision(12, 2);
            entity.Property(e => e.subtotal_amount).HasPrecision(12, 2);
            entity.Property(e => e.tax_amount).HasPrecision(12, 2);
            entity.Property(e => e.total_amount).HasPrecision(12, 2);
            entity.Property(e => e.updated_at).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<order_item>(entity =>
        {
            entity.HasKey(e => e.id).HasName("order_items_pkey");

            entity.HasIndex(e => e.order_id, "ix_order_items_order_id");

            entity.HasIndex(e => e.product_id, "ix_order_items_product_id");

            entity.Property(e => e.line_total).HasPrecision(12, 2);
            entity.Property(e => e.product_name).HasMaxLength(200);
            entity.Property(e => e.unit_price).HasPrecision(12, 2);

            entity.HasOne(d => d.order).WithMany(p => p.order_items)
                .HasForeignKey(d => d.order_id)
                .HasConstraintName("fk_order_items_orders");

            entity.HasOne(d => d.product).WithMany(p => p.order_items)
                .HasForeignKey(d => d.product_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_order_items_products");
        });

        modelBuilder.Entity<product>(entity =>
        {
            entity.HasKey(e => e.id).HasName("products_pkey");

            entity.HasIndex(e => e.category_id, "ix_products_category_id");

            entity.HasIndex(e => e.delete_flag, "ix_products_delete_flag");

            entity.Property(e => e.delete_flag).HasDefaultValue(false);
            entity.Property(e => e.name).HasMaxLength(200);
            entity.Property(e => e.price).HasPrecision(10, 2);

            entity.HasOne(d => d.category).WithMany(p => p.products)
                .HasForeignKey(d => d.category_id)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_products_categories");
        });

        modelBuilder.Entity<processed_stripe_event>(entity =>
        {
            entity.HasKey(e => e.id).HasName("processed_stripe_events_pkey");

            entity.HasIndex(e => e.stripe_event_id, "uq_processed_stripe_events_stripe_event_id")
                .IsUnique();

            entity.Property(e => e.created_at).HasDefaultValueSql("now()");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
