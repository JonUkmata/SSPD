using Microsoft.EntityFrameworkCore;
using BusinessService.Models;

namespace BusinessService.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Business> Businesses { get; set; }
    }
}
