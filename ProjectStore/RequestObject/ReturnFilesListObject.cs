using ProjectStore.FileService.Model;

namespace ProjectStore.FileService.RequestObject
{
    public record ReturnFilesListObject(List<FileEntity> files, List<DirectoryEntity> directories);
}
