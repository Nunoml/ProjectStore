using Microsoft.EntityFrameworkCore;

namespace ProjectStore.Identity.Model.DBContext
{
    public class UserContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=localhost;Database=Userdb;Trusted_Connection=True;TrustServerCertificate=True");
        }
    }
}
