using System.IO;
using System.Linq;
using AzureOsmanFunc.Data;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AzureOsmanFunc
{
    public class BlobResizeTriggerUpdateStatusInDb
    {
        private readonly AzureOsmanFuncDbContext _dbContext;

        public BlobResizeTriggerUpdateStatusInDb(AzureOsmanFuncDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [FunctionName("BlobResizeTriggerUpdateStatusinDb")]
        public void Run([BlobTrigger("functionsalesrep-sm/{name}", 
            Connection = "AzureWebJobsStorage")]Stream myBlob, 
            string name, 
            ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            var fileName = Path.GetFileNameWithoutExtension(name);
            var salesRequestFromDb = _dbContext.SalesRequests.FirstOrDefault(u => u.Id == fileName);

            if (salesRequestFromDb != null)
            {
                salesRequestFromDb.Status = "Image Processed";
                _dbContext.SalesRequests.Update(salesRequestFromDb);
                _dbContext.SaveChanges();
            }
        }
    }
}
