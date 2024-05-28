using ManageAzure.Models;

namespace ManageAzure.Interfaces
{
    public interface IAzureDevOpsRepository
    {
        Task UploadFiles(FileModel file, int workItemId);
        Task CreateIssue(Ticket ticket);
    }
}
