using AzureRedisCacheWeb.Data;
using AzureRedisCacheWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System.Diagnostics;

namespace AzureRedisCacheWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly IDistributedCache _cache;

        public HomeController(
            ILogger<HomeController> logger, 
            ApplicationDbContext dbContext, 
            IDistributedCache cache)
        {
            _logger = logger;
            _dbContext = dbContext;
            _cache = cache;
        }

        public IActionResult Index()
        {
            //_cache.Remove("categoryList");
            //_cache.Refresh("categoryList");

            List<Category> categoryList = new();
            var cachedCategories = _cache.GetString("categoryList");

            if (!string.IsNullOrEmpty(cachedCategories))
            {
                categoryList = JsonConvert.DeserializeObject<List<Category>>(cachedCategories);
            }
            else
            {
                categoryList = _dbContext.Categories.ToList();

                //Caching
                DistributedCacheEntryOptions cacheOptions = new()
                {
                    AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(30),
                };

                _cache.SetString("categoryList", JsonConvert.SerializeObject(categoryList), cacheOptions);
            }

            return View(categoryList);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
