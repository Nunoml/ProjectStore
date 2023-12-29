using Microsoft.EntityFrameworkCore;

namespace ProjectStore.FileService.Model.DBContext
{
    public class FileContext : DbContext
    {
        public DbSet<FileEntity> Files { get; set; }
        public DbSet<DirectoryEntity> Directories { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=localhost;Database=Filedb;Trusted_Connection=True;TrustServerCertificate=True");
        }
    }
}
