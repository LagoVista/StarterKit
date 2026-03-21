using LagoVista.Core.Interfaces;
using LagoVista.IoT.StarterKit;
using Microsoft.Extensions.Configuration;

namespace LagoVista.IoT.StarterKits.CloudRepos
{
    public class StarterKitConnection : IStarterKitConnection
    {
        public IConnectionSettings StarterKitStorage { get; }

        public StarterKitConnection(IConfiguration configuration)
        {
            StarterKitStorage = configuration.CreateDefaultDBStorageSettings();
        }   
    }
}
