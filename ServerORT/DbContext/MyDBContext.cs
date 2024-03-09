
using Microsoft.EntityFrameworkCore;
using ServerORT.Models;
namespace ServerORT.DbContext
{
    public class MyDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
        public DbSet<User> Users { get; set; }

    }
}
