// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: cd7416edde0ce26f6bbc8374868b599c6c2fcd190f1a65fecf29c61b76314bc4
// IndexVersion: 2
// --- END CODE INDEX META ---
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
