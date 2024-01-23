namespace AzureBlobProject.Services;

public interface IContainerService
{
    Task<List<string>> GetAllContainerAndBlobs();

    Task<List<string>> GetAllContainers();

    Task CreateContainer(string containerName);

    Task DeleteContainer(string containerName);
}
