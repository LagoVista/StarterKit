using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using System;
using System.Threading.Tasks;

namespace LagoVista.IoT.StarterKit
{
    public interface IYamlServices
    {
        Task<InvokeResult<string>> GetYamlAsync(String recordType, string id, EntityHeader org, EntityHeader usr);

        Task<InvokeResult> ApplyXaml(String yaml, EntityHeader org, EntityHeader usr);
    }
}
