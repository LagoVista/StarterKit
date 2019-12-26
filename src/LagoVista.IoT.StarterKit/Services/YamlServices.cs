using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LagoVista.CloudStorage.Storage;
using LagoVista.Core;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.IoT.Billing;
using LagoVista.IoT.Deployment.Admin;
using LagoVista.IoT.Deployment.Admin.Models;
using LagoVista.IoT.DeviceAdmin.Interfaces.Managers;
using LagoVista.IoT.DeviceAdmin.Models;
using LagoVista.IoT.DeviceManagement.Core;
using LagoVista.IoT.DeviceManagement.Core.Managers;
using LagoVista.IoT.DeviceMessaging.Admin.Managers;
using LagoVista.IoT.DeviceMessaging.Admin.Models;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.IoT.Pipeline.Admin.Managers;
using LagoVista.IoT.Pipeline.Admin.Models;
using LagoVista.IoT.Runtime.Core.Models.Verifiers;
using LagoVista.IoT.Simulator.Admin.Managers;
using LagoVista.IoT.Verifiers.Managers;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;

namespace LagoVista.IoT.StarterKit.Services
{
    public class YamlServices : IYamlServices
    {
        private List<string> _ignoredProperties = new List<string>()
        { "DatabaseName","EntityName","EntityType", "Environment","Id","OwnerOrganization","IsPublic","HasValue",
            "CreationDate","LastUpdatedDate","LastUpdatedBy","OwnerUser","IsValid","CreatedBy","Owner" };

        private List<string> _referenceProperties = new List<string>()
        {   nameof(Verifier.Component),
            "Listeners",
            "Planner",
            "DeviceConfigurations",
            nameof(DeviceType.DefaultDeviceConfiguration) };

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
        StorageUtils _storageUtils;

        private EntityHeader _org;
        private EntityHeader _user;

        public YamlServices(IAdminLogger logger, IStarterKitConnection starterKitConnection, IDeviceAdminManager deviceAdminMgr, ISubscriptionManager subscriptionMgr, IPipelineModuleManager pipelineMgr, IDeviceTypeManager deviceTypeMgr, IDeviceRepositoryManager deviceRepoMgr,
                          IUserManager userManager, IProductManager productManager, IDeviceTypeManager deviceTypeManager, IDeviceConfigurationManager deviceCfgMgr, IDeviceMessageDefinitionManager deviceMsgMgr, IDeploymentInstanceManager instanceMgr,
                          IDeploymentHostManager hostMgr, IDeviceManager deviceManager, IContainerRepositoryManager containerMgr, ISolutionManager solutionMgr, IOrganizationRepo orgMgr, ISimulatorManager simMgr, IVerifierManager verifierMgr)
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

            _instanceMgr = instanceMgr;
            _solutionMgr = solutionMgr;

            _storageUtils = new StorageUtils(new Uri(starterKitConnection.StarterKitStorage.Uri), starterKitConnection.StarterKitStorage.AccessKey,
                starterKitConnection.StarterKitStorage.ResourceName, logger);
        }




        protected void AddAuditProperties(IAuditableEntity entity, DateTime creationTimeStamp, EntityHeader org, EntityHeader user)
        {
            entity.CreationDate = creationTimeStamp.ToJSONString();
            entity.LastUpdatedDate = creationTimeStamp.ToJSONString();
            entity.CreatedBy = user;
            entity.LastUpdatedBy = user;
        }

        protected void AddOwnedProperties(IOwnedEntity entity, EntityHeader org)
        {
            entity.OwnerOrganization = org;
            entity.IsPublic = false;
        }

        protected void AddId(IIDEntity entity)
        {
            entity.Id = Guid.NewGuid().ToId();
        }

        public async Task ApplyReferenceEntityHeader(String propName, StringBuilder bldr, String indent, EntityHeader eh, Object model = null)
        {
            switch (propName)
            {
                case nameof(Verifier.Component):
                    var msg = await _deviceMsgMgr.GetDeviceMessageDefinitionAsync(eh.Id, _org, _user);
                    var verifier = model as Verifier;
                    bldr.AppendLine($"{indent}  type: {propName}");
                    bldr.AppendLine($"{indent}  name: {msg.Name}");
                    bldr.AppendLine($"{indent}  subType: {verifier.VerifierType}");
                    bldr.AppendLine($"{indent}  key: {msg.Key}");
                    break;
                case "Listeners":
                    var listener = await _pipelineMgr.GetListenerConfigurationAsync(eh.Id, _org, _user);
                    bldr.AppendLine($"{indent}  type: Listener");
                    bldr.AppendLine($"{indent}  name: {listener.Name}");
                    bldr.AppendLine($"{indent}  key: {listener.Key}");
                    break;
                case nameof(DeviceType.DefaultDeviceConfiguration):
                case "DeviceConfigurations":
                    var config = await _deviceCfgMgr.GetDeviceConfigurationAsync(eh.Id, _org, _user);
                    bldr.AppendLine($"{indent}  type: DeviceConfiguration");
                    bldr.AppendLine($"{indent}  name: {config.Name}");
                    bldr.AppendLine($"{indent}  key: {config.Key}");
                    break;
                case "Planner":
                    var planner = await _pipelineMgr.GetPlannerConfigurationAsync(eh.Id, _org, _user);
                    bldr.AppendLine($"{indent}  type: Planner");
                    bldr.AppendLine($"{indent}  name: {planner.Name}");
                    bldr.AppendLine($"{indent}  key: {planner.Key}");

                    break;
                default:
                    bldr.AppendLine($"{indent}  Don't know how to process {propName}");
                    break;
            }
        }

        public async Task ApplyEntityHeader(PropertyInfo prop, StringBuilder bldr, String indent, Object model, Object value)
        {
            var eh = value as EntityHeader;
            if (!EntityHeader.IsNullOrEmpty(eh))
            {
                bldr.AppendLine($"{indent}ehReference:");
                await ApplyReferenceEntityHeader(prop.Name, bldr, indent + " ", eh, model);
            }
        }

        public async Task ApplyProperty(PropertyInfo prop, StringBuilder bldr, string indent, Object model, Object value, int level)
        {

            Console.WriteLine(model.GetType().Name + "." + prop.Name + " " + prop.PropertyType.Name);

            switch (prop.PropertyType.Name)
            {
                case "Bool":
                case "Boolean":
                    bldr.AppendLine($"{indent}{prop.Name}: {prop.GetValue(model)}");
                    break;
                case "Int32":
                case "int":
                case "double":
                case "Double":
                    bldr.AppendLine($"{indent}{prop.Name}: {prop.GetValue(model)}");
                    break;
                case "String":
                case "string":
                    var strValue = value as String;
                    if (!String.IsNullOrEmpty(strValue))
                    {
                        strValue = strValue.Replace("\n", "\\n").Replace("\r", "\\r");
                        bldr.AppendLine($"{indent}{prop.Name}: {strValue}");
                    }
                    break;
                case "EntityHeader":
                case "EntityHeader`1":
                    if (_referenceProperties.Contains(prop.Name))
                    {
                        bldr.AppendLine($"{indent}{prop.Name}:");
                        await ApplyEntityHeader(prop, bldr, indent + "  ", model, value);
                    }
                    else
                    {
                        var objValue = value as EntityHeader;
                        var valueProp = objValue.GetType().GetProperties().Where(prp => prp.Name == "Value").First();
                        var enumValue = valueProp.GetValue(objValue);
                        bldr.AppendLine($"{indent}{prop.Name}: {enumValue ?? "UKNOWN"}");
                    }
                    break;
                default:

                    bldr.AppendLine($"{indent}{prop.Name} - UNSUPPROTED- {prop.PropertyType}");
                    await GenerateYaml(bldr, value, level + 1);
                    break;
            }
        }

        public async Task ProcessList(StringBuilder bldr, PropertyInfo prop, System.Collections.IEnumerable list, int level)
        {
            var size = 0;
            foreach (var child in list)
            {
                size++;
            }

            if (size > 0)
            {
                var indent = level.GetIndent();
                bldr.AppendLine($"{indent}{prop.Name}:");

                foreach (var child in list)
                {
                    bldr.Append($"{indent}  - ");

                    if (child.GetType().Name.StartsWith("EntityHeader"))
                    {
                        var eh = child as EntityHeader;
                        bldr.AppendLine($"ehReference:");
                        await ApplyReferenceEntityHeader(prop.Name, bldr, indent + "    ", eh);
                    }
                    else
                    {
                        await GenerateYaml(bldr, child, level + 2, true);
                    }
                }
            }
        }

        private async Task GenerateYaml(StringBuilder bldr, Object obj, int level, bool isList = false)
        {
            if (obj == null)
            {
                return;
            }

            var indent = level.GetIndent();

            var props = obj.GetType().GetProperties();
            var first = true;
            foreach (var prop in props.Where(prp => !prp.GetAccessors(true).First().IsStatic))
            {
                try
                {

                    var value = prop.GetValue(obj);
                    if (value == null)
                    {
                        continue;
                    }

                    if (value is System.Collections.IEnumerable list && !(value is String))
                    {
                        await ProcessList(bldr, prop, list, level);
                    }
                    else
                    {
                        if (!_ignoredProperties.Contains(prop.Name))
                        {
                            await ApplyProperty(prop, bldr, first && isList ? String.Empty : indent, obj, value, level);
                            first = false;
                        }
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"!!! Error attempting to get value of {prop.Name} of type {prop.PropertyType.Name}");
                    throw;
                }
            }
        }


        public Task<InvokeResult> ApplyXaml(string yaml, EntityHeader org, EntityHeader usr)
        {
            throw new NotImplementedException();
        }

        public async Task<InvokeResult<string>> GetYamlAsync(string recordType, string id, EntityHeader org, EntityHeader usr)
        {
            _org = org;
            _user = usr;

            StringBuilder bldr = new StringBuilder();
            bldr.AppendLine($"{recordType}:");

            switch (recordType)
            {
                case nameof(DeviceMessageDefinition):
                    var msg = await _deviceMsgMgr.GetDeviceMessageDefinitionAsync(id, org, usr);
                    await GenerateYaml(bldr, msg, 1);

                    var verifiers = await _verifierMgr.GetVerifierForComponentAsync(id, org, usr);
                    foreach (var verifier in verifiers)
                    {
                        bldr.AppendLine();
                        bldr.AppendLine("Verifier:");
                        await GenerateYaml(bldr, verifier, 1);
                    }

                    break;
                case nameof(DeviceConfiguration):
                    var deviceConfig = await _deviceCfgMgr.GetDeviceConfigurationAsync(id, org, usr);
                    await GenerateYaml(bldr, deviceConfig, 1);
                    break;
                case nameof(DeviceType):
                    var deviceType = await _deviceTypeMgr.GetDeviceTypeAsync(id, org, usr);
                    await GenerateYaml(bldr, deviceType, 1);
                    break;
                case nameof(ListenerConfiguration):
                    var listener = await _pipelineMgr.GetListenerConfigurationAsync(id, org, usr);
                    await GenerateYaml(bldr, listener, 1);

                    break;
                case nameof(Solution):
                    var solution = await _solutionMgr.GetSolutionAsync(id, org, usr);
                    await GenerateYaml(bldr, solution, 1);
                    break;
                default:
                    return InvokeResult<String>.FromError($"Don't know how to handle object of type [{recordType}]");
            }

            return InvokeResult<string>.Create(bldr.ToString());
        }
    }

    public static class Helpers
    {
        public static String GetIndent(this int level)
        {
            var indent = "";
            for (var idx = 0; idx < level; ++idx)
            {
                indent += "  ";
            }

            return indent;
        }
    }

}
