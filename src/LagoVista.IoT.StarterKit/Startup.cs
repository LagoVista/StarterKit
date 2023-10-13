using LagoVista.Core.Interfaces;
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
        }
    }
}
