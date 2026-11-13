using EasyStock.Web.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyStock.Web.Data.Configurations;

public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.Property(i => i.Name).HasMaxLength(150);
        builder.HasIndex(i => i.Name).IsUnique();
        builder.Property(i => i.Description).HasMaxLength(1000);

        builder.HasOne(i => i.Category)
            .WithMany(c => c.Items)
            .HasForeignKey(i => i.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Supplier)
            .WithMany(s => s.Items)
            .HasForeignKey(i => i.SupplierId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("ck_items_quantity_in_stock_non_negative", "quantity_in_stock >= 0");
            t.HasCheckConstraint("ck_items_minimum_quantity_non_negative", "minimum_quantity >= 0");
        });
    }
}