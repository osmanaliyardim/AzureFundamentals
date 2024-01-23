using AzureOsmanFunc.Data;
using AzureOsmanFunc.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AzureOsmanFunc
{
    public class OnQueueTriggerUpdateDatabase
    {
        private readonly AzureOsmanFuncDbContext _dbContext;

        public OnQueueTriggerUpdateDatabase(AzureOsmanFuncDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [FunctionName("OnQueueTriggerUpdateDatabase")]
        public void Run([QueueTrigger("SalesRequestInBound", Connection = "AzureWebJobsStorage")]SalesRequest myQueueItem, ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");

            myQueueItem.Status = "Submitted";

            _dbContext.SalesRequests.Add(myQueueItem);
            _dbContext.SaveChanges();
        }
    }
}
