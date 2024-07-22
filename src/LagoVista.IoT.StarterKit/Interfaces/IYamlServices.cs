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