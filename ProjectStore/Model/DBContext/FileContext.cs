using Microsoft.EntityFrameworkCore;

namespace ProjectStore.FileService.Model.DBContext
{
    public class FileContext : DbContext
    {
        public DbSet<File> Files { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=filedb,1433;Database=Filedb;User Id=sa;Password=SqAdmin123!!;");
        }
    }
}
