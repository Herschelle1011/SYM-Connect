using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using SYM_CONNECT.Models;
namespace SYM_CONNECT.Data
{
    public class AppDbContext : DbContext
    {
        // Constructor
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {

        }

        // Tables in the database
        public DbSet<AppUsers> AppUsers { get; set; }
    }
}
