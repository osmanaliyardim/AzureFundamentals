using AzureADB2CWeb.Data;
using AzureADB2CWeb.Extensions;
using AzureADB2CWeb.Models;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace AzureADB2CWeb.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public User Create(User user)
        {
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            return user;
        }

        public async Task<string> GetB2CTokenAsync()
        {
            try
            {
                return await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");
            }
            catch (Exception)
            {
                return null;
            }
        }

        public User GetUserById(string b2cObjectId)
        {
            var user = new User();

            try
            {
                return _dbContext.Users.FirstOrDefault(u => u.B2CObjectId == b2cObjectId);
            }
            catch (Exception)
            {
                return user;
            }
        }

        public User GetUserFromSession()
        {
            var user = _httpContextAccessor.HttpContext.Session.GetComplexData<User>("UserSession");

            if (user == null || string.IsNullOrWhiteSpace(user.B2CObjectId))
            {
                var idClaim = ((ClaimsIdentity)_httpContextAccessor.HttpContext.User.Identity).FindFirst(ClaimTypes.NameIdentifier);

                string userId = idClaim?.Value;
                user = GetUserById(userId);

                _httpContextAccessor.HttpContext.Session.SetComplexData("UserSession", user);
            }

            return user;
        }
    }
}
