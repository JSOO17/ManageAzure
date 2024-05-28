using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ManageAzure.Config;
using ManageAzure.Interfaces;
using ManageAzure.Models;
using Microsoft.Extensions.Options;

namespace ManageAzure.Repositories
{
    public class AzureBlobRepository : IFilesRepository
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobContainerClient _containerClient;
        private readonly ILogger<AzureBlobRepository> _logger;
        private readonly IOptions<AzureBlobOptions> _options;

        public AzureBlobRepository(IOptions<AzureBlobOptions> options, ILogger<AzureBlobRepository> logger)
        {
            _blobServiceClient = new BlobServiceClient(options.Value.ConnectionString);
            _containerClient = _blobServiceClient.GetBlobContainerClient(options.Value.ContainerName);
            _logger = logger;
            _options = options;
        }

        public async Task<FileModel> GetFileByUrl(string url)
        {
            var blobPath = url.Replace(_options.Value.UrlContainer, string.Empty);

            //var blobClient = new BlobClient(new Uri(url));
            var blobClient = _containerClient.GetBlobClient(blobPath);

            BlobDownloadInfo download = await blobClient.DownloadAsync();

            using var memoryStream = new MemoryStream();
            await download.Content.CopyToAsync(memoryStream);
            byte[] contentBytes = memoryStream.ToArray();

            return new FileModel
            {
                Content = Convert.ToBase64String(contentBytes),
                Name = blobClient.Name
            };
        }
    }
}
