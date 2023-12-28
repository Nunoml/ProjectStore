using System.ComponentModel.DataAnnotations;

namespace ProjectStore.FileService.Model
{
    public record File
    {
        [Key]
        [Required]
        public int UserId;
        [Key]
        [Required]
        public int FileID;
        public required string FileName;
        public required string Path;
        // Maybe instead of separate directory database, store if the entry is a directory?
        public required bool IsDirectory;
        // For ease of access maybe?
        // Make sure this is a directory!
        // If null assume its at root directory
        // Consider removing Path? Or storing it in a different way, reading db multiple time to find the path might be expensive
        // sendhelp
        public File? InsideOfDir;
    }
}
