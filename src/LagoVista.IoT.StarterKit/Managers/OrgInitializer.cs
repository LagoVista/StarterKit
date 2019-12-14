using LagoVista.Core.Models;
using LagoVista.Core;
using LagoVista.IoT.Billing;
using LagoVista.IoT.Deployment.Admin;
using LagoVista.IoT.DeviceAdmin.Interfaces.Managers;
using LagoVista.IoT.DeviceManagement.Core.Managers;
using LagoVista.IoT.DeviceManagement.Core.Models;
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
using LagoVista.IoT.DeviceAdmin.Models;
using LagoVista.IoT.Deployment.Admin.Models;
using LagoVista.IoT.DeviceMessaging.Admin.Models;
using System.Linq;
using LagoVista.IoT.Pipeline.Models;
using LagoVista.CloudStorage.Storage;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.Core.Validation;
using LagoVista.IoT.DeviceAdmin.Models.Resources;

namespace LagoVista.IoT.StarterKit.Managers
{
    public class OrgInitializer : InitializerBase, IOrgInitializer
    {
        IDeviceAdminManager _deviceAdminMgr;
        ISubscriptionManager _subscriptionMgr;
        IPipelineModuleManager _pipelineMgr;
        IDeviceTypeManager _deviceTypeMgr;
        IDeviceMessageDefinitionManager _deviceMsgMgr;
        IDeploymentInstanceManager _instanceMgr;
        ISolutionManager _solutionMgr;
        IDeviceConfigurationManager _deviceCfgMgr;
        IDeviceRepositoryManager _deviceRepoMgr;
        IProductManager _productManager;
        IVerifierManager _verifierMgr;

        StorageUtils _storageUtils;

        public OrgInitializer(IAdminLogger logger, IStarterKitConnection starterKitConnection, IDeviceAdminManager deviceAdminMgr, ISubscriptionManager subscriptionMgr, IPipelineModuleManager pipelineMgr, IDeviceTypeManager deviceTypeMgr, IDeviceRepositoryManager deviceRepoMgr,
                          IProductManager productManager, IDeviceTypeManager deviceTypeManager, IDeviceConfigurationManager deviceCfgMgr, IDeviceMessageDefinitionManager deviceMsgMgr, IDeploymentInstanceManager instanceMgr,
                          IContainerRepositoryManager containerMgr, ISolutionManager solutionMgr, IVerifierManager verifierMgr)
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
            _deviceTypeMgr = deviceTypeManager;

            _instanceMgr = instanceMgr;
            _solutionMgr = solutionMgr;

            _storageUtils = new StorageUtils(new Uri(starterKitConnection.StarterKitStorage.Uri), starterKitConnection.StarterKitStorage.AccessKey,
                starterKitConnection.StarterKitStorage.ResourceName, logger);
        }

        private const string EXAMPLE_MOTION_KEY = "examplemotion";
        private const string DEFAULT = "default";
        private const string ANONYMOUS = "anonymous";
        private const string TRIAL = "trial";

        public async Task Init(EntityHeader org, EntityHeader user, bool populateSampleData)
        {
            var creationTimeStamp = DateTime.UtcNow;
            await this.AddTrialSubscriptionAsync(org, user, creationTimeStamp);

            await this.AddOutputTranslatorsAsync(org, user, creationTimeStamp);
            await this.AddAnonymousSentinelAsync(org, user, creationTimeStamp);
            await this.AddInputTranslatorAsync(org, user, creationTimeStamp);
        }

        public async Task<InvokeResult> CreateExampleAppAsync(EntityHeader org, EntityHeader user)
        {
            var creationTimeStamp = DateTime.UtcNow;

            var defaultInputTranslator = await _storageUtils.FindWithKeyAsync<InputTranslatorConfiguration>(DEFAULT, org, false);
            if (defaultInputTranslator == null)
            {
                defaultInputTranslator = await AddInputTranslatorAsync(org, user, creationTimeStamp);
            }

            var defaultOutputTranslator = await _storageUtils.FindWithKeyAsync<OutputTranslatorConfiguration>(DEFAULT, org, false);
            if (defaultOutputTranslator == null)
            {
                defaultOutputTranslator = await AddOutputTranslatorsAsync(org, user, creationTimeStamp);
            }

            var defaultSentinal = await _storageUtils.FindWithKeyAsync<SentinelConfiguration>(ANONYMOUS, org, false);
            if (defaultSentinal == null)
            {
                defaultSentinal = await AddAnonymousSentinelAsync(org, user, creationTimeStamp);
            }

            var subscription = await _subscriptionMgr.GetTrialSubscriptionAsync(org, user);
            if (subscription == null)
            {
                subscription = await this.AddTrialSubscriptionAsync(org, user, creationTimeStamp);
            }

            var msg = await this.AddDefaultMessage("Example - Motion Message", EXAMPLE_MOTION_KEY, org, user, creationTimeStamp);
            var workflow = await this.AddDeviceWorkflow("Example - Motion", EXAMPLE_MOTION_KEY, org, user, creationTimeStamp);
            var listener = await this.AddPort80Listener("exampleport80", org, user, creationTimeStamp);

            var deviceConfig = await this.AddDeviceConfigAsync("Exmaple - Motion", EXAMPLE_MOTION_KEY,
                defaultSentinal, defaultInputTranslator, workflow, defaultOutputTranslator,
                msg, org, user, creationTimeStamp);
            await this.AddDeviceType("Example - Motion", EXAMPLE_MOTION_KEY, deviceConfig, org, user, creationTimeStamp);

            var planner = await this.AddPlanner("Example", "exampleplanner", org, user, creationTimeStamp);
            var solution = await this.CreateSolutionAsync("Example - Motion", EXAMPLE_MOTION_KEY, org, user, creationTimeStamp, planner, deviceConfig, listener);

            var trialRepo = await _storageUtils.FindWithKeyAsync<DeviceRepository>(TRIAL, org, false);
            if (trialRepo == null)
            {
                await this.AddTrialRepository("25 Device Trial Repository", TRIAL, subscription, org, user, creationTimeStamp);
            }

            await this.CreateInstanceAsync(subscription, solution, trialRepo, "Example - Motion", EXAMPLE_MOTION_KEY, org, user, creationTimeStamp);

            return InvokeResult.Success;
        }

        public async Task DeleteExample(EntityHeader org, EntityHeader user)
        {
            await _storageUtils.DeleteIfExistsAsync<DeviceMessageDefinition>(EXAMPLE_MOTION_KEY, org);
            await _storageUtils.DeleteIfExistsAsync<PlannerConfiguration>(EXAMPLE_MOTION_KEY, org);
            await _storageUtils.DeleteIfExistsAsync<DeviceType>(EXAMPLE_MOTION_KEY, org);
            await _storageUtils.DeleteIfExistsAsync<DeviceConfiguration>(EXAMPLE_MOTION_KEY, org);
        }

        public async Task<Subscription> AddTrialSubscriptionAsync(EntityHeader org, EntityHeader user, DateTime createTimeStamp)
        {
            var subscription = new Subscription()
            {
                Id = Guid.NewGuid(),
                OrgId = org.Id,
                Name = "Trial Subscription",
                Key = Subscription.SubscriptionKey_Trial,
                Status = Subscription.Status_OK,
                CreatedById = user.Id,
                LastUpdatedById = user.Id,
                CreationDate = createTimeStamp,
                LastUpdatedDate = createTimeStamp,
            };

            await this._subscriptionMgr.AddSubscriptionAsync(subscription, org, user);

            return subscription;
        }

        public async Task<DeviceMessageDefinition> AddDefaultMessage(string name, string key, EntityHeader org, EntityHeader user, DateTime createTimeStamp)
        {
            await _storageUtils.DeleteIfExistsAsync<DeviceMessageDefinition>(key, org);

            var msgDefinition = new DeviceMessageDefinition()
            {
                Id = Guid.NewGuid().ToId(),
                Name = name,
                Key = key,
                MessageId = "motion",
                MessageDirection = EntityHeader<MessageDirections>.Create(MessageDirections.Incoming),
                ContentType = EntityHeader<MessageContentTypes>.Create(MessageContentTypes.JSON)
            };

            msgDefinition.Fields.Add(new DeviceMessageDefinitionField()
            {
                ContentType = EntityHeader<MessageContentTypes>.Create(MessageContentTypes.JSON),
                FieldIndex = 0,
                Name = "Motion Status",
                SearchLocation = EntityHeader<SearchLocations>.Create(SearchLocations.Body),
                Id = Guid.NewGuid().ToId(),
                JsonPath = "motion",
                Key = "motion",
                StorageType = EntityHeader<ParameterTypes>.Create(ParameterTypes.String),
                ParsedStringFieldType = EntityHeader<ParseStringValueType>.Create(ParseStringValueType.String),
            });

            AddOwnedProperties(msgDefinition, org);
            AddAuditProperties(msgDefinition, createTimeStamp, org, user);

            await _deviceMsgMgr.AddDeviceMessageDefinitionAsync(msgDefinition, org, user);

            return msgDefinition;
        }

        public async Task<DeviceConfiguration> AddDeviceConfigAsync(string name, string key, SentinelConfiguration sentinal,
            InputTranslatorConfiguration inputTranslator, DeviceWorkflow workflow, OutputTranslatorConfiguration output,
            DeviceMessageDefinition msg, EntityHeader org, EntityHeader user, DateTime createTimestamp)
        {
            if (sentinal == null) throw new ArgumentNullException(nameof(sentinal));
            if (inputTranslator == null) throw new ArgumentNullException(nameof(inputTranslator));
            if (workflow == null) throw new ArgumentNullException(nameof(workflow));
            if (output == null) throw new ArgumentNullException(nameof(output));
            if (msg == null) throw new ArgumentNullException(nameof(msg));

            await _storageUtils.DeleteIfExistsAsync<DeviceConfiguration>(key, org);

            var deviceConfig = new DeviceConfiguration()
            {
                Id = Guid.NewGuid().ToId(),
                Name = name,
                Key = key,
            };

            AddOwnedProperties(deviceConfig, org);
            AddAuditProperties(deviceConfig, createTimestamp, org, user);

            var route = Route.Create();
            route.Name = "Motion Message Handler";
            route.Key = EXAMPLE_MOTION_KEY;
            AddId(route);
            AddAuditProperties(route, createTimestamp, org, user);
            route.MessageDefinition = new EntityHeader<DeviceMessageDefinition>()
            {
                Id = msg.Id,
                Text = msg.Name
            };

            route.PipelineModules.Where(pm => pm.ModuleType.Value == PipelineModuleType.Sentinel).First().Module = new EntityHeader<DeviceAdmin.Interfaces.IPipelineModuleConfiguration>()
            {
                Id = sentinal.Id,
                Text = sentinal.Name
            };

            route.PipelineModules.Where(pm => pm.ModuleType.Value == PipelineModuleType.InputTranslator).First().Module = new EntityHeader<DeviceAdmin.Interfaces.IPipelineModuleConfiguration>()
            {
                Id = inputTranslator.Id,
                Text = inputTranslator.Name
            };

            route.PipelineModules.Where(pm => pm.ModuleType.Value == PipelineModuleType.Workflow).First().Module = new EntityHeader<DeviceAdmin.Interfaces.IPipelineModuleConfiguration>()
            {
                Id = workflow.Id,
                Text = workflow.Name
            };

            route.PipelineModules.Where(pm => pm.ModuleType.Value == PipelineModuleType.OutputTranslator).First().Module = new EntityHeader<DeviceAdmin.Interfaces.IPipelineModuleConfiguration>()
            {
                Id = output.Id,
                Text = output.Name
            };

            deviceConfig.Routes.Add(route);

            await _deviceCfgMgr.AddDeviceConfigurationAsync(deviceConfig, org, user);


            return deviceConfig;
        }

        public async Task<Solution> CreateSolutionAsync(string name, string key, EntityHeader org, EntityHeader user, DateTime createTimestamp,
            PlannerConfiguration planner, DeviceConfiguration deviceConfig, ListenerConfiguration listener)
        {
            await _storageUtils.DeleteIfExistsAsync<Solution>(key, org);

            var solution = new Solution()
            {
                Name = name,
                Key = key
            };

            AddId(solution);
            AddOwnedProperties(solution, org);
            AddAuditProperties(solution, createTimestamp, org, user);

            solution.Planner = new EntityHeader<PlannerConfiguration>() { Id = planner.Id, Text = planner.Name };
            solution.DeviceConfigurations.Add(new EntityHeader<DeviceConfiguration>() { Id = deviceConfig.Id, Text = deviceConfig.Name });
            solution.Listeners.Add(new EntityHeader<ListenerConfiguration>() { Id = listener.Id, Text = listener.Name });
            await _solutionMgr.AddSolutionsAsync(solution, org, user);

            return solution;
        }

        public async Task<DeviceType> AddDeviceType(string name, string key, DeviceConfiguration deviceConfg, EntityHeader org, EntityHeader user, DateTime createTimestamp)
        {
            await _storageUtils.DeleteIfExistsAsync<DeviceType>(key, org);

            var deviceType = new DeviceType()
            {
                Id = Guid.NewGuid().ToId(),
                Name = name,
                Key = key,
                DefaultDeviceConfiguration = new EntityHeader() { Id = deviceConfg.Id, Text = deviceConfg.Name }
            };

            AddOwnedProperties(deviceType, org);
            AddAuditProperties(deviceType, createTimestamp, org, user);

            await _deviceTypeMgr.AddDeviceTypeAsync(deviceType, org, user);

            return deviceType;
        }

        public async Task<DeviceWorkflow> AddDeviceWorkflow(String name, string key, EntityHeader org, EntityHeader user, DateTime createTimestamp)
        {
            await _storageUtils.DeleteIfExistsAsync<DeviceWorkflow>(key, org);

            var wf = new DeviceWorkflow()
            {
                Id = Guid.NewGuid().ToId(),
                Name = name,
                Key = key,
            };

            wf.Pages.Add(new Page()
            {
                PageNumber = 1,
                Name = DeviceLibraryResources.Common_PageNumberOne
            });

            var attr = new DeviceAdmin.Models.Attribute()
            {
                Name = "Motion Status",
                Key = "motionstatus",
            };

            attr.AttributeType = EntityHeader<ParameterTypes>.Create(ParameterTypes.String);
            AddOwnedProperties(attr, org);
            AddAuditProperties(attr, createTimestamp, org, user);
            AddId(attr);

            attr.DiagramLocations.Add(new DiagramLocation()
            {
                Page = 1,
                X = 120,
                Y = 120,
            });

            var input = new DeviceAdmin.Models.WorkflowInput()
            {
                Name = "Motion Status",
                Key = "motionstatus",
            };

            input.InputType = EntityHeader<ParameterTypes>.Create(ParameterTypes.String);
            AddOwnedProperties(input, org);
            AddAuditProperties(input, createTimestamp, org, user);
            AddId(input);

            input.DiagramLocations.Add(new DiagramLocation()
            {
                Page = 1,
                X = 20,
                Y = 20,
            });

            wf.Attributes.Add(attr);
            wf.Inputs.Add(input);

            AddOwnedProperties(wf, org);
            AddAuditProperties(wf, createTimestamp, org, user);

            await this._deviceAdminMgr.AddDeviceWorkflowAsync(wf, org, user);


            return wf;
        }

        public async Task<PlannerConfiguration> AddPlanner(string name, string key, EntityHeader org, EntityHeader user, DateTime createTimestamp)
        {
            await _storageUtils.DeleteIfExistsAsync<PlannerConfiguration>(key, org);

            var planner = new PlannerConfiguration()
            {
                Id = Guid.NewGuid().ToId(),
                Name = name,
                Key = key,
                Description = "This planner will be used to identify the message id and device id from a URL path.  For the URL path /api/motion/dev001, it will identify the message id [motion] and device id [dev001]."
            };

            AddOwnedProperties(planner, org);
            AddAuditProperties(planner, createTimestamp, org, user);

            planner.DeviceIdParsers.Add(new DeviceMessaging.Admin.Models.DeviceMessageDefinitionField()
            {
                Name = "Device ID in path",
                Key = "deviceidinpath",
                Notes = "Will extract the device id from the provided URL path in this example it would extract [dev001] from the path /api/motion/dev001.  See help for more details.",
                SearchLocation = EntityHeader<DeviceMessaging.Admin.Models.SearchLocations>.Create(DeviceMessaging.Admin.Models.SearchLocations.Path),
                PathLocator = "/api/*/{messageidinpath}"
            });

            planner.MessageTypeIdParsers.Add(new DeviceMessaging.Admin.Models.DeviceMessageDefinitionField()
            {
                Name = "Message Id in path",
                Key = "messageidinpath",
                Notes = "Will extract message id from the provided URL path, in this example it would extract [motion] from the path /api/motion/dev001.  See help for more details.",
                SearchLocation = EntityHeader<DeviceMessaging.Admin.Models.SearchLocations>.Create(DeviceMessaging.Admin.Models.SearchLocations.Path),
                PathLocator = "/api/{messageidinpath}/*"
            });

            await this._pipelineMgr.AddPlannerConfigurationAsync(planner, org, user);

            return planner;
        }

        public async Task<DeviceRepository> AddTrialRepository(string name, string key, Subscription subscription, EntityHeader org, EntityHeader user, DateTime createTimestamp)
        {
            var repo = new DeviceRepository()
            {
                Key = key,
                Name = name,
                Description = "Trial Device Repository that you can use with up to 25 devices.  Contact Software Logistics if you need additional devices",
                RepositoryType = EntityHeader<RepositoryTypes>.Create(RepositoryTypes.NuvIoT),
                DeviceCapacity = EntityHeader.Create("trialdevices", "25 Device Trial"),
                StorageCapacity = EntityHeader.Create("trialstorage", "200mb Trial Storage"),
                Subscription = EntityHeader.Create(subscription.Id.ToString(), subscription.Name),
            };

            AddId(repo);
            AddOwnedProperties(repo, org);
            AddAuditProperties(repo, createTimestamp, org, user);

            await this._deviceRepoMgr.AddDeviceRepositoryAsync(repo, org, user);

            return repo;
        }

        public async Task<ListenerConfiguration> AddPort80Listener(string key, EntityHeader org, EntityHeader user, DateTime createTimestamp)
        {
            await _storageUtils.DeleteIfExistsAsync<ListenerConfiguration>(key, org);

            var port80Listener = new ListenerConfiguration();
            port80Listener.Name = "Port 80 REST Listener";
            port80Listener.Key = key;
            port80Listener.Description = "A simple listener that will listen on port 80 for incoming REST requests.";
            port80Listener.ListenerType = EntityHeader<ListenerTypes>.Create(ListenerTypes.Rest);
            port80Listener.ListenOnPort = 80;
            port80Listener.Anonymous = true;
            port80Listener.SecureConnection = false;
            port80Listener.RESTListenerType = RESTListenerTypes.PipelineModule;
            port80Listener.RestServerType = EntityHeader<RESTServerTypes>.Create(RESTServerTypes.HTTP);
            port80Listener.ContentType = EntityHeader<DeviceMessaging.Admin.Models.MessageContentTypes>.Create(DeviceMessaging.Admin.Models.MessageContentTypes.JSON);

            this.AddAuditProperties(port80Listener, createTimestamp, org, user);
            this.AddId(port80Listener);
            this.AddOwnedProperties(port80Listener, org);

            await this._pipelineMgr.AddListenerConfigurationAsync(port80Listener, org, user);

            return port80Listener;
        }

        public async Task<SentinelConfiguration> AddAnonymousSentinelAsync(EntityHeader org, EntityHeader user, DateTime createTimestamp)
        {
            var sentinal = new SentinelConfiguration();
            sentinal.Name = "Anonymous";
            sentinal.Key = ANONYMOUS;
            sentinal.Description = "The Anonymous sentinel module is appropriate for development or devices where authentication is performed via the transport.";
            this.AddAuditProperties(sentinal, createTimestamp, org, user);
            this.AddId(sentinal);
            this.AddOwnedProperties(sentinal, org);

            await this._pipelineMgr.AddSentinelConfigurationAsync(sentinal, org, user);

            return sentinal;
        }

        public async Task<InputTranslatorConfiguration> AddInputTranslatorAsync(EntityHeader org, EntityHeader user, DateTime createTimestamp)
        {
            var inputTranslator = new InputTranslatorConfiguration();
            inputTranslator.Name = "Default Input Translator";
            inputTranslator.Key = DEFAULT;
            inputTranslator.Description = "The default input translator will use the definition of the message to parse the contents of the incoming message.";
            inputTranslator.InputTranslatorType = EntityHeader<InputTranslatorConfiguration.InputTranslatorTypes>.Create(InputTranslatorConfiguration.InputTranslatorTypes.MessageBased);
            this.AddAuditProperties(inputTranslator, createTimestamp, org, user);
            this.AddId(inputTranslator);
            this.AddOwnedProperties(inputTranslator, org);
            await this._pipelineMgr.AddInputTranslatorConfigurationAsync(inputTranslator, org, user);

            return inputTranslator;
        }

        public async Task<OutputTranslatorConfiguration> AddOutputTranslatorsAsync(EntityHeader org, EntityHeader user, DateTime createTimestamp)
        {
            var outputTranslator = new OutputTranslatorConfiguration();
            outputTranslator.Name = "Default Output Translator";
            outputTranslator.Key = DEFAULT;
            outputTranslator.Description = "The default output translator will use the definition of the message to build the contents of the outgoing message.";
            outputTranslator.OutputTranslatorType = EntityHeader<OutputTranslatorConfiguration.OutputTranslatorTypes>.Create(OutputTranslatorConfiguration.OutputTranslatorTypes.MessageBased);
            this.AddAuditProperties(outputTranslator, createTimestamp, org, user);
            this.AddId(outputTranslator);
            this.AddOwnedProperties(outputTranslator, org);

            await this._pipelineMgr.AddOutputTranslatorConfigurationAsync(outputTranslator, org, user);

            return outputTranslator;
        }

        public async Task<DeploymentInstance> CreateInstanceAsync(Subscription subscription, Solution solution, DeviceRepository repo, string name, string key,
            EntityHeader org, EntityHeader user, DateTime createTimestamp)
        {
            if (subscription == null) throw new ArgumentNullException(nameof(subscription));
            if (solution == null) throw new ArgumentNullException(nameof(solution));
            if (repo == null) throw new ArgumentNullException(nameof(repo));

            await _storageUtils.DeleteIfExistsAsync<DeploymentInstance>(key, org);

            var instance = new DeploymentInstance()
            {
                Name = name,
                Key = key,
            };

            this.AddId(instance);
            this.AddAuditProperties(instance, createTimestamp, org, user);
            this.AddOwnedProperties(instance, org);

            var containerRepo = await _storageUtils.FindWithKeyAsync<ContainerRepository>("consoleruntime");
            if (containerRepo == null)
            {
                throw new ArgumentNullException(nameof(containerRepo));
            }

            if (EntityHeader.IsNullOrEmpty(containerRepo.PreferredTag))
            {
                throw new ArgumentNullException(nameof(containerRepo.PreferredTag));
            }

            instance.DeviceRepository = new EntityHeader<DeviceRepository>() { Id = repo.Id, Text = repo.Name };
            instance.Subscription = new EntityHeader() { Id = subscription.Id.ToString(), Text = subscription.Name };
            instance.DeploymentConfiguration = EntityHeader<DeploymentConfigurations>.Create(DeploymentConfigurations.SingleInstance);
            instance.DeploymentType = EntityHeader<DeploymentTypes>.Create(DeploymentTypes.Managed);
            instance.QueueType = EntityHeader<QueueTypes>.Create(QueueTypes.InMemory);
            instance.LogStorage = EntityHeader<LogStorage>.Create(LogStorage.Cloud);
            instance.PrimaryCacheType = EntityHeader<CacheTypes>.Create(CacheTypes.LocalInMemory);
            instance.SharedAccessKey1 = _instanceMgr.GenerateAccessKey();
            instance.SharedAccessKey2 = _instanceMgr.GenerateAccessKey();
            instance.NuvIoTEdition = EntityHeader<NuvIoTEditions>.Create(NuvIoTEditions.Container);
            instance.LogStorage = EntityHeader<LogStorage>.Create(LogStorage.Cloud);
            instance.WorkingStorage = EntityHeader<WorkingStorage>.Create(WorkingStorage.Cloud);
            instance.Solution = new EntityHeader<Solution>() { Id = solution.Id, Text = solution.Name };
            instance.ContainerRepository = new EntityHeader() { Id = containerRepo.Id, Text = containerRepo.Name };
            instance.ContainerTag = containerRepo.PreferredTag;
            instance.Size = EntityHeader.Create("freetrial", "Free");

            await _instanceMgr.AddInstanceAsync(instance, org, user);

            return instance;
        }
    }
}
