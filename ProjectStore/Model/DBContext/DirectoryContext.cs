using Microsoft.EntityFrameworkCore;

namespace ProjectStore.FileService.Model.DBContext
{
    public class DirectoryContext : DbContext
    {
        public DbSet<DirectoryEntity> Directories { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=localhost,1433;Database=Filedb;User Id=sa;Password=SqAdmin123!!;");
        }
    }
}
