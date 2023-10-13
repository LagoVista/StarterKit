using LagoVista.Core.Models.UIMetaData;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.IoT.Web.Common.Controllers;
using LagoVista.ProjectManagement.Models;
using LagoVista.UserAdmin.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace LagoVista.IoT.StarterKit.Rest.Controllers
{
    /// <summary>
    /// Clone Data Controller - services to clone NuvIoT objects.
    /// </summary>
    [Authorize]
    public class CloneDataController : LagoVistaBaseController
    {
        private readonly ICloneServices _cloneService;

        /// <summary>
        /// Clone Data Controller - services to clone NuvIoT objects.
        /// </summary>
        /// <param name="cloneServices"></param>
        /// <param name="userManager"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public CloneDataController(ICloneServices cloneServices, UserManager<AppUser> userManager, IAdminLogger logger) : base(userManager, logger)
        {
            this._cloneService = cloneServices ?? throw new ArgumentNullException(nameof(cloneServices));
        }


        /// <summary>
        /// Clone Data Controller - Facotry for Project Clone Request.
        /// </summary>
        /// <returns>UI Instance for cloning projects.</returns>
        [HttpGet("/api/dataservices/clone/project/factory")]
        public DetailResponse<CloneProjectRequest> ClonedRequestFactory()
        {
            return DetailResponse<CloneProjectRequest>.Create();
        }

        /// <summary>
        /// Clone Data Controller - Clone a Project
        /// </summary>
        /// <param name="request">Data used to clone project.</param>
        /// <returns>Cloned Project</returns>
        [HttpGet("/api/dataservices/clone/project")]
        public async Task<DetailResponse<Project>> CloneProjectAsync([FromBody] CloneProjectRequest request)
        {
            var project = await _cloneService.CloneProjectAsync(request, OrgEntityHeader, UserEntityHeader);
            return DetailResponse<Project>.Create(project);
        }
    }
}
