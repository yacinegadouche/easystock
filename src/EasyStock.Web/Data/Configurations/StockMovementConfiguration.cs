using EasyStock.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyStock.Web.Data.Configurations;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.Property(m => m.Type)
            .HasConversion<string>()
            .HasMaxLength(30);
        builder.Property(m => m.Reason).HasMaxLength(500);
        builder.Property(m => m.ExternalReference).HasMaxLength(100);

        builder.HasOne(m => m.Item)
            .WithMany(i => i.Movements)
            .HasForeignKey(m => m.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.User)
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Assignment)
            .WithMany()
            .HasForeignKey(m => m.AssignmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => new { m.ItemId, m.OccurredAt });
        builder.HasIndex(m => m.OccurredAt);

        builder.ToTable(t =>
            t.HasCheckConstraint("ck_stock_movements_quantity_positive", "quantity > 0"));
    }
}