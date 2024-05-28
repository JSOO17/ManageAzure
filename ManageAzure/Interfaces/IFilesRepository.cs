using ManageAzure.Models;

namespace ManageAzure.Interfaces
{
    public interface IFilesRepository
    {
        Task<FileModel> GetFileByUrl(string url);
    }
}
