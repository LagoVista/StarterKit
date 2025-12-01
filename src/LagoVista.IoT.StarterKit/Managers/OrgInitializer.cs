// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 281cec009a0a8e1195202701902a367fbc376581a79fd7bf8bca1aaacd7c9b09
// IndexVersion: 2
// --- END CODE INDEX META ---
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
using LagoVista.IoT.Simulator.Admin.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.IoT.Simulator.Admin.Models;
using LagoVista.IoT.DeviceManagement.Core;
using LagoVista.IoT.Runtime.Core.Models.Verifiers;
using LagoVista.IoT.Billing.Managers;
using LagoVista.ProjectManagement;

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
        IDeploymentHostManager _hostManager;
        ISolutionManager _solutionMgr;
        IDeviceConfigurationManager _deviceCfgMgr;
        IDeviceRepositoryManager _deviceRepoMgr;
        IProductManager _productManager;
        IVerifierManager _verifierMgr;
        ISimulatorManager _simulatorMgr;
        IDeviceManager _deviceManager;
        IOrganizationRepo _orgRepo;
        IUserManager _userManager;
        ICustomerManager _customerManager;
        IBillingManager _billingManager;
        IToDoManager _todoManager;

        StorageUtils _storageUtils;

        public OrgInitializer(IAdminLogger logger, IStarterKitConnection starterKitConnection, IDeviceAdminManager deviceAdminMgr, ISubscriptionManager subscriptionMgr, IPipelineModuleManager pipelineMgr, IDeviceTypeManager deviceTypeMgr, IDeviceRepositoryManager deviceRepoMgr,
                          IUserManager userManager, IProductManager productManager, IDeviceTypeManager deviceTypeManager, IDeviceConfigurationManager deviceCfgMgr, IDeviceMessageDefinitionManager deviceMsgMgr, IDeploymentInstanceManager instanceMgr,
                          IDeploymentHostManager hostMgr, IDeviceManager deviceManager, IContainerRepositoryManager containerMgr, ISolutionManager solutionMgr, IOrganizationRepo orgMgr, ISimulatorManager simMgr, IVerifierManager verifierMgr, ICustomerManager customerManager, 
                          IBillingManager billingManager, IToDoManager toDoManager)
        {
            _userManager = userManager;
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
            _simulatorMgr = simMgr;
            _orgRepo = orgMgr;
            _deviceManager = deviceManager;
            _hostManager = hostMgr;
            _customerManager = customerManager;
            _billingManager = billingManager;
            _instanceMgr = instanceMgr;
            _solutionMgr = solutionMgr;
            _todoManager = toDoManager;

            _storageUtils = new StorageUtils(new Uri(starterKitConnection.StarterKitStorage.Uri), starterKitConnection.StarterKitStorage.AccessKey,
                starterKitConnection.StarterKitStorage.ResourceName, logger);
            _billingManager = billingManager;
        }

        private const string EXAMPLE_MOTION_KEY = "examplemotion";
        private const string DEFAULT = "default";
        private const string ANONYMOUS = "anonymous";
        private const string TRIAL = "trial";

        public async Task Init(EntityHeader org, EntityHeader user, bool populateSampleData)
        {
            var creationTimeStamp = DateTime.UtcNow;
            await this.AddTrialSubscriptionAsync(org, user, creationTimeStamp);

            await this.AddInputTranslatorAsync(org, user, creationTimeStamp);
            await this.AddOutputTranslatorsAsync(org, user, creationTimeStamp);
            await this.AddAnonymousSentinelAsync(org, user, creationTimeStamp);
            await this.AddDefaultWorkflowAsync(org, user, creationTimeStamp);
            await this.CreateOrgSetupToDoAsync(org, user, creationTimeStamp);
        }

        public async Task<InvokeResult> CreateExampleAppAsync(string environmentName, EntityHeader org, EntityHeader user)
        {
            var instance = await _storageUtils.FindWithKeyAsync<DeploymentInstance>(EXAMPLE_MOTION_KEY, org, false);
            if (instance != null && instance.Status.Value != DeploymentInstanceStates.Offline)
            {
                return InvokeResult.FromError("The example instance is currently running, please go to Studio > Deployments > Instances > Motion Example > Manage and remove the 'Example Motion' instance.", "inuse");
            }

            if (instance != null && !EntityHeader.IsNullOrEmpty(instance.PrimaryHost))
            {
                var host = await _hostManager.GetDeploymentHostAsync(instance.PrimaryHost.Id, org, user, false);
                if (host.Status.Value != HostStatus.Offline)
                {
                    return InvokeResult.FromError("The example instance is currently running, please go to Studio > Deployments > Instances > Motion Example > Manage and remove the 'Example Motion' instance.", "inuse");
                }
            }

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
            var listener = await this.AddPort80Listener("exampleport80", "Port 80 REST Listener", "A simple listener that will listen on port 80 for incoming REST requests.", org, user, creationTimeStamp);

            var deviceConfig = await this.AddDeviceConfigAsync("Example - Motion", EXAMPLE_MOTION_KEY,
                defaultSentinal, defaultInputTranslator, workflow, defaultOutputTranslator,
                msg, org, user, creationTimeStamp);
            var deviceType = await this.AddDeviceType("Example - Motion", EXAMPLE_MOTION_KEY, deviceConfig, org, user, creationTimeStamp);

            var planner = await this.AddPlanner("Example", "exampleplanner", org, user, creationTimeStamp);


            var solution = await this.CreateSolutionAsync("Example - Motion", EXAMPLE_MOTION_KEY, org, user, creationTimeStamp, planner, deviceConfig, listener);

            await _storageUtils.DeleteByKeyIfExistsAsync<DeploymentInstance>(EXAMPLE_MOTION_KEY, org);

            var trialRepo = await this.AddTrialRepository("Example Motion Detector Repository", TRIAL, subscription, org, user, creationTimeStamp);

            var device = await AddDeviceAsync("Motion Sensor 1", "motion001", trialRepo, deviceConfig, deviceType, org, user, creationTimeStamp);

            instance = await this.CreateInstanceAsync(subscription, solution, trialRepo, "Example - Motion", EXAMPLE_MOTION_KEY, environmentName, org, user, creationTimeStamp);

            await CreateSimulator(instance, device, "Example Motion Simulator", EXAMPLE_MOTION_KEY, org, user, creationTimeStamp);


            return InvokeResult.Success;
        }

        public async Task DeleteExample(EntityHeader org, EntityHeader user)
        {
            await _storageUtils.DeleteByKeyIfExistsAsync<DeviceMessageDefinition>(EXAMPLE_MOTION_KEY, org);
            await _storageUtils.DeleteByKeyIfExistsAsync<PlannerConfiguration>(EXAMPLE_MOTION_KEY, org);
            await _storageUtils.DeleteByKeyIfExistsAsync<DeviceType>(EXAMPLE_MOTION_KEY, org);
            await _storageUtils.DeleteByKeyIfExistsAsync<DeviceConfiguration>(EXAMPLE_MOTION_KEY, org);
        }

        private async Task CreateOrgSetupToDoAsync(EntityHeader org, EntityHeader user, DateTime createTimeStamp)
        {
            _todoManager.IsForInitialization = true;

            await _todoManager.AddToDoAsync(new ProjectManagement.Models.ToDo()
            {
                OwnerOrganization = org,
                AssignedByUser = user,
                AssignedToUser = user,
                CreatedBy = user,
                LastUpdatedBy = user,
                DueDate = DateTime.UtcNow.AddDays(3).ToDateOnly(),
                Priority = EntityHeader<ProjectManagement.Models.ToDo_Priority>.Create(ProjectManagement.Models.ToDo_Priority.Normal),
                CreationDate = createTimeStamp.ToJSONString(),
                LastUpdatedDate = createTimeStamp.ToJSONString(),
                Name = "Build your Team.",
                Instructions = "Add your team members and assign them the appropriate roles.",
                WebLink = "/organization/users/manage"
            }, org, user);

            await _todoManager.AddToDoAsync(new ProjectManagement.Models.ToDo()
            {
                OwnerOrganization = org,
                AssignedByUser = user,
                AssignedToUser = user,
                CreatedBy = user,
                LastUpdatedBy = user,
                DueDate = DateTime.UtcNow.AddDays(5).ToDateOnly(),
                Priority = EntityHeader<ProjectManagement.Models.ToDo_Priority>.Create(ProjectManagement.Models.ToDo_Priority.Normal),
                CreationDate = createTimeStamp.ToJSONString(),
                LastUpdatedDate = createTimeStamp.ToJSONString(),
                Name = "Watch Getting started Video",
                Instructions = "To better understand the concepts and how to build IoT applications with NuvIoT, watch our getting started video.",
                WebLink = "/home/welcome"
            }, org, user);


            await _todoManager.AddToDoAsync(new ProjectManagement.Models.ToDo()
            {
                OwnerOrganization = org,
                AssignedByUser = user,
                AssignedToUser = user,
                CreatedBy = user,
                DueDate = DateTime.UtcNow.AddDays(7).ToDateOnly(),
                Priority = EntityHeader<ProjectManagement.Models.ToDo_Priority>.Create(ProjectManagement.Models.ToDo_Priority.Normal),
                LastUpdatedBy = user,
                CreationDate = createTimeStamp.ToJSONString(),
                LastUpdatedDate = createTimeStamp.ToJSONString(),
                Name = "Create your First IoT Application.",
                Instructions = "User our Quick Start Template to create your first IoT application with NuvIoT.",
                WebLink = "/project/fromtemplate"
            }, org, user);

            _todoManager.IsForInitialization = false;
        }

        public async Task<SubscriptionDTO> AddTrialSubscriptionAsync(EntityHeader org, EntityHeader user, DateTime createTimeStamp)
        {
            var subscription = new SubscriptionDTO()
            {
                Id = Guid.NewGuid(),
                OrgId = org.Id,
                Name = "Trial Subscription",
                Key = SubscriptionDTO.SubscriptionKey_Trial,
                Status = SubscriptionDTO.Status_OK,
                CreatedById = user.Id,
                LastUpdatedById = user.Id,
                CreationDate = createTimeStamp,
                LastUpdatedDate = createTimeStamp,                
            };


            _subscriptionMgr.IsForInitialization = true;
            await this._subscriptionMgr.AddSubscriptionAsync(subscription, org, user);
            _subscriptionMgr.IsForInitialization = false;

            return subscription;
        }

        public async Task<DeviceMessageDefinition> AddDefaultMessage(string name, string key, EntityHeader org, EntityHeader user, DateTime createTimeStamp)
        {
            await _storageUtils.DeleteByKeyIfExistsAsync<DeviceMessageDefinition>(key, org);
            await _storageUtils.DeleteByKeyIfExistsAsync<Verifier>("exmplmotgparser", org);

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

            msgDefinition.SampleMessages.Add(new SampleMessage()
            {
                Id = Guid.NewGuid().ToId(),
                Name = "Sample Motion Message",
                Key = "smplmotionmsg",
                Payload = "{'motion':'on'}",
                PathAndQueryString = "/api/motion/dev001",
                Description = "Sample message that would be sent by a motion detector.",
                Topic = String.Empty,
            });

            AddOwnedProperties(msgDefinition, org);
            AddAuditProperties(msgDefinition, createTimeStamp, org, user);

            await _deviceMsgMgr.AddDeviceMessageDefinitionAsync(msgDefinition, org, user);

            await _storageUtils.DeleteByKeyIfExistsAsync<Verifier>("examplemotionmsgparser", org);

            var verifier = new Verifier()
            {
                Component = new EntityHeader()
                {
                    Id = msgDefinition.Id,
                    Text = msgDefinition.Name
                },
                ExpectedOutputs = new System.Collections.ObjectModel.ObservableCollection<ExpectedValue>()
                {
                    new ExpectedValue() { Key = "motion", Value="on"},
                },
                InputType = EntityHeader<InputTypes>.Create(InputTypes.Text),
                Name = "Simple Message Verifier",
                Key = "exmplmotgparser",
                Description = "Validates that a Sample Motion Message has the proper field parsers",
                Input = "{'motion':'on'}",
                PathAndQueryString = "/smplmot001/device001",
                ShouldSucceed = true,
                VerifierType = EntityHeader<VerifierTypes>.Create(VerifierTypes.MessageParser)
            };
            AddId(verifier);
            AddOwnedProperties(verifier, org);
            AddAuditProperties(verifier, createTimeStamp, org, user);
            await _verifierMgr.AddVerifierAsync(verifier, org, user);

            return msgDefinition;
        }

        public async Task<DeviceConfiguration> AddDeviceConfigAsync(string name, string key, SentinelConfiguration sentinal,
            InputTranslatorConfiguration input, DeviceWorkflow workflow, OutputTranslatorConfiguration output,
            DeviceMessageDefinition msg, EntityHeader org, EntityHeader user, DateTime createTimestamp)
        {
            if (sentinal == null) throw new ArgumentNullException(nameof(sentinal));
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (workflow == null) throw new ArgumentNullException(nameof(workflow));
            if (output == null) throw new ArgumentNullException(nameof(output));
            if (msg == null) throw new ArgumentNullException(nameof(msg));

            await _storageUtils.DeleteByKeyIfExistsAsync<DeviceConfiguration>(key, org);

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
            route.MessageDefinition = new EntityHeader<DeviceMessageDefinition>()
            {
                Id = msg.Id,
                Text = msg.Name
            };

            var sentinalConfig = route.PipelineModules.Where(pm => pm.ModuleType.Value == PipelineModuleType.Sentinel).First();
            sentinalConfig.Name = sentinal.Name;
            sentinalConfig.Key = sentinal.Key;
            sentinalConfig.Module = new EntityHeader<DeviceAdmin.Interfaces.IPipelineModuleConfiguration>()
            {
                Id = sentinal.Id,
                Text = sentinal.Name
            };

            var inputTranslator = route.PipelineModules.Where(pm => pm.ModuleType.Value == PipelineModuleType.InputTranslator).First();
            inputTranslator.Name = input.Name;
            inputTranslator.Key = input.Key;
            inputTranslator.Module = new EntityHeader<DeviceAdmin.Interfaces.IPipelineModuleConfiguration>()
            {
                Id = input.Id,
                Text = input.Name
            };

            inputTranslator.PrimaryOutput.Mappings.Add(new KeyValuePair<string, object>("motion", "motionstatus"));

            var wf = route.PipelineModules.Where(pm => pm.ModuleType.Value == PipelineModuleType.Workflow).First();
            wf.Key = workflow.Key;
            wf.Name = workflow.Name;
            wf.Module = new EntityHeader<DeviceAdmin.Interfaces.IPipelineModuleConfiguration>()
            {
                Id = workflow.Id,
                Text = workflow.Name
            };

            var outputTranslator = route.PipelineModules.Where(pm => pm.ModuleType.Value == PipelineModuleType.OutputTranslator).First();
            outputTranslator.Name = output.Name;
            outputTranslator.Key = output.Key;
            outputTranslator.Module = new EntityHeader<DeviceAdmin.Interfaces.IPipelineModuleConfiguration>()
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
            await _storageUtils.DeleteByKeyIfExistsAsync<Solution>(key, org);

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


            var setting = new CustomField
            {
                DefaultValue = "32",
                FieldType = EntityHeader<ParameterTypes>.Create(ParameterTypes.Integer),
                Label = "Example Settung 1",
                Key = "example1",
                Name = "Example Setting 1",
                HelpText = "You can create custom settings for your solution you can access from our API or from the scripting environment."
            };

            solution.Settings.Add(setting);

            var setting2 = new CustomField
            {
                DefaultValue = "32",
                FieldType = EntityHeader<ParameterTypes>.Create(ParameterTypes.Integer),
                Label = "Example Setting 2",
                Key = "example2",
                Name = "Example Setting 2",
                HelpText = "A second optional setting for your solution."
            };

            solution.Settings.Add(setting2);

            await _solutionMgr.AddSolutionsAsync(solution, org, user);

            return solution;
        }

        public async Task<DeviceType> AddDeviceType(string name, string key, DeviceConfiguration deviceConfg, EntityHeader org, EntityHeader user, DateTime createTimestamp)
        {
            await _storageUtils.DeleteByKeyIfExistsAsync<DeviceType>(key, org);

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
            await _storageUtils.DeleteByKeyIfExistsAsync<DeviceWorkflow>(key, org);

            var appUser = await _userManager.FindByIdAsync(user.Id);

            var wf = new DeviceWorkflow()
            {
                Id = Guid.NewGuid().ToId(),
                Name = name,
                Key = key,
            };

            wf.Pages.Add(new DiagramPage()
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

            attr.IncomingConnections.Add(new Connection()
            {
                NodeKey = "motionstatus",
                NodeName = "Motion Status",
                NodeType = "workflowinput"
            });

            attr.OnSetScript = @"/* 
 * Provide a script to customize setting an attribute:
 *
 * The default setter method for the Motion Status attribute is:
 * 
 *     Attributes.motionstatus = value;
 *
 * If you do not provide a script, the attribute will be
 * set automatically, if a script is present, you will be
 * responsible for taking the value as an input parameter
 * and making the assignment.  
 * 
 * You can add conditional logic so that the attribute will
 * not get set.
 */
function onSet(value /* String */) {    
    let phoneNumber = '" + appUser.PhoneNumber + @"';
    Attributes.motionstatus = value;
    if(value === 'True'){
        sendSMS(phoneNumber, 'Motion detected on ' + IoTDevice.name);
    } 
    else {
        sendSMS(phoneNumber, 'Motion cleared on ' + IoTDevice.name);
    }
};";

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
            input.OutgoingConnections.Add(new Connection()
            {
                NodeKey = "motionstatus",
                NodeName = "Motion Status",
                NodeType = "attribute"
            });

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
            await _storageUtils.DeleteByKeyIfExistsAsync<PlannerConfiguration>(key, org);
            await _storageUtils.DeleteByKeyIfExistsAsync<Verifier>("explmotmsgidinpath", org);
            await _storageUtils.DeleteByKeyIfExistsAsync<Verifier>("explmotdeveidheader", org);


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
                PathLocator = "/api/*/{deviceidinpath}"
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

            var deviceIdParserVerifier1 = new Verifier()
            {
                Component = new EntityHeader()
                {
                    Id = planner.DeviceIdParsers.First().Id,
                    Text = planner.DeviceIdParsers.First().Name
                },
                ExpectedOutput = "dev001",
                InputType = EntityHeader<InputTypes>.Create(InputTypes.Text),
                Name = "Find Device Id in Header",
                Key = "explmotdeveidheader",
                Description = "Validates that the Device Id can be extracted from the path",
                PathAndQueryString = "/api/smplmsg001/dev001",
                ShouldSucceed = true,
                VerifierType = EntityHeader<VerifierTypes>.Create(VerifierTypes.MessageFieldParser)
            };
            AddId(deviceIdParserVerifier1);
            AddOwnedProperties(deviceIdParserVerifier1, org);
            AddAuditProperties(deviceIdParserVerifier1, createTimestamp, org, user);
            await _verifierMgr.AddVerifierAsync(deviceIdParserVerifier1, org, user);

            var messageIdParserVerifier1 = new Verifier()
            {
                Component = new EntityHeader()
                {
                    Id = planner.MessageTypeIdParsers.First().Id,
                    Text = planner.MessageTypeIdParsers.First().Name
                },
                ExpectedOutput = "smplmsg001",
                InputType = EntityHeader<InputTypes>.Create(InputTypes.Text),
                Name = "Finds Message id in Header",
                Key = "explmotmsgidinpath",
                Description = "Validates that the message id can be extracted from the path",
                PathAndQueryString = "/api/smplmsg001/dev001",
                ShouldSucceed = true,
                VerifierType = EntityHeader<VerifierTypes>.Create(VerifierTypes.MessageFieldParser)
            };
            AddId(messageIdParserVerifier1);
            AddOwnedProperties(messageIdParserVerifier1, org);
            AddAuditProperties(messageIdParserVerifier1, createTimestamp, org, user);


            await _verifierMgr.AddVerifierAsync(messageIdParserVerifier1, org, user);

            return planner;
        }

        public async Task<DeviceRepository> AddTrialRepository(string name, string key, SubscriptionDTO subscription, EntityHeader org, EntityHeader user, DateTime createTimestamp)
        {
            await _storageUtils.DeleteByKeyIfExistsAsync<DeviceRepository>(key, org);

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

            _pipelineMgr.IsForInitialization = true;
            await this._deviceRepoMgr.AddDeviceRepositoryAsync(repo, org, user);
            _pipelineMgr.IsForInitialization = false;

            /* when it gets created all the "stuff" to access the repo won't be present, load it to get those values. */
            repo = await this._deviceRepoMgr.GetDeviceRepositoryWithSecretsAsync(repo.Id, org, user);

            return repo;
        }

        public async Task<ListenerConfiguration> AddPort80Listener(string key, string name, string description, EntityHeader org, EntityHeader user, DateTime createTimestamp)
        {
            await _storageUtils.DeleteByKeyIfExistsAsync<ListenerConfiguration>(key, org);

            var port80Listener = new ListenerConfiguration();
            port80Listener.Name = name;
            port80Listener.Key = key;
            port80Listener.Description = description;
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

            _pipelineMgr.IsForInitialization = true;
            await this._pipelineMgr.AddListenerConfigurationAsync(port80Listener, org, user);
            _pipelineMgr.IsForInitialization = false;

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

            _pipelineMgr.IsForInitialization = true;
            await this._pipelineMgr.AddSentinelConfigurationAsync(sentinal, org, user);
            _pipelineMgr.IsForInitialization = false;

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

            _pipelineMgr.IsForInitialization = true;
            await this._pipelineMgr.AddInputTranslatorConfigurationAsync(inputTranslator, org, user);
            _pipelineMgr.IsForInitialization = false;

            return inputTranslator;
        }

        public async Task<DeviceWorkflow> AddDefaultWorkflowAsync(EntityHeader org, EntityHeader user, DateTime createTimestamp)
        {
            var wf = new DeviceWorkflow();
            wf.Pages.Add(new DiagramPage()
            {
                PageNumber = 1,
                Name = DeviceLibraryResources.Common_PageNumberOne
            });

            wf.Name = "Default Workflow (empty)";
            wf.Key = DEFAULT;
            wf.Description = "The default empty workflow that can be customized as necessary.";
            this.AddAuditProperties(wf, createTimestamp, org, user);
            this.AddId(wf);
            this.AddOwnedProperties(wf, org);

            _deviceAdminMgr.IsForInitialization = true;
            await _deviceAdminMgr.AddDeviceWorkflowAsync(wf, org, user);
            _deviceAdminMgr.IsForInitialization = false;

            return wf;
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

            _pipelineMgr.IsForInitialization = true;
            await this._pipelineMgr.AddOutputTranslatorConfigurationAsync(outputTranslator, org, user);
            _pipelineMgr.IsForInitialization = false;

            return outputTranslator;
        }

        public async Task<DeploymentInstance> CreateInstanceAsync(SubscriptionDTO subscription, Solution solution, DeviceRepository repo, string name, string key,
            string environmentName, EntityHeader org, EntityHeader user, DateTime createTimestamp)
        {
            if (subscription == null) throw new ArgumentNullException(nameof(subscription));
            if (solution == null) throw new ArgumentNullException(nameof(solution));
            if (repo == null) throw new ArgumentNullException(nameof(repo));

            var userOrg = await _orgRepo.GetOrganizationAsync(org.Id);

            await _storageUtils.DeleteByKeyIfExistsAsync<DeploymentInstance>(key, org);
            await _storageUtils.DeleteByKeyIfExistsAsync<DeploymentHost>(key, org);

            var instance = new DeploymentInstance()
            {
                Name = name,
                Key = key,
            };

            var freeVMInstance = await _productManager.GetProductByKeyAsync("vms", "freetrial", org, user);
            if (freeVMInstance == null)
            {
                throw new ArgumentNullException(nameof(freeVMInstance));
            }

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
            instance.IsDeployed = false;
            instance.Solution = new EntityHeader<Solution>() { Id = solution.Id, Text = solution.Name };
            instance.ContainerRepository = new EntityHeader() { Id = containerRepo.Id, Text = containerRepo.Name };
            instance.ContainerTag = containerRepo.PreferredTag;
            instance.Size = EntityHeader.Create(freeVMInstance.Id.ToString(), freeVMInstance.Name);

            var topLevel = key;

            instance.DnsHostName = environmentName == "production" ? $"{topLevel}.{userOrg.Namespace}.iothost.net" : $"{topLevel}.{userOrg.Namespace}.{environmentName}.iothost.net";

            await _instanceMgr.AddInstanceAsync(instance, org, user);

            return instance;
        }

        public string btoa(byte[] buffer)
        {
            string toReturn = System.Convert.ToBase64String(buffer);
            return toReturn;
        }

        public async Task<Device> AddDeviceAsync(string name, string deviceId, DeviceRepository repo, DeviceConfiguration deviceConfig, DeviceType deviceType,
            EntityHeader org, EntityHeader user, DateTime createTimestamp)
        {
            if (deviceType == null) throw new ArgumentNullException(nameof(deviceType));
            if (deviceConfig == null) throw new ArgumentNullException(nameof(deviceConfig));
            if (repo == null) throw new ArgumentNullException(nameof(repo));


            var device = new Device();
            device.Name = name;
            device.DeviceId = deviceId;
            device.DeviceRepository = new EntityHeader() { Id = repo.Id, Text = repo.Name };
            device.DeviceType = new EntityHeader<DeviceType> { Id = deviceType.Id, Text = deviceType.Name };
            device.DeviceConfiguration = new EntityHeader() { Id = deviceConfig.Id, Text = deviceConfig.Name };
            device.SerialNumber = "SN0001";
            var rnd = new Random();
            var buffer = new byte[64];
            rnd.NextBytes(buffer);
            device.PrimaryAccessKey = btoa(buffer);
            rnd.NextBytes(buffer);
            device.SecondaryAccessKey = btoa(buffer);

            AddId(device);
            AddAuditProperties(device, createTimestamp, org, user);
            AddOwnedProperties(device, org);

            await _deviceManager.AddDeviceAsync(repo, device, false, org, user);

            return device;
        }

        public async Task<LagoVista.IoT.Simulator.Admin.Models.Simulator> CreateSimulator(DeploymentInstance instance, Device device, string name, string key,
            EntityHeader org, EntityHeader user, DateTime createTimestamp)
        {
            await _storageUtils.DeleteByKeyIfExistsAsync<LagoVista.IoT.Simulator.Admin.Models.Simulator>(key, org);

            var sim = new LagoVista.IoT.Simulator.Admin.Models.Simulator()
            {
                Name = name,
                Key = key,
            };

            AddId(sim);
            AddOwnedProperties(sim, org);
            AddAuditProperties(sim, createTimestamp, org, user);

            sim.DefaultEndPoint = instance.DnsHostName;
            sim.DefaultPort = 80;
            sim.Anonymous = true;
            sim.BasicAuth = false;
            sim.DefaultPayloadType = EntityHeader<PaylodTypes>.Create(PaylodTypes.String);
            sim.DefaultTransport = EntityHeader<Simulator.Admin.Models.TransportTypes>.Create(Simulator.Admin.Models.TransportTypes.RestHttp);
            sim.DeviceId = device.DeviceId;
            sim.Description = "This simulator is created as part of the Motion Tutorial, it will demonstrate how to send two messages, one that indicates motion has been seen and one that motion is no longer active.";

            var motionMessage = new MessageTemplate()
            {
                ContentType = "application/json",
                HttpVerb = "POST",
                PayloadType = EntityHeader<PaylodTypes>.Create(PaylodTypes.String),
                Name = "Motion Detected",
                Key = "motiondetected",
                Id = Guid.NewGuid().ToId(),
                TextPayload = "{'motion':true}",
                PathAndQueryString = "/api/motion/~deviceid~",
                EndPoint = sim.DefaultEndPoint,
                Transport = EntityHeader<TransportTypes>.Create(TransportTypes.RestHttp),
            };

            var motionClearedMessage = new MessageTemplate()
            {
                ContentType = "application/json",
                HttpVerb = "POST",
                PayloadType = EntityHeader<PaylodTypes>.Create(PaylodTypes.String),
                Name = "Motion Cleared",
                Key = "motioncleared",
                Id = Guid.NewGuid().ToId(),
                TextPayload = "{'motion':false}",
                EndPoint = sim.DefaultEndPoint,
                PathAndQueryString = "/api/motion/~deviceid~",
                Transport = EntityHeader<TransportTypes>.Create(TransportTypes.RestHttp),
            };

            sim.MessageTemplates.Add(motionMessage);
            sim.MessageTemplates.Add(motionClearedMessage);

            await this._simulatorMgr.AddSimulatorAsync(sim, org, user);

            return sim;
        }
    }
}
