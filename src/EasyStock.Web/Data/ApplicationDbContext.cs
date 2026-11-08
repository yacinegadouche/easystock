using Microsoft.EntityFrameworkCore;

namespace EasyStock.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    // DbSet<T> properties will be added in step 3 (one per table).
}