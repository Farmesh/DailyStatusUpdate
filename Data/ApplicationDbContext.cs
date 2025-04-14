using Microsoft.EntityFrameworkCore;
using DailyStatusApp.Models;

namespace DailyStatusApp.Data
{
    public class ApplicationDbContxt
    {
        public class ApplicationDbContext : DbContext
        {
            public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
            {
            }

            public DbSet<DailyStatus> DailyStatus { get; set; }
        }
    }
}
