using LagoVista.Core.Models;
using LagoVista.IoT.Deployment.Admin.Models;
using LagoVista.IoT.DeviceAdmin.Models;
using LagoVista.IoT.DeviceManagement.Core.Models;
using LagoVista.IoT.DeviceMessaging.Admin.Models;
using LagoVista.IoT.Pipeline.Admin.Models;
using LagoVista.ProjectManagement.Models;
using System.Threading.Tasks;

namespace LagoVista.IoT.StarterKit
{
    public interface ICloneServices
    {
        Task<DeploymentInstance> CloneDeploymentInstanceAsync();
        Task<DeviceConfiguration> CloneDeviceConfigurationAsync();
        Task<DeviceRepository> CloneDeviceRepository();
        Task<DeviceType> CloneDeviceTypeAsync(string originalListenerId, EntityHeader org, EntityHeader user);
        Task<DeviceWorkflow> CloneDeviceWorkflowAsync();
        Task<InputTranslatorConfiguration> CloneInputTranslatorAsync();
        void CloneListenerAsync();
        Task<DeviceMessageDefinition> CloneMessageAsync(string originalMessageId, EntityHeader org, EntityHeader user);
        Task<DeviceWorkflow> CloneOutputTranslatorAysnc();
        Task<PlannerConfiguration> ClonePlannerAsync();
        Task<Project> CloneProjectAsync(CloneProjectRequest cloneRequest, EntityHeader org, EntityHeader user);
        Task<SentinelConfiguration> CloneSentinalAsync();
        Task<Solution> CloneSolutionAsync();
    }
}