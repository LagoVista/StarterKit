using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.IoT.StarterKit
{
    public interface IStarterKitConnection
    {
        IConnectionSettings StarterKitStorage { get; }
        bool ShouldConsolidateCollections { get; }
    }
}
