using EasyStock.Web.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyStock.Web.Data.Configurations;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.Property(s => s.Name).HasMaxLength(150);
        builder.HasIndex(s => s.Name).IsUnique();
        builder.Property(s => s.ContactEmail).HasMaxLength(254);
        builder.Property(s => s.Phone).HasMaxLength(30);
        builder.Property(s => s.Address).HasMaxLength(300);
    }
}