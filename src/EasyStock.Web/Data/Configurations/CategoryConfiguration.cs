using EasyStock.Web.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyStock.Web.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.Property(c => c.Name).HasMaxLength(100);
        builder.HasIndex(c => c.Name).IsUnique();
        builder.Property(c => c.Description).HasMaxLength(500);
    }
}