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

    public virtual DbSet<product> products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<category>(entity =>
        {
            entity.HasKey(e => e.id).HasName("categories_pkey");

            entity.Property(e => e.delete_flag).HasDefaultValue(false);
        });

        modelBuilder.Entity<product>(entity =>
        {
            entity.HasKey(e => e.id).HasName("products_pkey");

            entity.Property(e => e.delete_flag).HasDefaultValue(false);

            entity.HasOne(d => d.category).WithMany(p => p.products)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_products_categories");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
