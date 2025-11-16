// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: aff252f5d8fede0530a3e1b6a94e6ba675126663b9daacd8e1f190f30acb08e0
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Interfaces;
using LagoVista.IoT.StarterKit.Interfaces;
using LagoVista.IoT.StarterKit.Managers;
using LagoVista.IoT.StarterKit.Services;
using LagoVista.UserAdmin;

namespace LagoVista.IoT.StarterKit
{
    public static class Startup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IOrgInitializer, OrgInitializer>();
            services.AddTransient<IYamlServices, YamlServices>();
            services.AddTransient<ICloneServices, CloneServices>();
            services.AddTransient<IAppWizard, AppWizard>();
            services.AddTransient<IDataServicesManager, DataServicesManager>();
            services.AddTransient<IProductLineManager, ProductLineManager>();
        }
    }
}
