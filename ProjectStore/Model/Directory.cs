using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ProjectStore.FileService.Model
{
    public class DirectoryEntity
    {
        public int UserId { get; set; }
        [Key]
        public int DirId { get; set; }
        public required string DirName { get; set; }
        public required string Path { get; set; }
    }
}
