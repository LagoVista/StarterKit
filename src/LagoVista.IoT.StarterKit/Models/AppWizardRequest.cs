using LagoVista.Core.Attributes;
using LagoVista.Core.Models;
using LagoVista.IoT.StarterKit.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.IoT.StarterKit.Models
{

    [EntityDescription(StarterKitDomain.StarterKit, StarterKitResources.Names.AppWizard_TItle, StarterKitResources.Names.AppWizard_Help,
        StarterKitResources.Names.AppWizard_Help, EntityDescriptionAttribute.EntityTypes.SimpleModel, typeof(StarterKitResources))]
    public class AppWizardRequest
    {
        public string SurveyResponseId { get; set; }

        [FormField(LabelResource: StarterKitResources.Names.AppWizard_ProjectName, FieldType: FieldTypes.Text, 
            ResourceType: typeof(StarterKitResources), IsRequired: true, IsUserEditable: true)]
        public string ProjectName { get; set; }

        [FormField(LabelResource: StarterKitResources.Names.AppWizard_ProjectKey, FieldType: FieldTypes.Key,
            ResourceType: typeof(StarterKitResources), IsRequired: true, IsUserEditable: true)]
        public string ProjectKey { get; set; }

        [FormField(LabelResource: StarterKitResources.Names.AppWizard_ProjectKey, FieldType: FieldTypes.MultiLineText,
            ResourceType: typeof(StarterKitResources), IsRequired: false, IsUserEditable: true)]
        public string Description { get; set;  }
    }
}
