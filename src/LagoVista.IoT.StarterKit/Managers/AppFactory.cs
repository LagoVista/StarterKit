using System;
using System.Collections.Generic;
using System.Text;
using LagoVista.IoT.DeviceAdmin.Interfaces.Managers;
using LagoVista.IoT.DeviceAdmin.Models;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.IoT.Pipeline.Admin.Managers;
using LagoVista.IoT.DeviceMessaging.Admin.Managers;
using LagoVista.IoT.Deployment.Admin;
using LagoVista.Core.Validation;
using System.Threading.Tasks;
using LagoVista.Core.Models;
using LagoVista.Core.Interfaces;
using LagoVista.Core;
using LagoVista.IoT.Pipeline.Admin.Models;

namespace LagoVista.IoT.StarterKit.Managers
{
    public class AppFactory
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

        public AppFactory(IDeviceAdminManager deviceAdminMgr, ISubscriptionManager subscriptionMgr, IPipelineModuleManager pipelineMgr, IDeviceTypeManager deviceTypeMgr,
           IDeviceConfigurationManager deviceCfgMgr, IDeviceMessageDefinitionManager deviceMsgMgr,  IDeploymentHostManager hostMgr, IDeploymentInstanceManager instanceMgr, ISolutionManager solutionMgr)
        {
            _deviceAdminMgr = deviceAdminMgr;
            _subscriptionMgr = subscriptionMgr;
            _pipelineMgr = pipelineMgr;
            _deviceTypeMgr = deviceTypeMgr;
            _deviceMsgMgr = deviceMsgMgr;
            _deviceCfgMgr = deviceCfgMgr;

            _hostMgr = hostMgr;
            _instanceMgr = instanceMgr;
            _solutionMgr = solutionMgr;
        }

        private void AddAuditProperties(IAuditableEntity entity, DateTime creationTimeStamp, EntityHeader org, EntityHeader user)
        {
            entity.CreationDate = creationTimeStamp.ToJSONString();
            entity.LastUpdatedDate = creationTimeStamp.ToJSONString();
            entity.CreatedBy = user;
            entity.LastUpdatedBy = user;
        }

        private void AddOwnedProperties(IOwnedEntity entity, EntityHeader org)
        {
            entity.OwnerOrganization = org;
            entity.IsPublic = false;
        }

        private void AddId(IIDEntity entity)
        {
            entity.Id = Guid.NewGuid().ToId();
        }

        public async Task<InvokeResult> CreateSimpleAsync(EntityHeader org, EntityHeader user)
        {
            var createTimeStamp = DateTime.UtcNow;

            /* Create unit/state sets */
            var stateSet = new StateSet() { Key = "onoff", Name = "On/Off", Description="Provides two simple states, On and Off", States = new List<State> { new State() { Key = "on", Name = "On", IsInitialState = true }, new State() { Key = "off", Name = "Off" } } };
            AddId(stateSet);
            AddOwnedProperties(stateSet, org);
            AddAuditProperties(stateSet, createTimeStamp, org, user);

            await _deviceAdminMgr.AddStateSetAsync(stateSet, org, user);

            var unitSet = new UnitSet();
            await _deviceAdminMgr.AddUnitSetAsync(unitSet, org, user);


            /* Create Device Workflow */
            var wf = new DeviceWorkflow()
            {
                Key = "sample",
                Name = "Sample",
                Description = "Sample Workflow",
                Attributes = new List<DeviceAdmin.Models.Attribute>() { },
                Inputs = new List<WorkflowInput>() { },
                InputCommands = new List<InputCommand>() { },
                StateMachines = new List<StateMachine>() { },
                OutputCommands = new List<OutputCommand>() { }
            };
            AddId(wf);
            AddOwnedProperties(wf, org);
            AddAuditProperties(wf, createTimeStamp, org, user);
            await _deviceAdminMgr.AddDeviceWorkflowAsync(wf, org, user);

            /* Create Pipeline Modules */
            var restListener = new ListenerConfiguration()
            {
                Name = "Sample Rest",
                Key = "samplereset",
                ListenerType = EntityHeader<ListenerTypes>.Create(ListenerTypes.Rest),
                RestServerType = EntityHeader<RESTServerTypes>.Create(RESTServerTypes.HTTP),
                ListenOnPort = 80,
            };
            AddId(wf);
            AddOwnedProperties(wf, org);
            AddAuditProperties(wf, createTimeStamp, org, user);
            await _pipelineMgr.AddListenerConfigurationAsync(restListener, org, user);

            /* Create Route */

            /* Create Device Configurations */

            /* Create Solution */

            /* Create Instance */

            /* Create Subscription */

            return InvokeResult.Success;
        }

    }
}
