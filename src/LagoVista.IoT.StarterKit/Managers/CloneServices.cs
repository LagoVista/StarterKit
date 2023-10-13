using LagoVista.Core;
using LagoVista.Core.Exceptions;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.IoT.Billing;
using LagoVista.IoT.Deployment.Admin;
using LagoVista.IoT.Deployment.Admin.Models;
using LagoVista.IoT.DeviceAdmin.Interfaces.Managers;
using LagoVista.IoT.DeviceAdmin.Models;
using LagoVista.IoT.DeviceManagement.Core.Managers;
using LagoVista.IoT.DeviceManagement.Core.Models;
using LagoVista.IoT.DeviceMessaging.Admin.Managers;
using LagoVista.IoT.DeviceMessaging.Admin.Models;
using LagoVista.IoT.Pipeline.Admin.Managers;
using LagoVista.IoT.Pipeline.Admin.Models;
using LagoVista.IoT.Verifiers.Managers;
using LagoVista.ProjectManagement;
using LagoVista.ProjectManagement.Models;
using LagoVista.UserAdmin.Interfaces.Managers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.IoT.StarterKit.Managers
{
    public class CloneServices
    {
        IDeviceAdminManager _deviceAdminMgr;
        ISubscriptionManager _subscriptionMgr;
        IPipelineModuleManager _pipelineMgr;
        IDeviceTypeManager _deviceTypeMgr;
        IDeviceMessageDefinitionManager _deviceMsgMgr;
        IDeploymentHostManager _hostMgr;
        IDeploymentInstanceManager _instanceMgr;
        ISolutionManager _solutionMgr;
        IDeviceConfigurationManager _deviceCfgMgr;
        IDeviceRepositoryManager _deviceRepoMgr;
        IProductManager _productManager;
        IVerifierManager _verifierMgr;
        IProjectRepo _projectRepo;
        IWorkTaskRepo _taskRepo;
       

        public CloneServices(IDeviceAdminManager deviceAdminMgr, ISubscriptionManager subscriptionMgr, IPipelineModuleManager pipelineMgr, IDeviceTypeManager deviceTypeMgr, IDeviceRepositoryManager deviceRepoMgr,
           IProductManager productManager, IDeviceConfigurationManager deviceCfgMgr, IDeviceMessageDefinitionManager deviceMsgMgr, IDeploymentHostManager hostMgr, IDeploymentInstanceManager instanceMgr,
           ISolutionManager solutionMgr, IVerifierManager verifierMgr, IProjectRepo projectRepo, IWorkTaskRepo taskRepo)
        {
            _deviceAdminMgr = deviceAdminMgr;
            _subscriptionMgr = subscriptionMgr;
            _pipelineMgr = pipelineMgr;
            _deviceTypeMgr = deviceTypeMgr;
            _deviceMsgMgr = deviceMsgMgr;
            _deviceCfgMgr = deviceCfgMgr;
            _deviceRepoMgr = deviceRepoMgr;
            _productManager = productManager;
            _verifierMgr = verifierMgr;

            _hostMgr = hostMgr;
            _instanceMgr = instanceMgr;
            _solutionMgr = solutionMgr;

            _projectRepo = projectRepo;
            _taskRepo = taskRepo;
        }

        public async Task<DeviceMessageDefinition> CloneMessageAsync(string originalMessageId, EntityHeader org, EntityHeader user)
        {
            var msg = await this._deviceMsgMgr.GetDeviceMessageDefinitionAsync(originalMessageId, org, user);
            var clonedMsg = await msg.CloneAsync(user, org, msg.Name, msg.Key);
            await this._deviceMsgMgr.AddDeviceMessageDefinitionAsync(clonedMsg, org, user);
            return msg;
        }

        #region Items that are "atomic" (mostly stand alone)
        public Task<DeviceType> CloneDeviceTypeAsync(string originalListenerId, EntityHeader org, EntityHeader user)
        {
            throw new NotImplementedException();
        }

        public void CloneListenerAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PlannerConfiguration> ClonePlannerAsync()
        {
            throw new NotImplementedException();
        }

        public Task<SentinelConfiguration> CloneSentinalAsync()
        {
            throw new NotImplementedException();
        }

        public Task<InputTranslatorConfiguration> CloneInputTranslatorAsync()
        {
            throw new NotImplementedException();
        }

        public Task<DeviceWorkflow> CloneDeviceWorkflowAsync()
        {
            throw new NotImplementedException();
        }

        public Task<DeviceWorkflow> CloneOutputTranslatorAysnc()
        {
            throw new NotImplementedException();
        }
        #endregion


        #region Items that require sub-objects
        public Task<DeviceConfiguration> CloneDeviceConfigurationAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Solution> CloneSolutionAsync()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region top level items requiring a subscription
        public Task<DeploymentInstance> CloneDeploymentInstanceAsync()
        {
            throw new NotImplementedException();
        }

        public Task<DeviceRepository> CloneDeviceRepository()
        {
            throw new NotImplementedException();
        }
        #endregion

        public async Task<Project> CloneProjectAsync(string originalProjectId, string name, string code, EntityHeader org, EntityHeader user)
        {
            var project = await _projectRepo.GetProjectAsync(originalProjectId);
            if (!project.IsPublic && project.OwnerOrganization.Id != org.Id)
                throw new NotAuthorizedException($"Attempt to clone a non public project that is not owned by the current org.  Current Org: {org.Text}  Owner Org: {project.OwnerOrganization.Text}");

            var timeStamp = DateTime.UtcNow.ToJSONString();

            project.Id = Guid.NewGuid().ToId();
            project.Name = name;
            project.ProjectCode = code;
            project.CreatedBy = user;
            project.LastUpdatedBy = user;
            project.CreationDate = timeStamp;
            project.LastUpdatedDate = timeStamp;
            project.OwnerOrganization = org;

            await _projectRepo.AddProjectAsync(project);

            var request = new ListRequest()
            {
                PageSize = 99999
            };

            var tasks = await _taskRepo.GetTasksForProjectAsync(originalProjectId, org.Id, request);
            var idx = 0;
            foreach(var taskSummary in tasks.Model)
            {
                var task = await _taskRepo.GetWorkTaskAsync(taskSummary.Id);
                task.Project = EntityHeader.Create(project.Id, project.Key, project.Name);
                task.CreatedBy = user;
                task.LastUpdatedBy = user;
                task.CreationDate = timeStamp;
                task.LastUpdatedDate = timeStamp;
                task.OwnerOrganization = org;
                task.TaskCode = $"{project.ProjectCode}-{idx:00000}";
                await _taskRepo.AddWorkTaskAsync(task);
            }

            return project;
        }

    }
}
