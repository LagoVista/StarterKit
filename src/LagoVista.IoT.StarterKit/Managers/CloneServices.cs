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
    public class CloneServices : ICloneServices
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

        public async Task<Project> CloneProjectAsync(CloneProjectRequest cloneRequest, EntityHeader org, EntityHeader user)
        {
            var project = await _projectRepo.GetProjectAsync(cloneRequest.OriginalProject.Id);
            if (!project.IsPublic && project.OwnerOrganization.Id != org.Id)
                throw new NotAuthorizedException($"Attempt to clone a non public project that is not owned by the current org.  Current Org: {org.Text}  Owner Org: {project.OwnerOrganization.Text}");

            var timeStamp = DateTime.UtcNow.ToJSONString();

            project.Id = Guid.NewGuid().ToId();
            project.Name = cloneRequest.NewProjectName;
            project.ProjectCode = cloneRequest.NewProjectCode;
            project.CreatedBy = user;
            project.LastUpdatedBy = user;
            project.CreationDate = timeStamp;
            project.LastUpdatedDate = timeStamp;
            project.OwnerOrganization = org;
            project.CurrentTaskIndex = 1;
            project.HoursUsed = 0;
            project.Status = EntityHeader<ProjectStatus>.Create(ProjectStatus.Pending);
            project.Customer = cloneRequest.Customer;
            project.Agreement = cloneRequest.Agreement;
            project.Sprints = new List<EnumDescription>();
            project.Notes = new List<ProjectNotes>();
            project.ProjectLead = cloneRequest.ProjectLead;
            project.DefaultPrimaryContributor = cloneRequest.DefaultPrimaryContributor;
            project.DefaultQAResource = cloneRequest.DefaultQAResource;
            project.ProjectAdminLead = cloneRequest.ProjectAdminLead;

            var request = new ListRequest()
            {
                PageSize = 99999
            };

            var tasks = await _taskRepo.GetTasksForProjectAsync(cloneRequest.OriginalProject.Id, org.Id, request);
            foreach (var taskSummary in tasks.Model)
            {
                var task = await _taskRepo.GetWorkTaskAsync(taskSummary.Id);
                task.Project = EntityHeader.Create(project.Id, project.Key, project.Name);
                task.CreatedBy = user;
                task.LastUpdatedBy = user;
                task.CreationDate = timeStamp;
                task.LastUpdatedDate = timeStamp;
                task.OwnerOrganization = org;
                task.Discussions = new List<WorkTaskDiscussion>();
                task.Issues = new List<WorkTaskIssue>();
                task.DueDate = null;
                task.TaskCode = $"{project.ProjectCode}-{project.CurrentTaskIndex++:00000}";
                task.History = new List<WorkTaskHistory>();
                task.AddHistory("Cloned", $"Cloned from Project {project.Name}.", user);
                task.AssignedByUser = user;
                task.AssignedToUser = cloneRequest.ProjectLead;
                task.QaResource = cloneRequest.DefaultQAResource;
                task.PrimaryContributorUser = cloneRequest.DefaultPrimaryContributor;
                task.ExternalTaskCode = null;
                task.ExternalTaskLink = null;
                task.ExternalId = null;
                task.ExternalStatusConfigurationType = null;

                task.ExpectedCloseDate = null;

                var idx = 1;
                foreach (var outcome in task.ExpectedOutcomes)
                {
                    outcome.Id = Guid.NewGuid().ToId();
                    outcome.ExpectedOutcomeCode = $"{task.TaskCode}-EO-{idx++}";
                    outcome.VerifiedBy = null;
                    outcome.CreationDate = timeStamp;
                    outcome.VerificationDate = null;
                    outcome.CompletionDate = null;
                    outcome.Verified = false;
                    outcome.Completed = false;
                    outcome.CompletedBy = null;
                    outcome.CompletionDate = null;
                    outcome.Attachments = new List<Attachment>();
                    outcome.VerificationRuns = new List<VerificationRun>();
                    foreach (var step in outcome.VerificationSteps)
                    {
                        step.Id = Guid.NewGuid().ToId();
                    }
                }

                idx = 1;
                foreach (var subtask in task.SubTasks)
                {
                    subtask.Id = Guid.NewGuid().ToId();
                    subtask.SubTaskCode = $"{task.TaskCode}-ST-{idx++}";
                    subtask.CreationDate = timeStamp;
                    subtask.Issues = new List<WorkTaskIssue>();
                    subtask.Discussions = new List<WorkTaskDiscussion>();
                    subtask.AssignedToUser = cloneRequest.DefaultPrimaryContributor;
                    subtask.AssignedByUser = user;
                }
                await _taskRepo.AddWorkTaskAsync(task);
            }

            foreach (var module in project.Modules)
            {
                module.Id = Guid.NewGuid().ToId();
                module.CreatedBy = user;
                module.LastUpdatedBy = user;
                module.CreationDate = timeStamp;
                module.LastUpdatedDate = timeStamp;
            }

            await _projectRepo.AddProjectAsync(project);

            return project;
        }
    }
}
