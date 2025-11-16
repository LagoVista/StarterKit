// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 1834962d4fd30631e34075bc95683d5fa0a08627ebb090dd8105a8bfcf96d189
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Interfaces;
using LagoVista.IoT.StarterKit.Interfaces;
using LagoVista.IoT.StarterKits.CloudRepos.Repos;
using LagoVista.IoT.Verifiers.Managers;
using LagoVista.IoT.Verifiers.Repos;

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
