using Microsoft.EntityFrameworkCore;
using AnalysisService.Models;

namespace AnalysisService.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Analysis> Analyses { get; set; }
    }
}
