// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 884e54be831cd65783a63fd103b6eaaf5e1d842fc9551f9fc470b7fe750fbb5b
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using System;
using System.IO;
using System.Threading.Tasks;

namespace LagoVista.IoT.StarterKit
{
    public interface IYamlServices
    {
        Task<InvokeResult<Tuple<string, string>>> SerilizeToYamlAsync(string recordType, string id, EntityHeader org, EntityHeader usr);

        Task<InvokeResult<object>> DeserializeFromYamlAsync(string recordType, Stream strm, EntityHeader org, EntityHeader usr);
    }
}