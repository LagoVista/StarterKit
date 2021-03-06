﻿using LagoVista.Core.Exceptions;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.IoT.Runtime.Core.Module;
using LagoVista.IoT.Web.Common.Controllers;
using LagoVista.UserAdmin;
using LagoVista.UserAdmin.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
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
    [Authorize]
    public class StarterKitController : LagoVistaBaseController
    {
        readonly IOrgInitializer _orgInitializer;
        readonly IWebHostEnvironment _env;

        /// <summary>
        /// Constructor for controller that creates sample projects.
        /// </summary>
        /// <param name="orgInitializer"></param>
        /// <param name="userManager"></param>
        /// <param name="logger"></param>
        /// <param name="env" />
        public StarterKitController(IOrgInitializer orgInitializer, UserManager<AppUser> userManager, IAdminLogger logger, IWebHostEnvironment env) : base(userManager, logger)
        {
            this._orgInitializer = orgInitializer ?? throw new ArgumentNullException(nameof(orgInitializer));
            this._env = env ?? throw new ArgumentNullException(nameof(env));
        }

        /// <summary>
        /// Starter Kit - Create Initial Sample
        /// </summary>
        /// <returns></returns>
        [HttpGet("/api/examples/create/default")]
        public async Task<InvokeResult> CreateInitialSampleAsync()
        {
            try
            {
                return await this._orgInitializer.CreateExampleAppAsync(_env.EnvironmentName.ToLower(), OrgEntityHeader, UserEntityHeader);
            }
            catch(RecordNotFoundException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.RecordType + " " + ex.RecordId);
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                Console.ResetColor();

                return InvokeResult.FromException("CreateInitialSampleAsync", ex);
            }
            catch(ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                Console.ResetColor();

                return InvokeResult.FromException("CreateInitialSampleAsync", ex);
            }
        }
    }
}
