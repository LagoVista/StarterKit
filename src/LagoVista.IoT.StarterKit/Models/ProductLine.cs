// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: e0508c3e532aa55b712bcf3f18ee8378943ba9b054abb16f32b3432c10751db3
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core;
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.IoT.StarterKit.Resources;
using LagoVista.ProjectManagement.Models;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LagoVista.IoT.StarterKit.Models
{
    [EntityDescription(StarterKitDomain.StarterKit, StarterKitResources.Names.ProductLine_Name, StarterKitResources.Names.ProductLineObject_Help,        
        StarterKitResources.Names.ProductLineObject_Help, EntityDescriptionAttribute.EntityTypes.SimpleModel, typeof(StarterKitResources),
        FactoryUrl: "/api/productline/factory", GetListUrl:"/api/productlines", GetUrl:"/api/productline/{id}", DeleteUrl: "/api/productline/{id}", SaveUrl:"/api/productline" )]
    public class ProductLine : EntityBase, IFormDescriptor, IFormDescriptorCol2, ISummaryFactory, IValidateable
    {

        [FormField(LabelResource: StarterKitResources.Names.ProductLine_Products, FieldType: FieldTypes.ProductPickerList, ResourceType: typeof(StarterKitResources))]
        public List<EntityHeader> Products { get; set; } = new List<EntityHeader>();

        [FormField(LabelResource: StarterKitResources.Names.ProductLine_Objects, HelpResource:StarterKitResources.Names.ProductLine_Objects_Help, FieldType: FieldTypes.ChildListInline, ResourceType: typeof(StarterKitResources),
            FactoryUrl: "/api/productline/object/factory" )]
        public List<ProductLineObject> Objects { get; set; } = new List<ProductLineObject>();

        [FormField(LabelResource: StarterKitResources.Names.ProductLine_ToDoTemplates, HelpResource: StarterKitResources.Names.ProductLine_ToDoTemplate_Help,  FieldType: FieldTypes.ChildListInline, ResourceType: typeof(StarterKitResources),
            FactoryUrl: "/api/productline/todotemplate/factory")]
        public List<ToDoTemplate> ToDoTemplates { get; set; } = new List<ToDoTemplate>();


        [FormField(LabelResource: StarterKitResources.Names.Common_Summary, FieldType: FieldTypes.MultiLineText, ResourceType: typeof(StarterKitResources), IsRequired: true, IsUserEditable: true)]
        public string Summary { get; set; }

        [FormField(LabelResource: StarterKitResources.Names.Common_Description, FieldType: FieldTypes.HtmlEditor, ResourceType: typeof(StarterKitResources), IsRequired: false, IsUserEditable: true)]
        public string Description { get; set; }


        public ProductLineSummary CreateSummary()
        {
            return new ProductLineSummary()
            {
                Description = Description,
                Id = Id,
                IsPublic = IsPublic,
                Key = Key,
                Name = Name,
            };
        }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),                
                nameof(Key),
                nameof(Summary),
                nameof(Description)               
            };
        }

        public List<string> GetFormFieldsCol2()
        {
            return new List<string>()
            {
                nameof(Products),
                nameof(Objects),
                nameof(ToDoTemplates)
            };
        }

        ISummaryData ISummaryFactory.CreateSummary()
        {
            return CreateSummary();
        }
    }


    [EntityDescription(StarterKitDomain.StarterKit, StarterKitResources.Names.ProductLines_Name, StarterKitResources.Names.ProductLineObject_Help,
        StarterKitResources.Names.ProductLineObject_Help, EntityDescriptionAttribute.EntityTypes.SimpleModel, typeof(StarterKitResources),
        FactoryUrl: "/api/productline/factory", GetListUrl: "/api/productlines", GetUrl: "/api/productline/{id}", DeleteUrl: "/api/productline/{id}", SaveUrl: "/api/productline")]
    public class ProductLineSummary : SummaryData
    {

    }


    [EntityDescription(StarterKitDomain.StarterKit, StarterKitResources.Names.AppWizard_TItle, StarterKitResources.Names.AppWizard_Help,
        StarterKitResources.Names.AppWizard_Help, EntityDescriptionAttribute.EntityTypes.SimpleModel, typeof(StarterKitResources),
        FactoryUrl: "/api/productline/object/factory")]
    public class ProductLineObject : IFormDescriptor, IFormConditionalFields
    {
        public ProductLineObject()
        {
            Id = Guid.NewGuid().ToId();
        }

        public string Id { get; set; }


        [FormField(LabelResource: StarterKitResources.Names.Common_Name, FieldType: FieldTypes.Text, ResourceType: typeof(StarterKitResources), IsRequired: true, IsUserEditable: true)]
        public string Name { get; set; }

        [FormField(LabelResource: StarterKitResources.Names.ProductLineObjectType_ObjectType, WaterMark: StarterKitResources.Names.ProductLineObject_ObjectType_Select, 
            FieldType: FieldTypes.EntityHeaderPicker,
           EntityHeaderPickerUrl: "/api/objects/cloneable", ResourceType: typeof(StarterKitResources), IsRequired: true, IsUserEditable: true)]
        public EntityHeader ObjectType { get; set; }

        [FormField(LabelResource: StarterKitResources.Names.ProductLineObject_Object, WaterMark: StarterKitResources.Names.ProductLineObject_Object_Select,  FieldType: FieldTypes.EntityHeaderPicker, 
           EntityHeaderPickerUrl: "/api/dataservices/objects/{ObjectType.Id}", ResourceType: typeof(StarterKitResources), IsRequired: true, IsUserEditable: true)]
        public EntityHeader Object { get; set; }

        [FormField(LabelResource: StarterKitResources.Names.ProductLineObject_CustomizationInstructions, FieldType: FieldTypes.HtmlEditor, ResourceType: typeof(StarterKitResources), IsRequired: false, IsUserEditable: true)]
        public string CustomizationInstructions { get; set; }
        

        [FormField(LabelResource: StarterKitResources.Names.ProductLine_ToDoTemplates, HelpResource: StarterKitResources.Names.ProductLine_ToDoTemplate_Help, FieldType: FieldTypes.ChildListInline, ResourceType: typeof(StarterKitResources),
            FactoryUrl: "/api/productline/todotemplate/factory")]
        public List<ToDoTemplate> ToDoTemplates { get; set; } = new List<ToDoTemplate>();

        public FormConditionals GetConditionalFields()
        {
            return new FormConditionals()
            {
                ConditionalFields = new List<string>() { nameof(Object) },
                Conditionals = new List<FormConditional>()
                 {
                    new FormConditional()
                    {
                        RequiredFields = new List<string>() {nameof(ObjectType), nameof(Object) },
                        VisibleFields = new List<string>() {nameof(Object )},
                        Field = nameof(ObjectType),
                        Value = "*"
                    }
                 }
            };
        }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(ObjectType),
                nameof(Object),
                nameof(CustomizationInstructions),
                nameof(ToDoTemplates),
            };
        }
    }

    [EntityDescription(StarterKitDomain.StarterKit, StarterKitResources.Names.ToDoTemplate_Title, StarterKitResources.Names.ToDoTemplate_Help,
        StarterKitResources.Names.ToDoTemplate_Help, EntityDescriptionAttribute.EntityTypes.SimpleModel, typeof(StarterKitResources),
        FactoryUrl:"/api/productline/todotemplate/factory"  )]
    public class ToDoTemplate : IFormDescriptor
    {
        public ToDoTemplate()
        {
            Id = Guid.NewGuid().ToId();
            Priority = EntityHeader<ToDo_Priority>.Create(ToDo_Priority.Normal);
            DueDateDays = 7;
        }

        public string Id { get; set; }

        [FormField(LabelResource: StarterKitResources.Names.ToDoTemplate_DueDateDays, HelpResource: StarterKitResources.Names.ToDoTemplate_DueDateDays_Help, 
            FieldType: FieldTypes.Integer, ResourceType: typeof(StarterKitResources), IsRequired: true, IsUserEditable: true)]
        public int DueDateDays { get; set; }

        [FormField(LabelResource: StarterKitResources.Names.ToDoTemplate_Priority, FieldType: FieldTypes.Picker, EnumType: typeof(ToDo_Priority),
            WaterMark: StarterKitResources.Names.ToDoTemplate_Select_Priority, ResourceType: typeof(StarterKitResources), IsRequired: true, IsUserEditable: true)]
        public EntityHeader<ToDo_Priority> Priority { get; set; }

        [FormField(LabelResource: StarterKitResources.Names.Common_Name, FieldType: FieldTypes.Text, ResourceType: typeof(StarterKitResources), IsRequired: true, IsUserEditable: true)]
        public string Name { get; set; }

        [FormField(LabelResource: StarterKitResources.Names.ToDoTemplate_Instructions, FieldType: FieldTypes.HtmlEditor, ResourceType: typeof(StarterKitResources), IsRequired: true, IsUserEditable: true)]
        public string Instructions { get; set; }

        [FormField(LabelResource: StarterKitResources.Names.ToDoTemplate_WebLink, FieldType: FieldTypes.Text, ResourceType: typeof(StarterKitResources), IsRequired: false, IsUserEditable: true)]
        public string WebLink { get; set; }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(DueDateDays),
                nameof(Priority),
                nameof(Instructions),
                nameof(WebLink)
            };
        }
    }
}


