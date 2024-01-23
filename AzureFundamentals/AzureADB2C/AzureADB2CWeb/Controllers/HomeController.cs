using AzureADB2CWeb.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using AzureADB2CWeb.Services;
using System.Security.Claims;
using AzureADB2CWeb.Helper;

namespace AzureADB2CWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IUserService _userService;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory, IUserService userService)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _userService = userService;
        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var b2cobjectId = ((ClaimsIdentity)HttpContext.User.Identity).FindFirst(ClaimTypes.NameIdentifier).Value;
                var user = _userService.GetUserById(b2cobjectId);

                if (user == null || string.IsNullOrWhiteSpace(user.B2CObjectId))
                {
                    var role = ((ClaimsIdentity)HttpContext.User.Identity).FindFirst("extension_UserRole").Value;
                    var email = ((ClaimsIdentity)HttpContext.User.Identity).FindFirst("emails").Value;

                    user = new() { B2CObjectId = b2cobjectId, Email = email, UserRole = role };

                    _userService.Create(user);
                }
            }

            return View();
        }

        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }

        [Permission("homeowner")]
        //[Authorize(Roles = "homeowner")]
        public IActionResult Homeowner()
        {
            return View();
        }

        [Permission("contractor")]
        //[Authorize(Roles = "contractor")]
        public IActionResult Contractor()
        {
            return View();
        }

        public IActionResult Signin()
        {
            var scheme = OpenIdConnectDefaults.AuthenticationScheme;

            var redirectUri = Url.ActionContext.HttpContext.Request.Scheme
                                + "://" + Url.ActionContext.HttpContext.Request.Host;

            return Challenge(new AuthenticationProperties { RedirectUri = redirectUri }, scheme);
        }

        public IActionResult SignOut()
        {
            var scheme = OpenIdConnectDefaults.AuthenticationScheme;

            return SignOut(new AuthenticationProperties(), CookieAuthenticationDefaults.AuthenticationScheme, scheme);
        }

        public IActionResult EditProfile()
        {
            return Challenge(new AuthenticationProperties { RedirectUri = "/" }, "B2C_1_Edit");
        }

        public async Task<IActionResult> CallApi()
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            var client = _httpClientFactory.CreateClient();

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                "https://localhost:7056/WeatherForecast");

            request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, accessToken);

            var response = await client.SendAsync(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                // error
            }

            return Content(await response.Content.ReadAsStringAsync());
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
