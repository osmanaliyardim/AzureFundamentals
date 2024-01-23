using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using AzureBlobProject.Models;

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

        public async Task<bool> UploadBlob(string blobName, IFormFile file, string containerName, Blob blob)
        {
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            var blobClient = blobContainerClient.GetBlobClient(blobName);

            var httpHeaders = new BlobHttpHeaders()
            {
                ContentType = file.ContentType
            };

            var metadata = new Dictionary<string, string>();
            metadata.Add("title", blob.Title);
            metadata.Add("comment", blob.Comment);

            var result = await blobClient.UploadAsync(file.OpenReadStream(), httpHeaders, metadata);

            // To remove metadata (2 ways)
            //var removeMetadata = new Dictionary<string, string>();
            //await blobClient.SetMetadataAsync(removeMetadata);
            //metadata.Remove("title");
            //await blobClient.SetMetadataAsync(metadata);

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

        public async Task<List<Blob>> GetAllBlobsWithUri(string containerName)
        {
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            var blobs = blobContainerClient.GetBlobsAsync();

            //Container level SAS Token generation
            string sasContainerSignature = "";
            if (blobContainerClient.CanGenerateSasUri)
            {
                var sasBuilder = new BlobSasBuilder()
                {
                    BlobContainerName = blobContainerClient.Name,
                    Resource = "c",
                    ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(1)
                };

                sasBuilder.SetPermissions(BlobSasPermissions.Read); //BlobSasPermissions.Write

                sasContainerSignature = blobContainerClient.GenerateSasUri(sasBuilder).AbsoluteUri.Split('?')[1].ToString();
            }

            var blobList = new List<Blob>();
            await foreach (var blob in blobs)
            {
                var blobClient = blobContainerClient.GetBlobClient(blob.Name);

                var blobIndividual = new Blob()
                {
                    Uri = blobClient.Uri.AbsoluteUri + "?" + sasContainerSignature
                };

                // Blob level SAS Token generation
                //if (blobClient.CanGenerateSasUri)
                //{
                //    var sasBuilder = new BlobSasBuilder()
                //    {
                //        BlobContainerName = blobClient.GetParentBlobContainerClient().Name,
                //        BlobName = blobClient.Name,
                //        Resource = "b",
                //        ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
                //    };

                //    sasBuilder.SetPermissions(BlobSasPermissions.Read); // BlobSasPermissions.Write

                //    blobIndividual.Uri = blobClient.GenerateSasUri(sasBuilder).AbsoluteUri;
                //}

                BlobProperties blobProps = await blobClient.GetPropertiesAsync();
                if (blobProps.Metadata.ContainsKey("title"))
                    blobIndividual.Title = blobProps.Metadata["title"];
                if (blobProps.Metadata.ContainsKey("comment"))
                    blobIndividual.Comment = blobProps.Metadata["comment"];

                blobList.Add(blobIndividual);
            }

            return blobList;
        }
    }
}
