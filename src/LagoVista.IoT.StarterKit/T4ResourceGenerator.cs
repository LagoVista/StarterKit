/*7/22/2024 6:03:25 PM*/
using System.Globalization;
using System.Reflection;

//Resources:StarterKitResources:AppWizard_Description
namespace LagoVista.IoT.StarterKit.Resources
{
	public class StarterKitResources
	{
        private static global::System.Resources.ResourceManager _resourceManager;
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        private static global::System.Resources.ResourceManager ResourceManager 
		{
            get 
			{
                if (object.ReferenceEquals(_resourceManager, null)) 
				{
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("LagoVista.IoT.StarterKit.Resources.StarterKitResources", typeof(StarterKitResources).GetTypeInfo().Assembly);
                    _resourceManager = temp;
                }
                return _resourceManager;
            }
        }
        
        /// <summary>
        ///   Returns the formatted resource string.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        private static string GetResourceString(string key, params string[] tokens)
		{
			var culture = CultureInfo.CurrentCulture;;
            var str = ResourceManager.GetString(key, culture);

			for(int i = 0; i < tokens.Length; i += 2)
				str = str.Replace(tokens[i], tokens[i+1]);
										
            return str;
        }
        
        /// <summary>
        ///   Returns the formatted resource string.
        /// </summary>
		/*
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        private static HtmlString GetResourceHtmlString(string key, params string[] tokens)
		{
			var str = GetResourceString(key, tokens);
							
			if(str.StartsWith("HTML:"))
				str = str.Substring(5);

			return new HtmlString(str);
        }*/
		
		public static string AppWizard_Description { get { return GetResourceString("AppWizard_Description"); } }
//Resources:StarterKitResources:AppWizard_Help

		public static string AppWizard_Help { get { return GetResourceString("AppWizard_Help"); } }
//Resources:StarterKitResources:AppWizard_ProjectDescription

		public static string AppWizard_ProjectDescription { get { return GetResourceString("AppWizard_ProjectDescription"); } }
//Resources:StarterKitResources:AppWizard_ProjectKey

		public static string AppWizard_ProjectKey { get { return GetResourceString("AppWizard_ProjectKey"); } }
//Resources:StarterKitResources:AppWizard_ProjectName

		public static string AppWizard_ProjectName { get { return GetResourceString("AppWizard_ProjectName"); } }
//Resources:StarterKitResources:AppWizard_ProjectTemplate

		public static string AppWizard_ProjectTemplate { get { return GetResourceString("AppWizard_ProjectTemplate"); } }
//Resources:StarterKitResources:AppWizard_ProjectTemplate_Select

		public static string AppWizard_ProjectTemplate_Select { get { return GetResourceString("AppWizard_ProjectTemplate_Select"); } }
//Resources:StarterKitResources:AppWizard_TItle

		public static string AppWizard_TItle { get { return GetResourceString("AppWizard_TItle"); } }
//Resources:StarterKitResources:Common_Description

		public static string Common_Description { get { return GetResourceString("Common_Description"); } }
//Resources:StarterKitResources:Common_Name

		public static string Common_Name { get { return GetResourceString("Common_Name"); } }
//Resources:StarterKitResources:Common_Summary

		public static string Common_Summary { get { return GetResourceString("Common_Summary"); } }
//Resources:StarterKitResources:ProductLine_Description

		public static string ProductLine_Description { get { return GetResourceString("ProductLine_Description"); } }
//Resources:StarterKitResources:ProductLine_Name

		public static string ProductLine_Name { get { return GetResourceString("ProductLine_Name"); } }
//Resources:StarterKitResources:ProductLine_Objects

		public static string ProductLine_Objects { get { return GetResourceString("ProductLine_Objects"); } }
//Resources:StarterKitResources:ProductLine_Objects_Help

		public static string ProductLine_Objects_Help { get { return GetResourceString("ProductLine_Objects_Help"); } }
//Resources:StarterKitResources:ProductLine_ToDoTemplate_Help

		public static string ProductLine_ToDoTemplate_Help { get { return GetResourceString("ProductLine_ToDoTemplate_Help"); } }
//Resources:StarterKitResources:ProductLine_ToDoTemplates

		public static string ProductLine_ToDoTemplates { get { return GetResourceString("ProductLine_ToDoTemplates"); } }
//Resources:StarterKitResources:ProductLineObject_Help

		public static string ProductLineObject_Help { get { return GetResourceString("ProductLineObject_Help"); } }
//Resources:StarterKitResources:ProductLineObject_Object

		public static string ProductLineObject_Object { get { return GetResourceString("ProductLineObject_Object"); } }
//Resources:StarterKitResources:ProductLineObject_Title

		public static string ProductLineObject_Title { get { return GetResourceString("ProductLineObject_Title"); } }
//Resources:StarterKitResources:ProductLineObjectType_ObjectType

		public static string ProductLineObjectType_ObjectType { get { return GetResourceString("ProductLineObjectType_ObjectType"); } }
//Resources:StarterKitResources:ProductLines_Name

		public static string ProductLines_Name { get { return GetResourceString("ProductLines_Name"); } }
//Resources:StarterKitResources:ToDoTemplate_DueDateDays

		public static string ToDoTemplate_DueDateDays { get { return GetResourceString("ToDoTemplate_DueDateDays"); } }
//Resources:StarterKitResources:ToDoTemplate_DueDateDays_Help

		public static string ToDoTemplate_DueDateDays_Help { get { return GetResourceString("ToDoTemplate_DueDateDays_Help"); } }
//Resources:StarterKitResources:ToDoTemplate_Help

		public static string ToDoTemplate_Help { get { return GetResourceString("ToDoTemplate_Help"); } }
//Resources:StarterKitResources:ToDoTemplate_Instructions

		public static string ToDoTemplate_Instructions { get { return GetResourceString("ToDoTemplate_Instructions"); } }
//Resources:StarterKitResources:ToDoTemplate_Priority

		public static string ToDoTemplate_Priority { get { return GetResourceString("ToDoTemplate_Priority"); } }
//Resources:StarterKitResources:ToDoTemplate_Select_Priority

		public static string ToDoTemplate_Select_Priority { get { return GetResourceString("ToDoTemplate_Select_Priority"); } }
//Resources:StarterKitResources:ToDoTemplate_Title

		public static string ToDoTemplate_Title { get { return GetResourceString("ToDoTemplate_Title"); } }
//Resources:StarterKitResources:ToDoTemplate_WebLink

		public static string ToDoTemplate_WebLink { get { return GetResourceString("ToDoTemplate_WebLink"); } }

		public static class Names
		{
			public const string AppWizard_Description = "AppWizard_Description";
			public const string AppWizard_Help = "AppWizard_Help";
			public const string AppWizard_ProjectDescription = "AppWizard_ProjectDescription";
			public const string AppWizard_ProjectKey = "AppWizard_ProjectKey";
			public const string AppWizard_ProjectName = "AppWizard_ProjectName";
			public const string AppWizard_ProjectTemplate = "AppWizard_ProjectTemplate";
			public const string AppWizard_ProjectTemplate_Select = "AppWizard_ProjectTemplate_Select";
			public const string AppWizard_TItle = "AppWizard_TItle";
			public const string Common_Description = "Common_Description";
			public const string Common_Name = "Common_Name";
			public const string Common_Summary = "Common_Summary";
			public const string ProductLine_Description = "ProductLine_Description";
			public const string ProductLine_Name = "ProductLine_Name";
			public const string ProductLine_Objects = "ProductLine_Objects";
			public const string ProductLine_Objects_Help = "ProductLine_Objects_Help";
			public const string ProductLine_ToDoTemplate_Help = "ProductLine_ToDoTemplate_Help";
			public const string ProductLine_ToDoTemplates = "ProductLine_ToDoTemplates";
			public const string ProductLineObject_Help = "ProductLineObject_Help";
			public const string ProductLineObject_Object = "ProductLineObject_Object";
			public const string ProductLineObject_Title = "ProductLineObject_Title";
			public const string ProductLineObjectType_ObjectType = "ProductLineObjectType_ObjectType";
			public const string ProductLines_Name = "ProductLines_Name";
			public const string ToDoTemplate_DueDateDays = "ToDoTemplate_DueDateDays";
			public const string ToDoTemplate_DueDateDays_Help = "ToDoTemplate_DueDateDays_Help";
			public const string ToDoTemplate_Help = "ToDoTemplate_Help";
			public const string ToDoTemplate_Instructions = "ToDoTemplate_Instructions";
			public const string ToDoTemplate_Priority = "ToDoTemplate_Priority";
			public const string ToDoTemplate_Select_Priority = "ToDoTemplate_Select_Priority";
			public const string ToDoTemplate_Title = "ToDoTemplate_Title";
			public const string ToDoTemplate_WebLink = "ToDoTemplate_WebLink";
		}
	}
}

