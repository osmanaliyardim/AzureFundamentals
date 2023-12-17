using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace AzureBlobProject.Services;

public class ContainerService : IContainerService
{
    private readonly BlobServiceClient _blobServiceClient;

    public ContainerService(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    public async Task CreateContainer(string containerName)
    {
        var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);

        await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.BlobContainer);

        Task.CompletedTask.Wait();
    }

    public async Task DeleteContainer(string containerName)
    {
        var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);

        await blobContainerClient.DeleteIfExistsAsync();
    }

    public async Task<List<string>> GetAllContainerAndBlobs()
    {
        var containerAndBlobNames = new List<string>();

        containerAndBlobNames.Add("Storage Account Name: " + _blobServiceClient.AccountName);
        containerAndBlobNames.Add("---------------------------------------------------------------------------------------------");

        await foreach (BlobContainerItem blobContainerItem in _blobServiceClient.GetBlobContainersAsync())
        {
            containerAndBlobNames.Add("---Container Name: " + blobContainerItem.Name);

            var blobContainer = _blobServiceClient.GetBlobContainerClient(blobContainerItem.Name);

            await foreach (BlobItem blobItem in blobContainer.GetBlobsAsync())
            {
                containerAndBlobNames.Add("------Blob Name: " + blobItem.Name);
            }
            containerAndBlobNames.Add("---------------------------------------------------------------------------------------------");
        }

        return containerAndBlobNames;
    }

    public async Task<List<string>> GetAllContainers()
    {
        var containerNames = new List<string>();

        await foreach (BlobContainerItem blobContainerItem in _blobServiceClient.GetBlobContainersAsync())
        {
            containerNames.Add(blobContainerItem.Name);
        }

        return containerNames;
    }
}
