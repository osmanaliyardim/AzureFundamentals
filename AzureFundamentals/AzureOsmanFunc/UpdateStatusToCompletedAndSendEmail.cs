using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureOsmanFunc.Data;
using AzureOsmanFunc.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SendGrid.Helpers.Mail;

namespace AzureOsmanFunc
{
    public class UpdateStatusToCompletedAndSendEmail
    {
        private readonly AzureOsmanFuncDbContext _dbContext;

        public UpdateStatusToCompletedAndSendEmail(AzureOsmanFuncDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [FunctionName("UpdateStatusToCompletedAndSendEmail")]
        public async Task Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer,
            [SendGrid(ApiKey = "CustomSendGridKeyAppSettingName")] IAsyncCollector<SendGridMessage> messageCollector,
            ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            IEnumerable<SalesRequest> salesRequestFromDb = _dbContext.SalesRequests.Where(u => u.Status == "Image Processed").ToList();
            foreach (var salesReq in salesRequestFromDb)
            {
                // For each request update status
                salesReq.Status = "Completed";
            }
            //salesRequestFromDb.ForEach(x => x.Status = "Completed");

            _dbContext.UpdateRange(salesRequestFromDb);
            _dbContext.SaveChanges();

            var message = new SendGridMessage();
            message.AddTo("osmanaliyardim@gmail.com");
            message.AddContent("text/html", $"Processing comleted for {salesRequestFromDb.Count()} records");
            message.SetFrom(new EmailAddress("sample@gmail.com"));
            message.SetSubject("AzureOsmanFunc Processing Successful");
            await messageCollector.AddAsync(message);
        }
    }
}
