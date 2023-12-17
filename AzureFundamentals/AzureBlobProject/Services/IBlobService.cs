namespace AzureBlobProject.Services;

public interface IBlobService
{
    Task<string> GetBlob(string blobName, string containerName);

    Task<List<string>> GetAllBlobs(string containerName);

    Task<bool> UploadBlob(string blobName, IFormFile file, string containerName);

    Task<bool> DeleteBlob(string blobName, string containerName);
}
