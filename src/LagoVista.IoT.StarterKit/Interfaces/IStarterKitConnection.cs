using LagoVista.Core.Interfaces;

namespace LagoVista.IoT.StarterKit
{
    public interface IStarterKitConnection
    {
        IConnectionSettings StarterKitStorage { get; }
    }
}
