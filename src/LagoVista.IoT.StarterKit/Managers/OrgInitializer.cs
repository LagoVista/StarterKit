using LagoVista.Core.Models;
using LagoVista.IoT.Billing;
using LagoVista.IoT.Deployment.Admin;
using LagoVista.IoT.DeviceAdmin.Interfaces.Managers;
using LagoVista.IoT.DeviceManagement.Core.Managers;
using LagoVista.IoT.DeviceMessaging.Admin.Managers;
using LagoVista.IoT.Pipeline.Admin.Managers;
using LagoVista.IoT.Pipeline.Admin.Models;
using LagoVista.IoT.Verifiers.Managers;
using LagoVista.UserAdmin;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Models.Orgs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.IoT.StarterKit.Managers
{
    public class OrgInitializer : InitializerBase, IOrgInitializer
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


        public OrgInitializer(IDeviceAdminManager deviceAdminMgr, ISubscriptionManager subscriptionMgr, IPipelineModuleManager pipelineMgr, IDeviceTypeManager deviceTypeMgr, IDeviceRepositoryManager deviceRepoMgr,
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

        public async Task Init(EntityHeader org, EntityHeader user, bool populateSampleData)
        {
            var creationTimeStamp = DateTime.UtcNow;
            await this.AddTrialSubscriptionAsync(org, user, creationTimeStamp);
            await this.AddInputTranslatorAsync(org, user, creationTimeStamp);
            await this.AddOutputTranslatorsAsync(org, user, creationTimeStamp);
        }

        public async Task<Subscription> AddTrialSubscriptionAsync(EntityHeader org, EntityHeader user, DateTime createTimeStamp)
        {
            var subscription = new Subscription()
            {
                Id = Guid.NewGuid(),
                OrgId = org.Id,
                Name = "Trial Subscription",
                Key = "trialsubscription",
                Status = Subscription.Status_FreeAccount,
                CreatedById = user.Id,                
                LastUpdatedById = user.Id,
                CreationDate = createTimeStamp,
                LastUpdatedDate = createTimeStamp,
            };

            await this._subscriptionMgr.AddSubscriptionAsync(subscription, org, user);           

            return subscription;
        }

        public async Task AddInputTranslatorAsync(EntityHeader org, EntityHeader user, DateTime createTimestamp)
        {
            var inputTranslator = new InputTranslatorConfiguration();
            inputTranslator.Name = "Default Input Translator";
            inputTranslator.Key = "default";
            inputTranslator.InputTranslatorType = EntityHeader<InputTranslatorConfiguration.InputTranslatorTypes>.Create(InputTranslatorConfiguration.InputTranslatorTypes.MessageBased);
            this.AddAuditProperties(inputTranslator, createTimestamp, org, user);
            this.AddId(inputTranslator);
            this.AddOwnedProperties(inputTranslator, org);
            await this._pipelineMgr.AddInputTranslatorConfigurationAsync(inputTranslator, org, user);
        }

        public async Task AddOutputTranslatorsAsync(EntityHeader org, EntityHeader user, DateTime createTimestamp)
        {
            var outputTranslator = new OutputTranslatorConfiguration();
            outputTranslator.Name = "Default Output Translator";
            outputTranslator.Key = "default";
            outputTranslator.OutputTranslatorType = EntityHeader<OutputTranslatorConfiguration.OutputTranslatorTypes>.Create(OutputTranslatorConfiguration.OutputTranslatorTypes.MessageBased);
            this.AddAuditProperties(outputTranslator, createTimestamp, org, user);
            this.AddId(outputTranslator);
            this.AddOwnedProperties(outputTranslator, org);

            await this._pipelineMgr.AddOutputTranslatorConfigurationAsync(outputTranslator, org, user);
        }
    }
}
