using Microsoft.EntityFrameworkCore;
using MotoModel.Entities;

namespace MotoData
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Moto> Moto { get; set; }  
    }
}
