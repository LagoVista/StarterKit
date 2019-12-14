using LagoVista.Core.Models;
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

        public CloneServices(IDeviceAdminManager deviceAdminMgr, ISubscriptionManager subscriptionMgr, IPipelineModuleManager pipelineMgr, IDeviceTypeManager deviceTypeMgr, IDeviceRepositoryManager deviceRepoMgr,
           IProductManager productManager, IDeviceConfigurationManager deviceCfgMgr, IDeviceMessageDefinitionManager deviceMsgMgr, IDeploymentHostManager hostMgr, IDeploymentInstanceManager instanceMgr,
           ISolutionManager solutionMgr, IVerifierManager verifierMgr)
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

    }
}
