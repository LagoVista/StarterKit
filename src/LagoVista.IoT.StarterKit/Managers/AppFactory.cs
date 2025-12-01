// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 6b50510f41a92b9ddd0bb55a6a6cab971217cbef7c7427eb9e831e0efc463708
// IndexVersion: 2
// --- END CODE INDEX META ---
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
using LagoVista.IoT.Deployment.Admin.Models;
using LagoVista.IoT.DeviceManagement.Core.Models;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.IoT.DeviceManagement.Core.Managers;
using LagoVista.IoT.Billing;
using LagoVista.IoT.Verifiers.Managers;
using LagoVista.IoT.Runtime.Core.Models.Verifiers;
using LagoVista.IoT.DeviceMessaging.Admin.Models;

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
        IDeviceRepositoryManager _deviceRepoMgr;
        IProductManager _productManager;
        IVerifierManager _verifierMgr;

        public AppFactory(IDeviceAdminManager deviceAdminMgr, ISubscriptionManager subscriptionMgr, IPipelineModuleManager pipelineMgr, IDeviceTypeManager deviceTypeMgr, IDeviceRepositoryManager deviceRepoMgr,
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

        public SubscriptionDTO CreateFreeSubscription(EntityHeader org, EntityHeader user, DateTime createTimeStamp)
        {
            var subscription = new SubscriptionDTO()
            {
                Id = Guid.NewGuid(),
                OrgId = org.Id,
                Name = "Free Subscription",
                Key = "freesubscription",
                Status = SubscriptionDTO.Status_FreeAccount,
                CreatedById = user.Id,
                CreationDate = createTimeStamp,
                LastUpdatedById = user.Id,
                LastUpdatedDate = createTimeStamp,
            };

            return subscription;
        }

        public DeviceRepository CreateDevice(SubscriptionDTO subscription, EntityHeader org, EntityHeader user, DateTime createTimeStamp)
        {
            /* Create Device Configurations */
            var deviceRepository = new DeviceRepository()
            {
                Name = "Sample Device Repo",
                Key = "sampledevicerepo",
                RepositoryType = EntityHeader<RepositoryTypes>.Create(RepositoryTypes.NuvIoT),
                Subscription = new EntityHeader() { Id = subscription.Id.ToString(), Text = subscription.Name }
            };
            AddId(deviceRepository);
            AddOwnedProperties(deviceRepository, org);
            AddAuditProperties(deviceRepository, createTimeStamp, org, user);

            return deviceRepository;
        }


        public async Task<InvokeResult<Solution>> CreateSimpleSolutionAsync(EntityHeader org, EntityHeader user, DateTime createTimeStamp)
        {
            /* Create unit/state sets */
            var stateSet = new StateSet()
            {
                Key = "onoff",
                Name = "On/Off",
                Description = "Provides two simple states, On and Off",
                States = new List<State> {
                    new State() { Key = "on", Name = "On", IsInitialState = true },
                    new State() { Key = "off", Name = "Off" }
                }
            };
            AddId(stateSet);
            AddOwnedProperties(stateSet, org);
            AddAuditProperties(stateSet, createTimeStamp, org, user);
            await _deviceAdminMgr.AddStateSetAsync(stateSet, org, user);

            var unitSet = new UnitSet()
            {
                Key = "Temperature",
                Name = "temperature",
                Units = new List<Unit>()
                {
                    new Unit() {Key="fahrenheit", Name = "Fahrenheit", IsDefault=true},
                    new Unit() {Key="celsius", Name = "Celsius", IsDefault=false},
                }
            };
            AddId(unitSet);
            AddOwnedProperties(unitSet, org);
            AddAuditProperties(unitSet, createTimeStamp, org, user);
            await _deviceAdminMgr.AddUnitSetAsync(unitSet, org, user);

            /* Create Pipeline Modules */
            var restListener = new ListenerConfiguration()
            {
                Name = "Sample Rest",
                Key = "samplereset",
                ListenerType = EntityHeader<ListenerTypes>.Create(ListenerTypes.Rest),
                RestServerType = EntityHeader<RESTServerTypes>.Create(RESTServerTypes.HTTP),
                ListenOnPort = 80,
                ContentType = EntityHeader<DeviceMessaging.Admin.Models.MessageContentTypes>.Create(DeviceMessaging.Admin.Models.MessageContentTypes.JSON),
                UserName = "clientuser",
                Password = "Test1234"
            };
            AddId(restListener);
            AddOwnedProperties(restListener, org);
            AddAuditProperties(restListener, createTimeStamp, org, user);
            await _pipelineMgr.AddListenerConfigurationAsync(restListener, org, user);

            var msgTempDefinition = new DeviceMessaging.Admin.Models.DeviceMessageDefinition()
            {
                Name = "Sample Temperature Message",
                Key = "sampletempmsg",
                Delimiter = ",",
                SampleMessages = new List<DeviceMessaging.Admin.Models.SampleMessage>()
                {
                    new DeviceMessaging.Admin.Models.SampleMessage()
                    {
                        Id = Guid.NewGuid().ToId(),
                        Name = "Example Temperature Message",
                        Key="exmpl001",
                        Payload="98.5,38",
                        Headers = new System.Collections.ObjectModel.ObservableCollection<SampleMessageHeader>()
                        {
                            new SampleMessageHeader() { Name="x-deviceid", Value="device001"},
                            new SampleMessageHeader() { Name = "x-messageid", Value="tmpmsg001"}
                        }
                    }
                },
                MessageId = "smpltmp001",
                ContentType = EntityHeader<DeviceMessaging.Admin.Models.MessageContentTypes>.Create(DeviceMessaging.Admin.Models.MessageContentTypes.Delimited),
                Fields = new List<DeviceMessaging.Admin.Models.DeviceMessageDefinitionField>()
                {
                  new DeviceMessaging.Admin.Models.DeviceMessageDefinitionField()
                    {
                        Id = Guid.NewGuid().ToId(),
                        Name = "Temperature",
                        Key = "temp",
                        DelimitedIndex = 0,
                        UnitSet = new EntityHeader<UnitSet>() {Id = unitSet.Id, Text = unitSet.Name, Value = unitSet},
                        StorageType =  EntityHeader<ParameterTypes>.Create(ParameterTypes.State),
                    },
                    new DeviceMessaging.Admin.Models.DeviceMessageDefinitionField()
                    {
                        Id = Guid.NewGuid().ToId(),
                        Name = "Humidity",
                        Key = "humidity",
                        DelimitedIndex = 1,
                        StorageType =  EntityHeader<ParameterTypes>.Create(ParameterTypes.Integer),
                    }
                }
            };
            AddId(msgTempDefinition);
            AddOwnedProperties(msgTempDefinition, org);
            AddAuditProperties(msgTempDefinition, createTimeStamp, org, user);
            await _deviceMsgMgr.AddDeviceMessageDefinitionAsync(msgTempDefinition, org, user);

            var motionMsgDefinition = new DeviceMessaging.Admin.Models.DeviceMessageDefinition()
            {
                Name = "Sample Motion Message",
                Key = "samplemptionmsg",
                SampleMessages = new List<DeviceMessaging.Admin.Models.SampleMessage>()
                {
                    new DeviceMessaging.Admin.Models.SampleMessage()
                    {
                        Id = Guid.NewGuid().ToId(),
                        Name = "Example Motion Message",
                        Key="exmpl001",
                        Payload="{'motion':'on','level':80}",
                        PathAndQueryString = "/motion/device001"
                    }
                },
                MessageId = "smplmot001",
                ContentType = EntityHeader<DeviceMessaging.Admin.Models.MessageContentTypes>.Create(DeviceMessaging.Admin.Models.MessageContentTypes.JSON),
                Fields = new List<DeviceMessaging.Admin.Models.DeviceMessageDefinitionField>()
                {
                    new DeviceMessaging.Admin.Models.DeviceMessageDefinitionField()
                    {
                        Id = Guid.NewGuid().ToId(),
                        Name = "Motion",
                        Key = "motion",
                        JsonPath = "motion",
                        StateSet = new EntityHeader<StateSet>() {Id = stateSet.Id, Text = stateSet.Name, Value = stateSet},
                        StorageType =  EntityHeader<ParameterTypes>.Create(ParameterTypes.State),
                    },
                    new DeviceMessaging.Admin.Models.DeviceMessageDefinitionField()
                    {
                        Id = Guid.NewGuid().ToId(),
                        Name = "Level",
                        Key = "level",
                        JsonPath = "level",
                        StorageType =  EntityHeader<ParameterTypes>.Create(ParameterTypes.Integer),
                    }
                }
            };
            AddId(motionMsgDefinition);
            AddOwnedProperties(motionMsgDefinition, org);
            AddAuditProperties(motionMsgDefinition, createTimeStamp, org, user);
            await _deviceMsgMgr.AddDeviceMessageDefinitionAsync(motionMsgDefinition, org, user);


            var deviceIdParser1 = new DeviceMessaging.Admin.Models.DeviceMessageDefinitionField()
            {
                SearchLocation = EntityHeader<DeviceMessaging.Admin.Models.SearchLocations>.Create(DeviceMessaging.Admin.Models.SearchLocations.Header),
                HeaderName = "x-deviceid",
                Name = "Device Id in Header",
                Key = "xdeviceid"
            };

            var deviceIdParser2 = new DeviceMessaging.Admin.Models.DeviceMessageDefinitionField()
            {
                SearchLocation = EntityHeader<DeviceMessaging.Admin.Models.SearchLocations>.Create(DeviceMessaging.Admin.Models.SearchLocations.Path),
                PathLocator = "/*/{deviceid}",
                Name = "Device Id in Path",
                Key = "pathdeviceid"
            };

            var messageIdParser1 = new DeviceMessaging.Admin.Models.DeviceMessageDefinitionField()
            {
                SearchLocation = EntityHeader<DeviceMessaging.Admin.Models.SearchLocations>.Create(DeviceMessaging.Admin.Models.SearchLocations.Header),
                HeaderName = "x-messageid",
                Name = "Message Id in Header",
                Key = "xmessageid",
            };
            var messageIdParser2 = new DeviceMessaging.Admin.Models.DeviceMessageDefinitionField()
            {
                SearchLocation = EntityHeader<DeviceMessaging.Admin.Models.SearchLocations>.Create(DeviceMessaging.Admin.Models.SearchLocations.Path),
                PathLocator = "/{messageid}/*",
                Name = "Message Id in Path",
                Key = "pathmessageid"
            };

            var verifier = new Verifier()
            {
                Component = new EntityHeader()
                {
                    Id = motionMsgDefinition.Id,
                    Text = motionMsgDefinition.Name
                },
                ExpectedOutputs = new System.Collections.ObjectModel.ObservableCollection<ExpectedValue>()
                {
                    new ExpectedValue() { Key = "motion", Value="on"},
                    new ExpectedValue() {Key = "level", Value="80"}
                },
                InputType = EntityHeader<InputTypes>.Create(InputTypes.Text),
                Name = "Simple Message Verifier",
                Key = "smplmsgver",
                Description = "Validates that a Sample Motion Message has the proper field parsers",
                Input = "{'motion':'on','level':80}",
                PathAndQueryString = "/smplmot001/device001",
                ShouldSucceed = true,
                VerifierType = EntityHeader<VerifierTypes>.Create(VerifierTypes.MessageParser)
            };
            AddId(verifier);
            AddOwnedProperties(verifier, org);
            AddAuditProperties(verifier, createTimeStamp, org, user);
            await _verifierMgr.AddVerifierAsync(verifier, org, user);

            var deviceIdParserVerifier1 = new Verifier()
            {
                Component = new EntityHeader()
                {
                    Id = deviceIdParser1.Id,
                    Text = deviceIdParser1.Name
                },
                ExpectedOutput = "device001",
                InputType = EntityHeader<InputTypes>.Create(InputTypes.Text),
                Name = "Find Device Id in Header",
                Key = "msgidheader",
                Description = "Validates that the Device Id can be extracted from the header",
                Headers = new System.Collections.ObjectModel.ObservableCollection<SampleMessageHeader>()
                {
                     new SampleMessageHeader() { Name = "x-messageid", Value="device001"}
                },
                ShouldSucceed = true,
                VerifierType = EntityHeader<VerifierTypes>.Create(VerifierTypes.MessageFieldParser)
            };
            AddId(deviceIdParserVerifier1);
            AddOwnedProperties(deviceIdParserVerifier1, org);
            AddAuditProperties(deviceIdParserVerifier1, createTimeStamp, org, user);
            await _verifierMgr.AddVerifierAsync(deviceIdParserVerifier1, org, user);

            var deviceIdParserVerifier2 = new Verifier()
            {
                Component = new EntityHeader()
                {
                    Id = deviceIdParser2.Id,
                    Text = deviceIdParser2.Name
                },
                ExpectedOutput = "device002",
                InputType = EntityHeader<InputTypes>.Create(InputTypes.Text),
                Name = "Finds Device Id in Path",
                Key = "msgidpath",
                Description = "Validats the the device id can be extracted from thepath",
                PathAndQueryString = "/smplmot001/device002",
                ShouldSucceed = true,
                VerifierType = EntityHeader<VerifierTypes>.Create(VerifierTypes.MessageFieldParser)
            };
            AddId(deviceIdParserVerifier2);
            AddOwnedProperties(deviceIdParserVerifier2, org);
            AddAuditProperties(deviceIdParserVerifier2, createTimeStamp, org, user);
            await _verifierMgr.AddVerifierAsync(deviceIdParserVerifier2, org, user);

            var messageIdParserVerifier1 = new Verifier()
            {
                Component = new EntityHeader()
                {
                    Id = messageIdParser1.Id,
                    Text = messageIdParser1.Name
                },
                ExpectedOutput = "smplmsg001",
                InputType = EntityHeader<InputTypes>.Create(InputTypes.Text),
                Name = "Finds Message id in Header",
                Key = "msgidheader",
                Description = "Validates that the message id can be extracted from the header",
                Headers = new System.Collections.ObjectModel.ObservableCollection<SampleMessageHeader>()
                {
                     new SampleMessageHeader() { Name = "x-messageid", Value="smplmsg001"}
                },
                ShouldSucceed = true,
                VerifierType = EntityHeader<VerifierTypes>.Create(VerifierTypes.MessageFieldParser)
            };
            AddId(messageIdParserVerifier1);
            AddOwnedProperties(messageIdParserVerifier1, org);
            AddAuditProperties(messageIdParserVerifier1, createTimeStamp, org, user);
            await _verifierMgr.AddVerifierAsync(messageIdParserVerifier1, org, user);

            var messageIdParserVerifier2 = new Verifier()
            {
                Component = new EntityHeader()
                {
                    Id = messageIdParser2.Id,
                    Text = messageIdParser2.Name
                },
                ExpectedOutput = "smplmot002",
                InputType = EntityHeader<InputTypes>.Create(InputTypes.Text),
                Name = "Finds Message id in Path",
                Key = "msgidpath",
                Description = "Validates that the message id can be extracted from path.",
                PathAndQueryString = "/smplmot002/device001",
                ShouldSucceed = true,
                VerifierType = EntityHeader<VerifierTypes>.Create(VerifierTypes.MessageFieldParser)
            };
            AddId(messageIdParserVerifier2);
            AddOwnedProperties(messageIdParserVerifier2, org);
            AddAuditProperties(messageIdParserVerifier2, createTimeStamp, org, user);
            await _verifierMgr.AddVerifierAsync(messageIdParserVerifier2, org, user);

            var planner = new PlannerConfiguration()
            {
                Name = "Sample Planner",
                Key = "sampleplanner",
                MessageTypeIdParsers = new List<DeviceMessaging.Admin.Models.MessageAttributeParser>()
                {messageIdParser1, messageIdParser2},
                DeviceIdParsers = new List<DeviceMessaging.Admin.Models.MessageAttributeParser>()
                {deviceIdParser1, deviceIdParser2}
            };
            AddId(planner);
            AddOwnedProperties(planner, org);
            AddAuditProperties(planner, createTimeStamp, org, user);
            await _pipelineMgr.AddPlannerConfigurationAsync(planner, org, user);


            /* Create Pipeline Modules */
            var sentinelConfiguration = new SentinelConfiguration()
            {
                Name = "Sample Sentinel",
                Key = "samplereset",
            };
            AddId(sentinelConfiguration);
            AddOwnedProperties(sentinelConfiguration, org);
            AddAuditProperties(sentinelConfiguration, createTimeStamp, org, user);
            await _pipelineMgr.AddSentinelConfigurationAsync(sentinelConfiguration, org, user);

            var inputTranslator = new InputTranslatorConfiguration()
            {
                Name = "Sample Input Translator",
                Key = "sampleinputtranslator",
                InputTranslatorType = EntityHeader<InputTranslatorConfiguration.InputTranslatorTypes>.Create(InputTranslatorConfiguration.InputTranslatorTypes.MessageBased),
            };
            AddId(inputTranslator);
            AddOwnedProperties(inputTranslator, org);
            AddAuditProperties(inputTranslator, createTimeStamp, org, user);
            await _pipelineMgr.AddInputTranslatorConfigurationAsync(inputTranslator, org, user);

            var wf = new DeviceWorkflow()
            {
                Key = "sampleworkflow",
                Name = "Sample Workflow",
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

            var outTranslator = new OutputTranslatorConfiguration()
            {
                Name = "Sample Output Translator",
                Key = "sampleinputtranslator",
                OutputTranslatorType = EntityHeader<OutputTranslatorConfiguration.OutputTranslatorTypes>.Create(OutputTranslatorConfiguration.OutputTranslatorTypes.MessageBased),
            };
            AddId(outTranslator);
            AddOwnedProperties(outTranslator, org);
            AddAuditProperties(outTranslator, createTimeStamp, org, user);
            await _pipelineMgr.AddOutputTranslatorConfigurationAsync(outTranslator, org, user);

            var tmpOutTrn = new RouteModuleConfig()
            {
                Id = Guid.NewGuid().ToId(),
                Name = outTranslator.Name,
                Module = new EntityHeader<DeviceAdmin.Interfaces.IPipelineModuleConfiguration>() { Id = outTranslator.Id, Text = outTranslator.Name, Value = outTranslator },
                ModuleType = EntityHeader<PipelineModuleType>.Create(PipelineModuleType.OutputTranslator)
            };
            var tmpWf = new RouteModuleConfig()
            {
                Id = Guid.NewGuid().ToId(),
                Name = wf.Name,
                Module = new EntityHeader<DeviceAdmin.Interfaces.IPipelineModuleConfiguration>() { Id = wf.Id, Text = wf.Name, Value = wf },
                PrimaryOutput = new RouteConnection() { Id = tmpOutTrn.Id, Name = outTranslator.Name, Mappings = new List<KeyValuePair<string, object>>() },
                ModuleType = EntityHeader<PipelineModuleType>.Create(PipelineModuleType.Workflow)
            };
            var tmpInputTrn = new RouteModuleConfig()
            {
                Id = Guid.NewGuid().ToId(),
                Name = inputTranslator.Name,
                Module = new EntityHeader<DeviceAdmin.Interfaces.IPipelineModuleConfiguration>() { Id = inputTranslator.Id, Text = inputTranslator.Name, Value = inputTranslator },
                PrimaryOutput = new RouteConnection() { Id = tmpWf.Id, Name = wf.Name, Mappings = new List<KeyValuePair<string, object>>() },
                ModuleType = EntityHeader<PipelineModuleType>.Create(PipelineModuleType.InputTranslator)
            };
            var tmpSent = new RouteModuleConfig()
            {
                Id = Guid.NewGuid().ToId(),
                Name = sentinelConfiguration.Name,
                Module = new EntityHeader<DeviceAdmin.Interfaces.IPipelineModuleConfiguration>() { Id = sentinelConfiguration.Id, Text = sentinelConfiguration.Name, Value = sentinelConfiguration },
                PrimaryOutput = new RouteConnection() { Id = tmpInputTrn.Id, Name = inputTranslator.Name, Mappings = new List<KeyValuePair<string, object>>() },
                ModuleType = EntityHeader<PipelineModuleType>.Create(PipelineModuleType.Sentinel)
            };


            /* Create Route */
            var temperatureRoute = new Route()
            {
                Name = "Sample Temperature Route",
                Key = "sampletemproute",
                MessageDefinition = new EntityHeader<DeviceMessaging.Admin.Models.DeviceMessageDefinition>() { Id = msgTempDefinition.Id, Text = msgTempDefinition.Name, Value = msgTempDefinition },
                PipelineModules = new List<RouteModuleConfig>()
            };

            var motOutTran = new RouteModuleConfig()
            {
                Id = Guid.NewGuid().ToId(),
                Name = outTranslator.Name,
                Module = new EntityHeader<DeviceAdmin.Interfaces.IPipelineModuleConfiguration>() { Id = outTranslator.Id, Text = outTranslator.Name, Value = outTranslator },
                ModuleType = EntityHeader<PipelineModuleType>.Create(PipelineModuleType.OutputTranslator)
            };
            var motWf = new RouteModuleConfig()
            {
                Id = Guid.NewGuid().ToId(),
                Name = wf.Name,
                Module = new EntityHeader<DeviceAdmin.Interfaces.IPipelineModuleConfiguration>() { Id = wf.Id, Text = wf.Name, Value = wf },
                PrimaryOutput = new RouteConnection() { Id = motOutTran.Id, Name = outTranslator.Name, Mappings = new List<KeyValuePair<string, object>>() },
                ModuleType = EntityHeader<PipelineModuleType>.Create(PipelineModuleType.Workflow)
            };
            var motInputTrn = new RouteModuleConfig()
            {
                Id = Guid.NewGuid().ToId(),
                Name = inputTranslator.Name,
                Module = new EntityHeader<DeviceAdmin.Interfaces.IPipelineModuleConfiguration>() { Id = inputTranslator.Id, Text = inputTranslator.Name, Value = inputTranslator },
                PrimaryOutput = new RouteConnection() { Id = motWf.Id, Name = wf.Name, Mappings = new List<KeyValuePair<string, object>>() },
                ModuleType = EntityHeader<PipelineModuleType>.Create(PipelineModuleType.InputTranslator)
            };

            var motSent = new RouteModuleConfig()
            {
                Id = Guid.NewGuid().ToId(),
                Name = sentinelConfiguration.Name,
                Module = new EntityHeader<DeviceAdmin.Interfaces.IPipelineModuleConfiguration>() { Id = sentinelConfiguration.Id, Text = sentinelConfiguration.Name, Value = sentinelConfiguration },
                PrimaryOutput = new RouteConnection() { Id = motInputTrn.Id, Name = inputTranslator.Name, Mappings = new List<KeyValuePair<string, object>>() },
                ModuleType = EntityHeader<PipelineModuleType>.Create(PipelineModuleType.Sentinel)
            };

            /* Create Route */
            var motionRoute = new Route()
            {
                Name = "Sample Motion Route",
                Key = "sampletemproute",
                MessageDefinition = new EntityHeader<DeviceMessaging.Admin.Models.DeviceMessageDefinition>() { Id = motionMsgDefinition.Id, Text = motionMsgDefinition.Name, Value = motionMsgDefinition },
                PipelineModules = new List<RouteModuleConfig>()
                 {
                     motSent, motInputTrn, motWf, motOutTran
                 }
            };

            var deviceConfig = new DeviceConfiguration()
            {
                ConfigurationVersion = 1.0,
                Name = "Sample Device Config",
                Key = "sampledeviceconfig",
                Properties = new List<CustomField>()
                {
                     new CustomField()
                     {
                         Id = Guid.NewGuid().ToId(),
                         DefaultValue ="90",
                         FieldType = EntityHeader<ParameterTypes>.Create(ParameterTypes.Decimal),
                         Key = "setpoint",
                         Name = "Setpoint",
                         Label = "Setpoint",
                         HelpText = "Setpoint where temperature will trigger warning",
                         IsRequired = true,
                          Order = 1
                     }
                },
                Routes = new List<Route>()
                {
                    temperatureRoute,
                    motionRoute
                }
            };
            AddId(deviceConfig);
            AddOwnedProperties(deviceConfig, org);
            AddAuditProperties(deviceConfig, createTimeStamp, org, user);
            await _deviceCfgMgr.AddDeviceConfigurationAsync(deviceConfig, org, user);

            var deviceType = new DeviceType()
            {
                Name = "Sample Device Type",
                Key = "sampledevicetype",
                DefaultDeviceConfiguration = new EntityHeader() { Id = deviceConfig.Id, Text = deviceConfig.Name }
            };
            AddId(deviceType);
            AddOwnedProperties(deviceType, org);
            AddAuditProperties(deviceType, createTimeStamp, org, user);
            await _deviceTypeMgr.AddDeviceTypeAsync(deviceType, org, user);


            /* Create Solution */
            var solution = new Solution()
            {
                Id = "Sample App",
                Name = "sampleapp",
                Listeners = new List<EntityHeader<ListenerConfiguration>>() { new EntityHeader<ListenerConfiguration>() { Id = restListener.Id, Text = restListener.Name, Value = restListener } },
                Planner = new EntityHeader<PlannerConfiguration>() { Id = planner.Id, Text = planner.Name, Value = planner },
                DeviceConfigurations = new List<EntityHeader<DeviceConfiguration>>() { new EntityHeader<DeviceConfiguration>() { Id = deviceConfig.Id, Text = deviceConfig.Name, Value = deviceConfig } },
            };
            AddId(solution);
            AddOwnedProperties(solution, org);
            AddAuditProperties(solution, createTimeStamp, org, user);

            return InvokeResult<Solution>.Create(solution);
        }

        public Simulator.Admin.Models.Simulator CreateSimulator(DeploymentInstance instance, EntityHeader org, EntityHeader user, DateTime creationTimeStamp)
        {
            var simulator = new Simulator.Admin.Models.Simulator()
            {
                Id = "Simulator for Sample App",
                Key = "sampleappsimulator",
                DefaultEndPoint = instance.DnsHostName,
                TLSSSL = false,
                DefaultTransport = EntityHeader<Simulator.Admin.Models.TransportTypes>.Create(Simulator.Admin.Models.TransportTypes.RestHttp),
                MessageTemplates = new List<Simulator.Admin.Models.MessageTemplate>()
                {
                   new Simulator.Admin.Models.MessageTemplate()
                   {
                       Name = "Motion Message",
                       Key="motionmsg",
                       Id = Guid.NewGuid().ToId(),
                       TextPayload = "{'motion':'~motionvalue~','level':~motionlevel~}",
                       PathAndQueryString="/smplmot001/~deviceid~",
                       DynamicAttributes = new List<Simulator.Admin.Models.MessageDynamicAttribute>()
                       {
                           new Simulator.Admin.Models.MessageDynamicAttribute()
                           {
                               Id = Guid.NewGuid().ToId(),
                                Key = "motionvalue",
                                DefaultValue = "on",
                                Name = "Motion On/Off",
                                ParameterType = EntityHeader<ParameterTypes>.Create(ParameterTypes.String),
                                Description = "Provide a value of 'on' or 'off' to simulate motion detected"
                           },
                           new Simulator.Admin.Models.MessageDynamicAttribute()
                           {
                                Id = Guid.NewGuid().ToId(),
                                Key = "motionlevel",
                                DefaultValue = "100",
                                Name = "Motion Level",
                                ParameterType = EntityHeader<ParameterTypes>.Create(ParameterTypes.Integer),
                                Description = "Provide a value between 0 and 100 as to the level of motion detected"
                           }
                       }
                   },
                   new Simulator.Admin.Models.MessageTemplate()
                   {
                       Name = "Temperature Message",
                       Key = "tempmsg",
                       Id = Guid.NewGuid().ToId(),
                       TextPayload = "~temperaturevalue~,~humidityvalue~",
                       PayloadType =  EntityHeader<Simulator.Admin.Models.PaylodTypes>.Create(Simulator.Admin.Models.PaylodTypes.String),
                       ContentType = "text/csv",
                       HttpVerb = "POST",
                       MessageHeaders = new List<Simulator.Admin.Models.MessageHeader>()
                       {
                           new Simulator.Admin.Models.MessageHeader() { HeaderName = "x-messageid", Value = "smpltmp001", Id = Guid.NewGuid().ToId(), Key = "xmessageid", Name="x-messageid"},
                           new Simulator.Admin.Models.MessageHeader() { HeaderName = "x-deviceid", Value = "~deviceid~", Id = Guid.NewGuid().ToId(), Key = "xdeviceid", Name="x-deviceid"},
                       },
                       DynamicAttributes = new List<Simulator.Admin.Models.MessageDynamicAttribute>()
                       {
                           new Simulator.Admin.Models.MessageDynamicAttribute()
                           {
                               Id = Guid.NewGuid().ToId(),
                                Key = "temperaturevalue",
                                DefaultValue = "98.6",
                                Name = "Temperature",
                                ParameterType = EntityHeader<ParameterTypes>.Create(ParameterTypes.Decimal),
                                Description = "Provide a value of 'on' or 'off' to simulate motion detected"
                           },
                           new Simulator.Admin.Models.MessageDynamicAttribute()
                           {
                                Id = Guid.NewGuid().ToId(),
                                Key = "humidityvalue",
                                DefaultValue = "65",
                                Name = "Humidity",
                                ParameterType = EntityHeader<ParameterTypes>.Create(ParameterTypes.Decimal),
                                Description = "Provide a value between 0 and 100 as to the level of motion detected"
                           }
                       }},
                }
            };

            AddId(simulator);
            AddOwnedProperties(simulator, org);
            AddAuditProperties(simulator, creationTimeStamp, org, user);

            return simulator;
        }

        public DeploymentInstance CreateInstance(Solution solution, DeviceRepository deviceRepo, EntityHeader org, EntityHeader user, DateTime creationTimeStamp)
        {
            var instance = new DeploymentInstance()
            {
                Name = "Sample Deployment",
                Key = "sample",
                DeviceRepository = new EntityHeader<DeviceRepository>() { Id = deviceRepo.Id, Text = deviceRepo.Name },
                Solution = new EntityHeader<Solution>() { Id = solution.Id, Text = solution.Name },
            };

            AddId(instance);
            AddOwnedProperties(instance, org);
            AddAuditProperties(instance, creationTimeStamp, org, user);
            return instance;
        }

        public async Task<InvokeResult> CreateDeployableApp(EntityHeader org, EntityHeader user)
        {
            var creationDate = DateTime.UtcNow;

            var subscription = CreateFreeSubscription(org, user, creationDate);
            var createSolutionResult = await CreateSimpleSolutionAsync(org, user, creationDate);
            var deviceRepo = CreateDevice(subscription, org, user, creationDate);

            await _solutionMgr.AddSolutionsAsync(createSolutionResult.Result, org, user);


            return InvokeResult.Success;
        }
    }
}
