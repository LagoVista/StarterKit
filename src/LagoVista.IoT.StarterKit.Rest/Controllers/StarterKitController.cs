using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.IoT.Runtime.Core.Module;
using LagoVista.IoT.Web.Common.Controllers;
using LagoVista.UserAdmin.Models.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.IoT.StarterKit.Rest.Controllers
{
    /// <summary>
    /// Controller to create sample projects
    /// </summary>
    public class StarterKitController : LagoVistaBaseController
    {
        /// <summary>
        /// Constructor for controller that creates sample projects.
        /// </summary>
        /// <param name="userManager"></param>
        /// <param name="logger"></param>
        public StarterKitController(UserManager<AppUser> userManager, IAdminLogger logger) : base(userManager, logger)
        {

        }

        /// <summary>
        /// Starter Kit - Create Initial Sample
        /// </summary>
        /// <returns></returns>
        [HttpGet("/api/samples/initial")]
        public Task<InvokeResult> CreateInitialSampleAsync()
        {
            throw new NotImplementedException();
        }
    }
}
