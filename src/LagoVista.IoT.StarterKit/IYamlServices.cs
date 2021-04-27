using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using System;
using System.IO;
using System.Threading.Tasks;

namespace LagoVista.IoT.StarterKit
{
    public interface IYamlServices
    {
        Task<InvokeResult<Tuple<string, string>>> GetYamlAsync(string recordType, string id, EntityHeader org, EntityHeader usr);

        Task<InvokeResult<Tuple<bool, string[]>>> ApplyXamlAsync(string recordType, Stream strm, EntityHeader org, EntityHeader usr);
    }
}