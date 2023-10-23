/*10/23/2023 4:23:39 PM*/
using System.Globalization;
using System.Reflection;

//Resources:StarterKitResources:AppWizard_Help
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
		
		public static string AppWizard_Help { get { return GetResourceString("AppWizard_Help"); } }
//Resources:StarterKitResources:AppWizard_ProjectDescription

		public static string AppWizard_ProjectDescription { get { return GetResourceString("AppWizard_ProjectDescription"); } }
//Resources:StarterKitResources:AppWizard_ProjectKey

		public static string AppWizard_ProjectKey { get { return GetResourceString("AppWizard_ProjectKey"); } }
//Resources:StarterKitResources:AppWizard_ProjectName

		public static string AppWizard_ProjectName { get { return GetResourceString("AppWizard_ProjectName"); } }
//Resources:StarterKitResources:AppWizard_TItle

		public static string AppWizard_TItle { get { return GetResourceString("AppWizard_TItle"); } }

		public static class Names
		{
			public const string AppWizard_Help = "AppWizard_Help";
			public const string AppWizard_ProjectDescription = "AppWizard_ProjectDescription";
			public const string AppWizard_ProjectKey = "AppWizard_ProjectKey";
			public const string AppWizard_ProjectName = "AppWizard_ProjectName";
			public const string AppWizard_TItle = "AppWizard_TItle";
		}
	}
}

