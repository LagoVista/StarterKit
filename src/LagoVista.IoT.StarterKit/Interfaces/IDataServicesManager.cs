// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: be263a4fa1c6b0ad0988b7c3ec5c44a43b835631b94685c39969d7c5ad526c25
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.IoT.StarterKit.Interfaces
{
    public interface IDataServicesManager
    {
        Task<List<EntityHeader>> GetAllObjectsOfType(string typeName, EntityHeader org, EntityHeader user);
    }
}
