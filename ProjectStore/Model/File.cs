using System.ComponentModel.DataAnnotations;

namespace ProjectStore.FileService.Model
{
    public class FileEntity
    {
        [Key]
        [Required]
        public int UserId;
        [Key]
        [Required]
        public int FileID;
        public required string FileName;
        public required string Path;
    }
}
