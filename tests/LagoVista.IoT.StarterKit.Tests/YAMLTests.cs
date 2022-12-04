//using LagoVista.Core.Models;
//using LagoVista.IoT.DeviceAdmin.Models;
//using LagoVista.IoT.DeviceMessaging.Admin.Models;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Reflection;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using YamlDotNet.Serialization;
//using LagoVista.Core.Interfaces;
//using LagoVista.Core;
//using LagoVista.IoT.Runtime.Core.Models.Verifiers;
//using LagoVista.IoT.DeviceAdmin.Models.Resources;
//using YamlDotNet.Core;
//using System.Collections;

//namespace LagoVista.IoT.StarterKit.Tests
//{
//    [TestClass]
//    public class YAMLTests
//    {





//        [TestMethod]
//        public void ReadYAML()
//        {
//            var typeLibrary = new Dictionary<string, Object>();

//            var yaml = System.IO.File.ReadAllText("GuideData.yaml");

//            var org = EntityHeader.Create(Guid.NewGuid().ToId(), "My Org");
//            var usr = EntityHeader.Create(Guid.NewGuid().ToId(), "My User");
//            var dateStamp = DateTime.UtcNow;

//            var rdr = new StringReader(yaml);
//            var deserializer = new DeserializerBuilder().Build();
//            var output = deserializer.Deserialize(rdr) as Dictionary<object, object>;
//            foreach (var key in output.Keys)
//            {
//                var childItem = output[key];
//                if (key is String keyStr)
//                    switch (keyStr.ToLower())
//                    {
//                        case "module":
//                            var module = CreateNuvIoTObject<LagoVista.UserAdmin.Models.Security.Module>(dateStamp, org, usr, childItem as Dictionary<object, object>);
//                            break;
//                        case "msgtype":
//                            var msg = CreateNuvIoTObject<DeviceMessageDefinition>(dateStamp, org, usr, childItem as Dictionary<object, object>);
//                            typeLibrary.Add($"{keyStr}-{msg.Key}", msg);
//                            break;
//                        case "msgtypeVerifier":
//                            var verifier = CreateNuvIoTObject<Verifier>(dateStamp, org, usr, childItem as Dictionary<object, object>);
//                            typeLibrary.Add($"{keyStr}-{verifier.Key}", verifier);
//                            break;
//                    }
//            }

//            Console.WriteLine(output.GetType().Name);
//        }


//        [TestMethod]
//        public void LoadModuleYRML()
//        {
//            var typeLibrary = new Dictionary<string, Object>();

//            var yaml = System.IO.File.ReadAllText("sftlog.project.yaml");

//            var org = EntityHeader.Create(Guid.NewGuid().ToId(), "My Org");
//            var usr = EntityHeader.Create(Guid.NewGuid().ToId(), "My User");
//            var dateStamp = DateTime.UtcNow;

//            var rdr = new StringReader(yaml);
//            var deserializer = new DeserializerBuilder().Build();
//            var output = deserializer.Deserialize(rdr) as Dictionary<object, object>;
//            foreach (var key in output.Keys)
//            {
//                var childItem = output[key];
//                if (key is String keyStr)
//                    switch (keyStr.ToLower())
//                    {
//                        case "module":
//                            var module = CreateNuvIoTObject<LagoVista.UserAdmin.Models.Security.Module>(dateStamp, org, usr, childItem as Dictionary<object, object>);
//                            ''
//                            typeLibrary.Add($"{keyStr}-{module.Key}", module);
//                            break;
//                        case "msgtype":
//                            var msg = CreateNuvIoTObject<DeviceMessageDefinition>(dateStamp, org, usr, childItem as Dictionary<object, object>);
//                            typeLibrary.Add($"{keyStr}-{msg.Key}", msg);
//                            break;
//                        case "msgtypeVerifier":
//                            var verifier = CreateNuvIoTObject<Verifier>(dateStamp, org, usr, childItem as Dictionary<object, object>);
//                            typeLibrary.Add($"{keyStr}-{verifier.Key}", verifier);
//                            break;
//                    }
//            }

//            Console.WriteLine(output.GetType().Name);
//        }



//        public DeviceWorkflow AddDeviceWorkflow(String name, string key, EntityHeader org, EntityHeader user, DateTime createTimestamp)
//        {
//            var wf = new DeviceWorkflow()
//            {
//                Id = Guid.NewGuid().ToId(),
//                Name = name,
//                Key = key,
//            };

//            wf.Pages.Add(new Page()
//            {
//                PageNumber = 1,
//                Name = DeviceLibraryResources.Common_PageNumberOne
//            });

//            var attr = new DeviceAdmin.Models.Attribute()
//            {
//                Name = "Motion Status",
//                Key = "motionstatus",
//            };

//            attr.AttributeType = EntityHeader<ParameterTypes>.Create(ParameterTypes.String);
//            AddOwnedProperties(attr, org);
//            AddAuditProperties(attr, createTimestamp, org, user);
//            AddId(attr);

//            attr.IncomingConnections.Add(new Connection()
//            {
//                NodeKey = "motionstatus",
//                NodeName = "Motion Status",
//                NodeType = "workflowinput"
//            });

//            attr.OnSetScript = @"/* 
// * Provide a script to customize setting an attribute:
// *
// * The default setter method for the Motion Status attribute is:
// * 
// *     Attributes.motionstatus = value;
// *
// * If you do not provide a script, the attribute will be
// * set automatically, if a script is present, you will be
// * responsible for taking the value as an input parameter
// * and making the assignment.  
// * 
// * You can add conditional logic so that the attribute will
// * not get set.
// */
//function onSet(value /* String */) {    
//    let phoneNumber = '555-1212';
//    Attributes.motionstatus = value;
//    if(value === 'True'){
//        sendSMS(phoneNumber, 'Motion detected on ' + IoTDevice.name);
//    } 
//    else {
//        sendSMS(phoneNumber, 'Motion cleared on ' + IoTDevice.name);
//    }
//};";

//            attr.DiagramLocations.Add(new DiagramLocation()
//            {
//                Page = 1,
//                X = 120,
//                Y = 120,
//            });

//            var input = new DeviceAdmin.Models.WorkflowInput()
//            {
//                Name = "Motion Status",
//                Key = "motionstatus",
//            };

//            input.InputType = EntityHeader<ParameterTypes>.Create(ParameterTypes.String);
//            input.OutgoingConnections.Add(new Connection()
//            {
//                NodeKey = "motionstatus",
//                NodeName = "Motion Status",
//                NodeType = "attribute"
//            });

//            AddOwnedProperties(input, org);
//            AddAuditProperties(input, createTimestamp, org, user);
//            AddId(input);

//            input.DiagramLocations.Add(new DiagramLocation()
//            {
//                Page = 1,
//                X = 20,
//                Y = 20,
//            });

//            wf.Attributes.Add(attr);
//            wf.Inputs.Add(input);

//            AddOwnedProperties(wf, org);
//            AddAuditProperties(wf, createTimestamp, org, user);


//            return wf;
//        }


//        public void GenerateXaml(Object obj, StringBuilder bldr, int level, bool isList = false)
//        {
//            if (obj == null)
//            {
//                return;
//            }

//            var indent = "";
//            for (var idx = 0; idx < level; ++idx)
//            {
//                indent += "  ";
//            }

//            if (isList)
//            {
//                bldr.Append($"{indent}- ");
//                indent = String.Empty;
//                level++;
//                for (var idx = 0; idx < level; ++idx)
//                {
//                    indent += "  ";
//                }
//            }

//            var first = true;
//            var props = obj.GetType().GetProperties();
//            foreach (var prop in props.Where(prp => !prp.GetAccessors(true).First().IsStatic))
//            {
//                var value = prop.GetValue(obj);
//                if (value == null)
//                {
//                    continue;
//                }
//                var currentIndent = first && isList ? String.Empty : indent;

//                if (value is System.Collections.IEnumerable list && !(value is String))
//                {
//                    var size = 0;
//                    foreach (var child in list)
//                    {
//                        size++;
//                    }

//                    if (size > 0)
//                    {
//                        bldr.AppendLine($"{indent}{prop.Name}:");

//                        foreach (var child in list)
//                        {
//                            GenerateXaml(child, bldr, level + 1, true);
//                        }
//                    }
//                }
//                else
//                {
//                    switch (prop.Name)
//                    {
//                        case "DatabaseName":
//                        case "EntityName":
//                        case "Environment":
//                        case "Id":
//                        case "OwnerOrganization":
//                        case "IsPublic":
//                        case "HasValue":
//                        case "CreationDate":
//                        case "LastUpdatedDate":
//                        case "LastUpdatedBy":
//                        case "OwnerUser":
//                        case "IsValid":
//                        case "CreatedBy":
//                        case "Owner":
//                            break;
//                        default:

//                            switch (prop.PropertyType.Name)
//                            {
//                                case "Bool":
//                                case "Boolean":
//                                    bldr.AppendLine($"{currentIndent}{prop.Name}: {prop.GetValue(obj)}");
//                                    break;
//                                case "Int32":
//                                case "int":
//                                case "double":
//                                case "Double":
//                                    bldr.AppendLine($"{currentIndent}{prop.Name}: {prop.GetValue(obj)}");
//                                    first = false;
//                                    break;
//                                case "String":
//                                case "string":
//                                    var strValue = value as String;
//                                    if (!String.IsNullOrEmpty(strValue))
//                                    {
//                                        strValue = strValue.Replace("\n", "\\n").Replace("\r", "\\r");
//                                        bldr.AppendLine($"{currentIndent}{prop.Name}: {strValue}");
//                                    }
//                                    first = false;
//                                    break;
//                                case "EntityHeader":
//                                    break;
//                                case "EntityHeader`1":
//                                    var objValue = prop.GetValue(obj) as EntityHeader;
//                                    var valueProp = objValue.GetType().GetProperties().Where(prp => prp.Name == "Value").First();
//                                    var enumValue = valueProp.GetValue(objValue);

//                                    bldr.AppendLine($"{currentIndent}{prop.Name}: {enumValue}");
//                                    first = false;
//                                    break;
//                                default:

//                                    bldr.AppendLine($"{currentIndent}{prop.Name} - UNSUPPROTED- {prop.PropertyType}");
//                                    first = false;
//                                    GenerateXaml(value, bldr, level + 1);
//                                    break;
//                            }
//                            break;
//                    }
//                }
//            }
//        }

//        [TestMethod]
//        public void SerializeYAML()
//        {
//            var org = EntityHeader.Create(Guid.NewGuid().ToId(), "My Org");
//            var usr = EntityHeader.Create(Guid.NewGuid().ToId(), "My User");
//            var dateStamp = DateTime.UtcNow;

//            var wf = AddDeviceWorkflow("my WF", "mywf", org, usr, dateStamp);

//            var serializer = new SerializerBuilder().Build();
//            var writer = new StringWriter();
//            serializer.Serialize(writer, wf);

//            Console.WriteLine(writer.ToString());
//        }


//        [TestMethod]
//        public void GenerateYAML()
//        {
//            var org = EntityHeader.Create(Guid.NewGuid().ToId(), "My Org");
//            var usr = EntityHeader.Create(Guid.NewGuid().ToId(), "My User");
//            var dateStamp = DateTime.UtcNow;

//            var wf = AddDeviceWorkflow("my WF", "mywf", org, usr, dateStamp);

//            var bldr = new StringBuilder();
//            bldr.AppendLine("workflow:");

//            GenerateXaml(wf, bldr, 1);

//            Console.WriteLine(bldr.ToString());

//            var rdr = new StringReader(bldr.ToString());
//            var deserializer = new DeserializerBuilder().Build();
//            var output = deserializer.Deserialize(rdr) as Dictionary<object, object>;
//            foreach (var key in output.Keys)
//            {
//                var childItem = output[key];
//                if (key is String keyStr)
//                    switch (keyStr)
//                    {
//                        case "msgtype":
//                            var msg = CreateNuvIoTObject<DeviceMessageDefinition>(dateStamp, org, usr, childItem as Dictionary<object, object>);

//                            break;
//                        case "msgtypeVerifier":
//                            var verifier = CreateNuvIoTObject<Verifier>(dateStamp, org, usr, childItem as Dictionary<object, object>);
//                            break;
//                        case "workflow":
//                            var createdWF = CreateNuvIoTObject<DeviceWorkflow>(dateStamp, org, usr, childItem as Dictionary<object, object>);



//                            break;
//                    }
//            }


//        }
//    }
//}
