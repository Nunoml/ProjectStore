namespace ProjectStore.Files.DataAccess.Model
{
    public class DirectoryModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string DirName { get; set; }
        public int Inside { get; set; }
    }
}
