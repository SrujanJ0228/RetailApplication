
using Microsoft.EntityFrameworkCore;
using RetailApplication.Models; // Import your models namespace

namespace RetailApplication.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Define DbSets for your models
        public DbSet<Product> Products { get; set; }
        public DbSet<ApprovalQueue> ApprovalQueue { get; set; }
    }
}
