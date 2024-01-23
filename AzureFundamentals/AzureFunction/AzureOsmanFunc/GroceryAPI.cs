using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using AzureOsmanFunc.Data;
using AzureOsmanFunc.Models;
using System.Linq;

namespace AzureOsmanFunc
{
    public class GroceryAPI
    {
        private readonly AzureOsmanFuncDbContext _dbContext;

        public GroceryAPI(AzureOsmanFuncDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [FunctionName("CreateGrocery")]
        public async Task<IActionResult> CreateGrocery(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "GroceryList")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Creating a grocery list item.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            GroceryItem_Upsert data = JsonConvert.DeserializeObject<GroceryItem_Upsert>(requestBody);

            var groceryItem = new GroceryItem
            {
                Name = data.Name
            };

            _dbContext.Add(groceryItem);
            _dbContext.SaveChanges();

            return new OkObjectResult(groceryItem);
        }

        [FunctionName("GetGrocery")]
        public async Task<IActionResult> GetGrocery(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "GroceryList")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Getting all grocery list items.");

            return new OkObjectResult(_dbContext.GroceryItems.ToList());
        }

        [FunctionName("GetGroceryByid")]
        public async Task<IActionResult> GetGroceryByid(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "GroceryList/{id}")] HttpRequest req,
            ILogger log,
            string id)
        {
            log.LogInformation("Getting a grocery list item by id.");

            var item = _dbContext.GroceryItems.FirstOrDefault(u => u.Id == id);

            if (item == null)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(item);
        }

        [FunctionName("RemoveGrocery")]
        public async Task RemoveGrocery(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "GroceryList/Remove/{id}")] HttpRequest req,
            ILogger log,
            string id)
        {
            log.LogInformation("Deleting a grocery list item by id.");

            var itemToDelete = _dbContext.GroceryItems.FirstOrDefault(gi => gi.Id == id);

            if (itemToDelete != null)
            {
                _dbContext.GroceryItems.Remove(itemToDelete);
                _dbContext.SaveChanges();
            }
        }

        [FunctionName("UpdateGrocery")]
        public async Task UpdateGrocery(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "GroceryList/Update/{id}")] HttpRequest req,
            ILogger log,
            string id)
        {
            log.LogInformation("Updating a grocery list item by id.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            GroceryItem_Upsert data = JsonConvert.DeserializeObject<GroceryItem_Upsert>(requestBody);

            var itemToUpdate = _dbContext.GroceryItems.FirstOrDefault(gi => gi.Id == id);

            if (itemToUpdate != null)
            {
                itemToUpdate.Name = data.Name;

                _dbContext.Update(itemToUpdate);
                _dbContext.SaveChanges();
            }
        }
    }
}
