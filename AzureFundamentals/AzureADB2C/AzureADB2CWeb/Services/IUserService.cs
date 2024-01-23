using AzureADB2CWeb.Models;

namespace AzureADB2CWeb.Services
{
    public interface IUserService
    {
        User Create(User user);

        User GetUserById(string b2cObjectId);

        Task<string> GetB2CTokenAsync();

        User GetUserFromSession();
    }
}
