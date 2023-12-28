using Microsoft.EntityFrameworkCore;

namespace ProjectStore.FileService.Model.DBContext
{
    public class DirectoryContext : DbContext
    {
        public DbSet<Directory> Directories { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=filedb,1433;Database=Filedb;User Id=sa;Password=SqAdmin123!!;");
        }
    }
}
