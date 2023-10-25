using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.IoT.StarterKit.Resources;
using LagoVista.ProjectManagement.Resources;
using System.Collections.Generic;

namespace LagoVista.IoT.StarterKit.Models
{

    [EntityDescription(StarterKitDomain.StarterKit, StarterKitResources.Names.AppWizard_TItle, StarterKitResources.Names.AppWizard_Help,
        StarterKitResources.Names.AppWizard_Help, EntityDescriptionAttribute.EntityTypes.SimpleModel, typeof(StarterKitResources))]
    public class AppWizardRequest : IFormDescriptor
    {
        public string SurveyResponseId { get; set; }

        [FormField(LabelResource: StarterKitResources.Names.AppWizard_ProjectTemplate, FieldType: FieldTypes.EntityHeaderPicker, WaterMark: StarterKitResources.Names.AppWizard_ProjectTemplate_Select,
            EntityHeaderPickerUrl: "/api/project/templates", ResourceType: typeof(StarterKitResources), IsRequired: true, IsUserEditable: true)]
        public EntityHeader ProjectTemplate { get; set; }

        [FormField(LabelResource: StarterKitResources.Names.AppWizard_ProjectName, FieldType: FieldTypes.Text,
            ResourceType: typeof(StarterKitResources), IsRequired: true, IsUserEditable: true)]
        public string ProjectName { get; set; }

        [FormField(LabelResource: PMResources.Names.Project_ProjectCode, ValidationRegEx: @"^[A-Z]{3,20}$", RegExValidationMessageResource: PMResources.Names.Project_ProjectCode_HelpValidation, HelpResource: PMResources.Names.Project_ProjectCode_Help,
                FieldType: FieldTypes.Text, ResourceType: typeof(PMResources), IsRequired: true, IsUserEditable: true)]
        public string ProjectCode { get; set; }

        [FormField(LabelResource: PMResources.Names.Project_ProjectLead, FieldType: FieldTypes.UserPicker, ResourceType: typeof(PMResources), WaterMark: PMResources.Names.Project_ProjectLead_Select, IsRequired: true)]
        public EntityHeader ProjectLead { get; set; }

        [FormField(LabelResource: PMResources.Names.Project_DefaultPrimaryContributor, FieldType: FieldTypes.UserPicker, ResourceType: typeof(PMResources), WaterMark: PMResources.Names.Project_DefaultPrimaryContributor_Select, IsRequired: false)]
        public EntityHeader DefaultPrimaryContributor { get; set; }

        [FormField(LabelResource: PMResources.Names.Project_DefaultQAResource, FieldType: FieldTypes.UserPicker, ResourceType: typeof(PMResources), WaterMark: PMResources.Names.Project_DefaultQAResource_Select, IsRequired: false)]
        public EntityHeader DefaultQAResource { get; set; }

        [FormField(LabelResource: PMResources.Names.Project_Admin_Lead, FieldType: FieldTypes.UserPicker, ResourceType: typeof(PMResources), WaterMark: PMResources.Names.Project_Admin_Lead_Select, IsRequired: true)]
        public EntityHeader ProjectAdminLead { get; set; }


        [FormField(LabelResource: StarterKitResources.Names.AppWizard_Description, FieldType: FieldTypes.MultiLineText,
            ResourceType: typeof(StarterKitResources), IsRequired: false, IsUserEditable: true)]
        public string Description { get; set; }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(ProjectTemplate),
                nameof(ProjectName),
                nameof(ProjectCode),
                nameof(ProjectLead),
                nameof(ProjectAdminLead),
                nameof(DefaultPrimaryContributor),                
                nameof(DefaultQAResource),
                nameof(Description)
            };
        }
    }
}
