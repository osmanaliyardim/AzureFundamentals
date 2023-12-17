using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace AzureBlobProject.Services
{
    public class BlobService : IBlobService
    {
        private readonly BlobServiceClient _blobServiceClient;

        public BlobService(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }

        public async Task<bool> DeleteBlob(string blobName, string containerName)
        {
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            var blobClient = blobContainerClient.GetBlobClient(blobName);

            return await blobClient.DeleteIfExistsAsync();
        }

        public async Task<bool> UploadBlob(string blobName, IFormFile file, string containerName)
        {
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            var blobClient = blobContainerClient.GetBlobClient(blobName);

            var httpHeaders = new BlobHttpHeaders()
            {
                ContentType = file.ContentType
            };

            var result = await blobClient.UploadAsync(file.OpenReadStream(), httpHeaders);

            if (result != null)
            {
                return true;
            }

            return false;
        }

        public async Task<List<string>> GetAllBlobs(string containerName)
        {
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            var blobs = blobContainerClient.GetBlobsAsync();

            var blobString = new List<string>();
            await foreach(var blob in blobs)
            {
                blobString.Add(blob.Name);
            }

            return blobString;
        }

        public async Task<string> GetBlob(string blobName, string containerName)
        {
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            var blobClient = blobContainerClient.GetBlobClient(blobName);

            var blob = blobClient.Name;

            return blobClient.Uri.AbsoluteUri;
        }
    }
}
