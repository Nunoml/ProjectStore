using System.ComponentModel.DataAnnotations;

namespace ProjectStore.FileService.Model
{
    public class FileEntity
    {
        [Key]
        public int FileID { get; set; }
        [Required]
        public int UserId { get; set; }
        public required string FileName { get; set; }
        public required string Path { get; set; }
    }
}
