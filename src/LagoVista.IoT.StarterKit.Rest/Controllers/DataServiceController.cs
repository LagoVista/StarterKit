using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.IoT.StarterKit.Services;
using LagoVista.IoT.Web.Common.Attributes;
using LagoVista.IoT.Web.Common.Controllers;
using LagoVista.UserAdmin.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;

namespace LagoVista.IoT.StarterKit.Rest.Controllers
{
    /// <summary>
    /// Controller to extract and apply YAML for objects
    /// </summary>
    [Authorize]
    [AppBuilder]
    public class DataServiceController : LagoVistaBaseController
    {
        IYamlServices _yamlServices;
        IHostingEnvironment _env;

        /// <summary>
        /// Constructor for controller that creates sample projects.
        /// </summary>
        /// <param name="orgInitializer"></param>
        /// <param name="userManager"></param>
        /// <param name="logger"></param>
        public DataServiceController(IYamlServices orgInitializer, UserManager<AppUser> userManager, IAdminLogger logger, IHostingEnvironment env) : base(userManager, logger)
        {
            this._yamlServices = orgInitializer ?? throw new ArgumentNullException(nameof(orgInitializer));
            this._env = env ?? throw new ArgumentNullException(nameof(env));
        }


        /// <summary>
        /// Generate YAML for the given record type and ID.
        /// </summary>
        /// <param name="recordtype"></param>
        /// <param name="recordid"></param>
        /// <returns></returns>
        [HttpGet("/api/dataservices/yaml/{recordtype}/{recordid}/generate")]
        public async Task<IActionResult> GetYAMLAsync(String recordtype, string recordid)
        {
            var result = await _yamlServices.GetYamlAsync(recordtype, recordid, OrgEntityHeader, UserEntityHeader);

            var buffer = System.Text.ASCIIEncoding.ASCII.GetBytes(result.Result);
         
            return File(buffer, "text/yaml", "out.yaml");
        }
    }
}
