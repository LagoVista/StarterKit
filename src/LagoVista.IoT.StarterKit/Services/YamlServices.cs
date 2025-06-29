﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Bibliography;
using LagoVista.CloudStorage.Storage;
using LagoVista.Core;
using LagoVista.Core.Attributes;
using LagoVista.Core.Exceptions;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.IoT.Billing;
using LagoVista.IoT.Billing.Models;
using LagoVista.IoT.Deployment.Admin;
using LagoVista.IoT.Deployment.Admin.Models;
using LagoVista.IoT.Deployment.Models;
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
using LagoVista.Manufacturing.Models;
using LagoVista.ProjectManagement;
using LagoVista.ProjectManagement.Core;
using LagoVista.ProjectManagement.Models;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Models.Security;
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
            "MessageDefinition",
            nameof(DeviceType.DefaultDeviceConfiguration) };

        readonly IDeviceAdminManager _deviceAdminMgr;
        readonly ISubscriptionManager _subscriptionMgr;
        readonly IPipelineModuleManager _pipelineMgr;

        readonly IDeviceMessageDefinitionManager _deviceMsgMgr;
        readonly IDeploymentInstanceManager _instanceMgr;
        readonly IDeploymentHostManager _hostManager;
        readonly ISolutionManager _solutionMgr;
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

        public YamlServices(IAdminLogger logger, IStarterKitConnection starterKitConnection, IDeviceAdminManager deviceAdminMgr, ISubscriptionManager subscriptionMgr, IPipelineModuleManager pipelineMgr,  IDeviceRepositoryManager deviceRepoMgr,
                          IUserManager userManager, IModuleManager moduleManager, IProductManager productManager, IDeviceTypeManager deviceTypeManager,  IDeviceMessageDefinitionManager deviceMsgMgr, IDeploymentInstanceManager instanceMgr,
                          IDeploymentHostManager hostMgr, IRoleManager roleManager, IDeviceManager deviceManager, IContainerRepositoryManager containerMgr, ISolutionManager solutionMgr, IOrganizationRepo orgMgr, ISimulatorManager simMgr, IVerifierManager verifierMgr,
                          ISurveyManager surveyManager, ISurveyResponseManager surveyResponseManager, ISiteContentManager siteContentManager, IGuideManager guideManager, IGlossaryManager glossaryManager, IWorkTaskTypeManager workTaskTypeManager, ITaskTemplateManager taskTemplateManager,
                          IStatusConfigurationManager statusConfigurationManager, IProjectTemplateManager projectTemplateManager)
        {
            _userManager = userManager;
            _deviceAdminMgr = deviceAdminMgr;
            _subscriptionMgr = subscriptionMgr;
            _pipelineMgr = pipelineMgr;
            _deviceMsgMgr = deviceMsgMgr;
            _deviceRepoMgr = deviceRepoMgr;
            _productManager = productManager;
            _verifierMgr = verifierMgr;
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
                    try
                    {
                        if (keyStr == "ehReference")
                        {
                            var childPropDict = yaml[key] as Dictionary<object, object>;
                            var ehName = childPropDict["text"] as string;
                            var ehId = childPropDict["id"] as string;
                            var ehKey = childPropDict["key"] as string;
                            var ehType = childPropDict["type"] as string;

                            return EntityHeader.Create(ehId, ehKey, ehName);
                        }
                        else
                        {
                            var prop = objType.GetProperties().Where(p => p.Name == keyStr).FirstOrDefault();
                            if (prop == null)
                            {
                                throw new Exception($"Unknown Property {keyStr}");
                            }
                            else if (prop.PropertyType.Name == "EntityHeader`1")
                            {
                                var enumType = prop.PropertyType.GetGenericArguments().First();
                                var enumValue = Enum.Parse(enumType, yaml[key] as string);
                                var createMethod = prop.PropertyType.GetMethod("Create", new Type[] { enumType });
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
                                var propType = prop.PropertyType;
                                var nullablePropType = Nullable.GetUnderlyingType(prop.PropertyType);
                                if (nullablePropType != null)
                                    propType = nullablePropType;

                                if (prop.GetAccessors().Where(acc => acc.Name == $"set_{prop.Name}").Any())
                                {
                                    if (propType == typeof(bool))
                                    {
                                        prop.SetValue(obj, bool.Parse(value));
                                    }
                                    else if (propType == typeof(double))
                                    {
                                        prop.SetValue(obj, double.Parse(value));
                                    }
                                    else if (propType == typeof(int))
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
                                if (props != null)
                                {
                                    foreach (var childProp in props)
                                    {
                                        if (childProp.Key as string == "ehReference")
                                        {
                                            var childPropDict = childProp.Value as Dictionary<object, object>;
                                            var ehName = childPropDict["name"] as string;
                                            var ehKey = childPropDict["key"] as string;
                                            var ehType = childPropDict["type"] as string;
                                  
                                            switch (ehType)
                                            {
                                                case "category":
                                                    prop.SetValue(obj, new EntityHeader()
                                                    {
                                                        Id = childPropDict["id"] as string,
                                                        Key = ehKey,
                                                        Text = ehName
                                                    });

                                                    break;
                                                case "uiCategory":
                                                    prop.SetValue(obj, new EntityHeader()
                                                    {
                                                        Id = childPropDict["id"] as string,
                                                        Key = ehKey,
                                                        Text = ehName
                                                    });
                                                    break;
                                                default:
                                                    var record = await _storageUtils.FindWithKeyAsync(ehType, ehKey, org);
                                                    if (record == null)
                                                        throw new Core.Exceptions.InvalidDataException($"could not find record: {ehKey} of type {ehType}");

                                                    prop.SetValue(obj, new EntityHeader()
                                                    {
                                                        Id = record.Id,
                                                        Key = record.Key,
                                                        Text = record.Name
                                                    });
                                                    break;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                throw new Exception($"Uknown value type for {keyStr}");
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                        Console.WriteLine($"[YamlServices__CreateNuvIoTObject] {ex.Message} ObjectType: {objType.Name} Key: {key}");
                        Console.WriteLine(ex.StackTrace);
                        Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                        throw;
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
                    var config = await _storageUtils.FindWithIdAsync<DeviceConfiguration>(eh.Id, _org.Id);
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
                case nameof(UiCategory):
                    bldr.AppendLine($"{indent}  type: uiCategory");
                    bldr.AppendLine($"{indent}  name: {eh.Text}");
                    bldr.AppendLine($"{indent}  key: {eh.Key}");
                    bldr.AppendLine($"{indent}  id: {eh.Id}");
                    break;
                case nameof(LagoVista.ProjectManagement.Models.GlossaryTerm.Related):
                    bldr.AppendLine($"{indent}  type: related");
                    bldr.AppendLine($"{indent}  text: {eh.Text}");
                    bldr.AppendLine($"{indent}  key: {eh.Key}");
                    bldr.AppendLine($"{indent}  id: {eh.Id}");
                    break;
                case nameof(Category):
                    bldr.AppendLine($"{indent}  type: category");
                    bldr.AppendLine($"{indent}  text: {eh.Text}");
                    bldr.AppendLine($"{indent}  key: {eh.Key}");
                    bldr.AppendLine($"{indent}  id: {eh.Id}");
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

        public bool IsNullable(Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

        public async Task<bool> ApplyProperty(PropertyInfo prop, StringBuilder bldr, string indent, Object model, Object value, int level)
        {
            var propValue = prop.GetValue(model);
            if(propValue == null && !IsNullable(prop.PropertyType))
            {
                return false;
            }

            var propType = prop.PropertyType.Name;

            var nullableType = Nullable.GetUnderlyingType(prop.PropertyType);
            if(nullableType != null)
            {
                propType = nullableType.Name;
            }

            switch (propType)
            {
                case "bool":
                case "Boolean":
                    var boolValue = prop.GetValue(model);
                    if(boolValue != null)
                        bldr.AppendLine($"{indent}{prop.Name}: {prop.GetValue(model)}");
                    return true;
                case "Int32":
                case "int":
                case "double":
                case "Double":
                    var numberValue = prop.GetValue(model);
                    if(numberValue != null)
                        bldr.AppendLine($"{indent}{prop.Name}: {prop.GetValue(model)}");
                    return true;
                case "String":
                case "string":
                    var strValue = value as String;
                    if (!String.IsNullOrEmpty(strValue))
                    {
                        strValue = strValue.Replace("\n", "\\n").Replace("\r", "\\r");
                        strValue = strValue.Replace(@"""", @"\""");
                        bldr.AppendLine($"{indent}{prop.Name}: \"{strValue}\"");
                        return true;
                    }
                    break;
                case "EntityHeader":
                    bldr.AppendLine($"{indent}{prop.Name}:");
                    await ApplyEntityHeader(prop, bldr, indent + "  ", model, value);
                    return true;

                case "EntityHeader`1":
                    var attr = prop.GetCustomAttribute<FKeyPropertyAttribute>();

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
                        bldr.AppendLine($"{indent}{prop.Name}: {enumValue ?? "UNKNOWN"}");
                    }
                    return true;
                default:
                    bldr.AppendLine($"{indent}{prop.Name}:");
                    await GenerateYaml(bldr, value, level + 1);
                    return true;
            }

            return false;
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

        private void VerboseLog(string message)
        {
            //Console.WriteLine(message);
        }

        public async Task GenerateYaml(StringBuilder bldr, Object objectToSerializeIsNull, int level, bool isList = false)
        {
            if (objectToSerializeIsNull == null)
            {
                throw new ArgumentNullException(nameof(objectToSerializeIsNull));
            }

            var indent = level.GetIndent();

            var props = objectToSerializeIsNull.GetType().GetProperties();
            VerboseLog($"Generate YAML => Found {props.Length.ToString()} to process on level {level} of type {objectToSerializeIsNull.GetType().Name}");

            if (objectToSerializeIsNull.GetType().Name.StartsWith("KeyValuePair"))
            {
                VerboseLog($"Generate YAML => Processing Key Value Pair Property {objectToSerializeIsNull.GetType().Name}");

                var propertyValue = (KeyValuePair<string, object>)objectToSerializeIsNull;
                bldr.AppendLine($"kvp:");
                bldr.AppendLine($"{indent}   key: {propertyValue.Key}");
                bldr.AppendLine($"{indent}   value:");

                VerboseLog($"CONTENT ON KVP: {propertyValue.Value.GetType().Name}");
                VerboseLog($"CONTENT ON KVP: {propertyValue.Value}");

                if (propertyValue.Value.GetType().Name == "JObject" || propertyValue.Value.GetType().Name == "String")
                {
                    var lines = propertyValue.Value.ToString().Split('\n');
                    foreach (var line in lines)
                    {
                        if (line.Trim() != String.Empty)
                            bldr.AppendLine($"{indent}     {line}");
                    }
                }
                else
                    await GenerateYaml(bldr, propertyValue.Value, level + 2);
            }
            else
            {
                var first = true;
                foreach (var prop in props.Where(prp => !prp.GetAccessors(true).First().IsStatic))
                {
                    try
                    {
                        var p = prop.GetIndexParameters();
                        var value = prop.GetValue(objectToSerializeIsNull);
                        if (value == null)
                        {
                            continue;
                        }
                        if (value is System.Collections.IEnumerable list && !(value is String))
                        {
                            VerboseLog("Generate YAML => Processing List: " + prop.Name);
                            await ProcessList(bldr, prop, list, level);
                        }
                        else
                        {
                            VerboseLog("Generate YAML => Processing Standard Properties: " + prop.Name);

                            if (!_ignoredProperties.Contains(prop.Name))
                            {
                                var applied = await ApplyProperty(prop, bldr, first && isList ? String.Empty : indent, objectToSerializeIsNull, value, level);
                                first = !applied;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"!!! Generate YAML => Error attempting to get value of {prop.Name} of type {prop.PropertyType.Name} {ex.Message}");
                        throw;
                    }
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
                            case "landingpagelayout":
                                var lpl = await CreateNuvIoTObject<LandingPageLayout>(dateStamp, org, usr, childItem as Dictionary<object, object>);
                                return InvokeResult<Object>.Create(lpl);
                            case "emailtemplatelayout":
                                var emt = await CreateNuvIoTObject<EmailTemplateLayout>(dateStamp, org, usr, childItem as Dictionary<object, object>);
                                return InvokeResult<Object>.Create(emt);
                            case "emailtemplate":
                                var et = await CreateNuvIoTObject<EmailTemplate>(dateStamp, org, usr, childItem as Dictionary<object, object>);
                                return InvokeResult<Object>.Create(et);
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
                            case "systemtest":
                                var systemTest = await CreateNuvIoTObject<SystemTest>(dateStamp, org, usr, childItem as Dictionary<object, object>);
                                return InvokeResult<Object>.Create(systemTest);
                            case "deviceconfiguration":
                                var deviceConfig = await CreateNuvIoTObject<DeviceConfiguration>(dateStamp, org, usr, childItem as Dictionary<object, object>);
                                return InvokeResult<Object>.Create(deviceConfig);


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
                case nameof(LandingPageLayout):
                    var lpLayout = await _storageUtils.FindWithIdAsync<LandingPageLayout>(id, org.Id);
                    await GenerateYaml(bldr, lpLayout, 1);
                    recordKey = lpLayout.Key;
                    break;
                case nameof(EmailTemplateLayout):
                    var emailTemplateLayout = await _storageUtils.FindWithIdAsync<EmailTemplateLayout>(id, org.Id);
                    await GenerateYaml(bldr, emailTemplateLayout, 1);
                    recordKey = emailTemplateLayout.Key;
                    break;

                case nameof(DeviceConfiguration):
                    var deviceConfig = await _storageUtils.FindWithIdAsync<DeviceConfiguration>(id, org.Id);
                    await GenerateYaml(bldr, deviceConfig, 1);
                    recordKey = deviceConfig.Key;
                    break;
                case nameof(DeviceType):
                    var deviceType = await _storageUtils.FindWithIdAsync<DeviceType>(id, org.Id);
                    await GenerateYaml(bldr, deviceType, 1);
                    recordKey = deviceType.Key;
                    break;
                case nameof(ListenerConfiguration):
                    var listener = await _storageUtils.FindWithIdAsync<ListenerConfiguration>(id, org.Id);
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
                case nameof(SystemTest):
                    var systemTest = await _storageUtils.FindWithIdAsync<SystemTest>(id, org.Id);
                    if(systemTest == null) throw new RecordNotFoundException(nameof(systemTest), id);
                    await GenerateYaml(bldr, systemTest, 1);
                    recordKey = systemTest.Key; 
                    break;
                case nameof(DeviceErrorCode):
                case nameof(DeviceNotification):
                case nameof(IncidentProtocol):
                case nameof(ComponentPackage):
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
