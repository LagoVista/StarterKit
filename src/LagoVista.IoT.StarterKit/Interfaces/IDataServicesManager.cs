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
