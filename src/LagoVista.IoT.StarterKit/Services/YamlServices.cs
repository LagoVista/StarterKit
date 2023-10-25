using System;
using System.Collections.Generic;
using System.IO;
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
using LagoVista.ProjectManagement;
using LagoVista.ProjectManagement.Core;
using LagoVista.ProjectManagement.Models;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using YamlDotNet.Serialization;

namespace LagoVista.IoT.StarterKit.Services
{
    public class YamlServices : IYamlServices
    {
        readonly private List<string> _ignoredProperties = new List<string>()
        { "DatabaseName","EntityName","EntityType", "Environment","Id","OwnerOrganization","IsPublic","HasValue",
            "CreationDate","LastUpdatedDate","LastUpdatedBy","OwnerUser","IsValid","CreatedBy","Owner" };

        readonly private List<string> _referenceProperties = new List<string>()
        {   nameof(Verifier.Component),
            "Listeners",
            "Planner",
            "DeviceConfigurations",
            "ChildSurveyType",
            nameof(DeviceType.DefaultDeviceConfiguration) };

        readonly IDeviceAdminManager _deviceAdminMgr;
        readonly ISubscriptionManager _subscriptionMgr;
        readonly IPipelineModuleManager _pipelineMgr;
        readonly IDeviceTypeManager _deviceTypeMgr;
        readonly IDeviceMessageDefinitionManager _deviceMsgMgr;
        readonly IDeploymentInstanceManager _instanceMgr;
        readonly IDeploymentHostManager _hostManager;
        readonly ISolutionManager _solutionMgr;
        readonly IDeviceConfigurationManager _deviceCfgMgr;
        readonly IDeviceRepositoryManager _deviceRepoMgr;
        readonly IProductManager _productManager;
        readonly IVerifierManager _verifierMgr;
        readonly ISimulatorManager _simulatorMgr;
        readonly IDeviceManager _deviceManager;
        readonly IOrganizationRepo _orgRepo;
        readonly IUserManager _userManager;
        readonly IModuleManager _moduleManager;
        readonly IRoleManager _roleManager;
        readonly StorageUtils _storageUtils;
        readonly ISurveyManager _surveyManager;
        readonly ISiteContentManager _siteContentManager;
        readonly IGuideManager _guideManager;
        readonly IGlossaryManager _glossaryManager;
        readonly ISurveyResponseManager _surveyResponseManager;
        readonly IWorkTaskTypeManager _workTaskTypeManager;
        readonly ITaskTemplateManager _taskTemplateManager;
        readonly IProjectTemplateManager _projectTemplateManager;
        readonly IStatusConfigurationManager _statusConfigurationManager;

        private EntityHeader _org;
        private EntityHeader _user;

        public YamlServices(IAdminLogger logger, IStarterKitConnection starterKitConnection, IDeviceAdminManager deviceAdminMgr, ISubscriptionManager subscriptionMgr, IPipelineModuleManager pipelineMgr, IDeviceTypeManager deviceTypeMgr, IDeviceRepositoryManager deviceRepoMgr,
                          IUserManager userManager, IModuleManager moduleManager, IProductManager productManager, IDeviceTypeManager deviceTypeManager, IDeviceConfigurationManager deviceCfgMgr, IDeviceMessageDefinitionManager deviceMsgMgr, IDeploymentInstanceManager instanceMgr,
                          IDeploymentHostManager hostMgr, IRoleManager roleManager, IDeviceManager deviceManager, IContainerRepositoryManager containerMgr, ISolutionManager solutionMgr, IOrganizationRepo orgMgr, ISimulatorManager simMgr, IVerifierManager verifierMgr,
                          ISurveyManager surveyManager, ISurveyResponseManager surveyResponseManager, ISiteContentManager siteContentManager, IGuideManager guideManager, IGlossaryManager glossaryManager, IWorkTaskTypeManager workTaskTypeManager, ITaskTemplateManager taskTemplateManager,
                          IStatusConfigurationManager statusConfigurationManager, IProjectTemplateManager projectTemplateManager)
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
            _moduleManager = moduleManager;
            _roleManager = roleManager;
            _instanceMgr = instanceMgr;
            _solutionMgr = solutionMgr;
            _surveyManager = surveyManager;
            _siteContentManager = siteContentManager;
            _guideManager = guideManager;
            _glossaryManager = glossaryManager;
            _surveyResponseManager = surveyResponseManager;
            _workTaskTypeManager = workTaskTypeManager;
            _taskTemplateManager = taskTemplateManager;
            _statusConfigurationManager = statusConfigurationManager;
            _projectTemplateManager = projectTemplateManager;

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


        private async Task<Object> CreateNuvIoTObject(Type objType, DateTime createDateStamp, EntityHeader org, EntityHeader user, Dictionary<object, object> yaml)
        {
            var obj = Activator.CreateInstance(objType);

            foreach (var key in yaml.Keys)
            {
                if (key is String keyStr)
                {
                    var prop = objType.GetProperties().Where(p => p.Name == keyStr).FirstOrDefault();
                    if (prop == null)
                    {
                        throw new Exception($"Uknown Property {keyStr}");
                    }
                    else if (prop.PropertyType.Name == "EntityHeader`1")
                    {
                        var enumType = prop.PropertyType.GetGenericArguments().First();
                        var enumValue = Enum.Parse(enumType, yaml[key] as string);
                        var createMethod = prop.PropertyType.GetMethod("Create");
                        var ehValue = createMethod.Invoke(null, new object[] { enumValue });
                        prop.SetValue(obj, ehValue);

                    }
                    else if (yaml[key] is IEnumerable<Object> childList)
                    {
                        var childListType = prop.PropertyType.GetGenericArguments().First();

                        var addMethod = prop.PropertyType.GetMethod("Add");
                        foreach (Dictionary<object, object> child in childList)
                        {
                            var childObject = await CreateNuvIoTObject(childListType, createDateStamp, org, user, child);
                            addMethod.Invoke(prop.GetValue(obj), new object[] { childObject });
                        }

                    }
                    else if (yaml[key] is String value)
                    {
                        if (prop.GetAccessors().Where(acc => acc.Name == $"set_{prop.Name}").Any())
                        {
                            if (prop.PropertyType == typeof(bool))
                            {
                                prop.SetValue(obj, bool.Parse(value));
                            }
                            else if (prop.PropertyType == typeof(double))
                            {
                                prop.SetValue(obj, double.Parse(value));
                            }
                            else if (prop.PropertyType == typeof(int))
                            {
                                prop.SetValue(obj, int.Parse(value));
                            }
                            else
                            {
                                value = value.Replace("\\r", "\r").Replace("\\n", "\n");
                                prop.SetValue(obj, value);
                            }
                        }
                    }
                    else if (yaml[key] is Dictionary<object, object>)
                    {
                        var props = yaml[key] as Dictionary<object, object>;
                        Console.WriteLine($"found dictionary for {key}");
                        foreach (var childProp in props)
                        {
                            Console.WriteLine($"\t {childProp.Key} - {childProp.Value}");
                            if (childProp.Key as string == "ehReference")
                            {
                                var childPropDict = childProp.Value as Dictionary<object, object>;
                                var ehName = childPropDict["name"] as string;
                                var ehKey = childPropDict["key"] as string;
                                var ehType = childPropDict["type"] as string;
                                Console.Write($" {ehName} - {ehKey} - {ehType}");

                                var record = await _storageUtils.FindWithKeyAsync(ehKey, ehKey, org);
                                prop.SetValue(obj, new EntityHeader()
                                {
                                    Id = record.Id,
                                    Key = record.Key,
                                    Text = record.Name
                                });
                            }
                        }
                    }
                    else
                    {
                        throw new Exception($"Uknown value type for {keyStr}");
                    }
                }
            }

            if (obj is IIDEntity)
            {
                AddId(obj as IIDEntity);
            }

            if (obj is IAuditableEntity)
            {
                AddAuditProperties(obj as IAuditableEntity, createDateStamp, org, user);
            }

            if (obj is IOwnedEntity)
            {
                AddOwnedProperties(obj as IOwnedEntity, org);
            }

            return obj;
        }

        private async Task<T> CreateNuvIoTObject<T>(DateTime createDateStamp, EntityHeader org, EntityHeader user, Dictionary<object, object> yaml) where T : class
        {
            return await CreateNuvIoTObject(typeof(T), createDateStamp, org, user, yaml) as T;
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
                case nameof(SurveyQuestion.ChildSurveyType):
                    var survey = await _surveyManager.GetSurveyAsync(eh.Id, _org, _user);
                    bldr.AppendLine($"{indent}  type: Survey");
                    bldr.AppendLine($"{indent}  name: {survey.Name}");
                    bldr.AppendLine($"{indent}  key: {survey.Key}");

                    break;
                case nameof(TaskTemplate.WorkTaskType):
                    var taskType = await _workTaskTypeManager.GetWorkTaskTypeAsync(eh.Id, _org, _user);
                    bldr.AppendLine($"{indent}  type: {nameof(TaskTemplate.WorkTaskType)}");
                    bldr.AppendLine($"{indent}  name: {taskType.Name}");
                    bldr.AppendLine($"{indent}  key: {taskType.Key}");

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

            //Console.WriteLine(model.GetType().Name + "." + prop.Name + " " + prop.PropertyType.Name);

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
                    bldr.AppendLine($"{indent}{prop.Name}:");
                    await ApplyEntityHeader(prop, bldr, indent + "  ", model, value);
                    break;

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
                    bldr.AppendLine($"{indent}{prop.Name}:");
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
                catch (Exception)
                {
                    Console.WriteLine($"!!! Error attempting to get value of {prop.Name} of type {prop.PropertyType.Name}");
                    throw;
                }
            }
        }


        public async Task<InvokeResult<Object>> DeserializeFromYamlAsync(string recordType, Stream strm, EntityHeader org, EntityHeader usr)
        {
            _org = org;
            _user = usr;
            var dateStamp = DateTime.UtcNow;

            using (var rdr = new StreamReader(strm))
            {
                var deserializer = new DeserializerBuilder().Build();
                var output = deserializer.Deserialize(rdr) as Dictionary<object, object>;
                if (output == null)
                    return InvokeResult<Object>.FromError($"Could not deserilaize YAML.");

                foreach (var key in output.Keys)
                {
                    var childItem = output[key];
                    if (key is String keyStr)
                        switch (recordType.ToLower())
                        {
                            case "module":
                                var module = await CreateNuvIoTObject<LagoVista.UserAdmin.Models.Security.Module>(dateStamp, org, usr, childItem as Dictionary<object, object>);
                                return InvokeResult<Object>.Create(module);
                            case "survey":
                                var survey = await CreateNuvIoTObject<Survey>(dateStamp, org, usr, childItem as Dictionary<object, object>);
                                return InvokeResult<Object>.Create(survey);
                            case "glossary":
                                var glossary = await CreateNuvIoTObject<Glossary>(dateStamp, org, usr, childItem as Dictionary<object, object>);
                                return InvokeResult<Object>.Create(glossary);
                            case "sitecontent":
                                var sitecontent = await CreateNuvIoTObject<SiteContent>(dateStamp, org, usr, childItem as Dictionary<object, object>);
                                return InvokeResult<Object>.Create(sitecontent);
                            case "guide":
                                var guide = await CreateNuvIoTObject<SiteContent>(dateStamp, org, usr, childItem as Dictionary<object, object>);
                                return InvokeResult<Object>.Create(guide);
                            case "worktasktype":
                                var workTaskType = await CreateNuvIoTObject<WorkTaskType>(dateStamp, org, usr, childItem as Dictionary<object, object>);
                                return InvokeResult<Object>.Create(workTaskType);
                            case "tasktemplate":
                                var template = await CreateNuvIoTObject<TaskTemplate>(dateStamp, org, usr, childItem as Dictionary<object, object>);
                                return InvokeResult<Object>.Create(template);
                            case "statusconfiguration":
                                var statusConfig = await CreateNuvIoTObject<StatusConfiguration>(dateStamp, org, usr, childItem as Dictionary<object, object>);
                                return InvokeResult<Object>.Create(statusConfig);
                            case "projecttemplate":
                                var projectTemplate = await CreateNuvIoTObject<ProjectTemplate>(dateStamp, org, usr, childItem as Dictionary<object, object>);
                                return InvokeResult<Object>.Create(projectTemplate);
                                break;
                            default:
                                return InvokeResult<Object>.FromError($"object type: [{recordType}] not supported.");
                        }

                }
            }
            return InvokeResult<Object>.FromError("could not create object");
        }

        public async Task<InvokeResult<Tuple<string, string>>> SerilizeToYamlAsync(string recordType, string id, EntityHeader org, EntityHeader usr)
        {
            _org = org;
            _user = usr;

            string recordKey = string.Empty;
            StringBuilder bldr = new StringBuilder();
            bldr.AppendLine($"{recordType}:");

            switch (recordType)
            {
                case nameof(DeviceMessageDefinition):
                    var msg = await _deviceMsgMgr.GetDeviceMessageDefinitionAsync(id, org, usr);
                    await GenerateYaml(bldr, msg, 1);
                    recordKey = msg.Key;

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
                    recordKey = deviceConfig.Key;
                    break;
                case nameof(DeviceType):
                    var deviceType = await _deviceTypeMgr.GetDeviceTypeAsync(id, org, usr);
                    await GenerateYaml(bldr, deviceType, 1);
                    recordKey = deviceType.Key;
                    break;
                case nameof(ListenerConfiguration):
                    var listener = await _pipelineMgr.GetListenerConfigurationAsync(id, org, usr);
                    await GenerateYaml(bldr, listener, 1);
                    recordKey = listener.Key;
                    break;
                case nameof(SiteContent):
                    var siteContent = await _siteContentManager.GetSiteContentAsync(id, org, usr);
                    await GenerateYaml(bldr, siteContent, 1);
                    recordKey = siteContent.Key;
                    break;
                case nameof(Glossary):
                    var glossary = await _glossaryManager.GetGlossaryAsync(id, org, usr);
                    await GenerateYaml(bldr, glossary, 1);
                    recordKey = glossary.Key;
                    break;
                case nameof(Guide):
                    var guide = await _guideManager.GetGuideAsync(id, org, usr);
                    await GenerateYaml(bldr, guide, 1);
                    recordKey = guide.Key;
                    break;
                case nameof(Solution):
                    var solution = await _solutionMgr.GetSolutionAsync(id, org, usr);
                    await GenerateYaml(bldr, solution, 1);
                    recordKey = solution.Key;
                    break;
                case nameof(LagoVista.UserAdmin.Models.Security.Module):
                    var module = await _moduleManager.GetModuleAsync(id, org, usr);
                    await GenerateYaml(bldr, module, 1);
                    recordKey = module.Key;
                    break;
                case nameof(LagoVista.ProjectManagement.Models.Survey):
                    var survey = await _surveyManager.GetSurveyAsync(id, org, usr);
                    await GenerateYaml(bldr, survey, 1);
                    recordKey = survey.Key;
                    break;
                case nameof(LagoVista.ProjectManagement.Models.SurveyResponse):
                    var surveyResponse = await _surveyResponseManager.GetSurveyResponseAsync(id, org.Id, org, usr);
                    await GenerateYaml(bldr, surveyResponse, 1);
                    recordKey = surveyResponse.Survey.Key;
                    break;
                case nameof(StatusConfiguration):
                    var statusConfig = await _statusConfigurationManager.GetStatusConfigurationAsync(id, org, usr);
                    await GenerateYaml(bldr, statusConfig, 1);
                    recordKey = statusConfig.Key;
                    break;
                case nameof(TaskTemplate):
                    var template = await _taskTemplateManager.GetTaskTemplateAsync(id, org, usr);
                    await GenerateYaml(bldr, template, 1);
                    recordKey = template.Key;
                    break;
                case nameof(ProjectTemplate):
                    var projectTemplate = await _projectTemplateManager.GetProjectTemplateAsync(id, org, usr);
                    await GenerateYaml(bldr, projectTemplate, 1);
                    recordKey = projectTemplate.Key;
                    break;
                case nameof(WorkTaskType):
                    var workTaskType = await _workTaskTypeManager.GetWorkTaskTypeAsync(id, org, usr);
                    await GenerateYaml(bldr, workTaskType, 1);
                    recordKey = workTaskType.Key;
                    break;
                case nameof(LagoVista.UserAdmin.Models.Users.Role):
                    var role = await _moduleManager.GetModuleAsync(id, org, usr);
                    await GenerateYaml(bldr, role, 1);
                    recordKey = role.Key;
                    break;

                default:
                    return InvokeResult<Tuple<string, string>>.FromError($"Don't know how to handle object of type [{recordType}]");
            }

            var fileName = $"{(await _orgRepo.GetOrganizationAsync(org.Id)).Namespace}.{recordType}.{recordKey}.yaml"; ;

            return InvokeResult<Tuple<string, string>>.Create(Tuple.Create(bldr.ToString(), fileName));
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
