// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 1834962d4fd30631e34075bc95683d5fa0a08627ebb090dd8105a8bfcf96d189
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.IoT.Logging.Loggers;
using LagoVista.IoT.StarterKit.Interfaces;
using LagoVista.IoT.StarterKit.Models;
using LagoVista.IoT.StarterKits.CloudRepos.Repos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LagoVista.IoT.StaterKit.CloudRepos
{
    public static class Startup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IProductLineRepo, ProductLineRepo>();
        }
    }
}


namespace LagoVista.DependencyInjection
{
    public static class BillingModule
    {
        public static void AddBillingModule(this IServiceCollection services, IConfigurationRoot configRoot, IAdminLogger logger)
        {
            LagoVista.IoT.StarterKit.Startup.ConfigureServices(services);
            LagoVista.IoT.StaterKit.CloudRepos.Startup.ConfigureServices(services);
            services.AddMetaDataHelper<ProductLine>();
        }
    }
}