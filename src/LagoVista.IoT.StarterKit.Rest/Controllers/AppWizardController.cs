using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.IoT.StarterKit.Models;
using LagoVista.IoT.Web.Common.Controllers;
using LagoVista.ProjectManagement.Models;
using LagoVista.UserAdmin.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace LagoVista.IoT.StarterKit.Rest.Controllers
{
    /// <summary>
    /// App Wizard Controller - Utilities to create applications from surveys.
    /// </summary>
    [Authorize]
    public class AppWizardController : LagoVistaBaseController
    {
        private readonly IAppWizard _appWizard;

        /// <summary>
        /// App Wizard Controller - Create applications from surveys
        /// </summary>
        /// <param name="appWizard"></param>
        /// <param name="userManager"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public AppWizardController(IAppWizard appWizard, UserManager<AppUser> userManager, IAdminLogger logger) : base(userManager, logger)
        {
            _appWizard = appWizard ?? throw new ArgumentNullException(nameof(appWizard));
        }

        /// <summary>
        /// AppWizard - Create a project from an app wizard request.
        /// </summary>
        /// <param name="appWizardRequest">App Wizard Request object used to create project.</param>
        /// <returns></returns>
        [HttpPost("/api/appwizard")]
        public Task<InvokeResult<Project>> CreateProjectFromSurvey([FromBody] AppWizardRequest appWizardRequest)
        {
            return _appWizard.CreateProjectAsync(appWizardRequest, OrgEntityHeader, UserEntityHeader);
        }

        /// <summary>
        /// AppWizard - Create a survey request object that can be populated via User Interface to create a project.
        /// </summary>
        /// <param name="surveyresponseid">Survey Response ID used to create the project</param>
        /// <returns>Form Detail Object used to Populate Details for creating an app.</returns>
        [HttpGet("/api/appwizard/{surveyresponseid}/request/factory")]
        public DetailResponse<AppWizardRequest> CreateRequest(string surveyresponseid)
        {
            var request = new AppWizardRequest()
            {
                SurveyResponseId = surveyresponseid
            };
            return DetailResponse<AppWizardRequest>.Create(request);
        }

    }
}
