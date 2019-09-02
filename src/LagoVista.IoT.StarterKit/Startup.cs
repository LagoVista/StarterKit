using LagoVista.Core.Interfaces;
using LagoVista.IoT.StarterKit.Managers;
using LagoVista.UserAdmin;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.IoT.StarterKit
{
    public static class Startup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IOrgInitializer, OrgInitializer>();
        }
    }
}
