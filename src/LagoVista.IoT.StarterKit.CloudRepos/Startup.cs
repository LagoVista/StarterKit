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
