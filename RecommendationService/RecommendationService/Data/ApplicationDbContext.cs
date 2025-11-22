using Microsoft.EntityFrameworkCore;
using RecommendationService.Models;

namespace RecommendationService.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Recommendation> Recommendations { get; set; }
    }
}
