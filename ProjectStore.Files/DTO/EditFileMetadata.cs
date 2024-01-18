namespace ProjectStore.Files.DTO
{
    public record EditFileMetadata
    {
        public string OldFileName { get; set; }
        public string NewFileName { get; set; }
        public int newFolder { get; set; }
    }
}
