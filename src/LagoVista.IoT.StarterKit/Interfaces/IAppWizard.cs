// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 1ab59630c11721561e7427958f3ff38dfd5c743065bd91d1af26874a01014ea1
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.IoT.StarterKit.Models;
using LagoVista.ProjectManagement.Models;
using System.Threading.Tasks;

namespace LagoVista.IoT.StarterKit
{
    public interface IAppWizard
    {
        Task<InvokeResult<Project>> CreateProjectAsync(AppWizardRequest request, EntityHeader org, EntityHeader user);
    }
}
