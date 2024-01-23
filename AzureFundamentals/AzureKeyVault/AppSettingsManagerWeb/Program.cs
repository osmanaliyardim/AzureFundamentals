using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace AzureKeyVaultWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, builder) =>
                {
                    builder.Sources.Clear();
                    
                    builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    builder.AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
                    builder.AddJsonFile("custom.json", optional: true, reloadOnChange: true);
                    if (hostingContext.HostingEnvironment.IsDevelopment())
                    {
                        builder.AddUserSecrets<Program>();
                    }
                    
                    builder.AddEnvironmentVariables();
                    builder.AddCommandLine(args);

                    if (hostingContext.HostingEnvironment.IsDevelopment())
                    {
                        return;
                    }

                    var builtConfig = builder.Build();
                    builder.AddAzureKeyVault($"https://{builtConfig["appsettingsosmanvault"]}.vault.azure.net/");
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
