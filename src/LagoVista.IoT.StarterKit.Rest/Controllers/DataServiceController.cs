﻿using LagoVista.Core.Validation;
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
using Microsoft.AspNetCore.Http;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Models;
using LagoVista.IoT.StarterKit.Interfaces;

namespace LagoVista.IoT.StarterKit.Rest.Controllers
{
    /// <summary>
    /// Controller to extract and apply YAML for objects
    /// </summary>
    [Authorize]
    [AppBuilder]
    public class DataServiceController : LagoVistaBaseController
    {
        readonly IYamlServices _yamlServices;
        readonly IWebHostEnvironment _env;
        readonly IDataServicesManager _dataServicesManager;

		/// <summary>
		/// Constructor for controller that creates sample projects.
		/// </summary>
		/// <param name="orgInitializer"></param>
		/// <param name="userManager"></param>
		/// <param name="logger"></param>
		/// <param name="env"></param>
		public DataServiceController(IYamlServices orgInitializer, IDataServicesManager dataServicesManager, UserManager<AppUser> userManager, IAdminLogger logger, IWebHostEnvironment env) : base(userManager, logger)
        {
            this._yamlServices = orgInitializer ?? throw new ArgumentNullException(nameof(orgInitializer));
            this._env = env ?? throw new ArgumentNullException(nameof(env));
            this._dataServicesManager = dataServicesManager ?? throw new ArgumentNullException(nameof(dataServicesManager));
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
            var result = await _yamlServices.SerilizeToYamlAsync(recordtype, recordid, OrgEntityHeader, UserEntityHeader);
            if (result.Successful)
            {
                var buffer = System.Text.ASCIIEncoding.ASCII.GetBytes(result.Result.Item1);

                return File(buffer, "text/yaml", result.Result.Item2);
            }
            else
            {
                throw new Exception(result.ErrorMessage);
            }
         }

            /// <summary>
            /// YAML upload a new Object.
            /// </summary>
            /// <param name="recordType"></param>
            /// <param name="file"></param>
            /// <returns></returns>
            [HttpPost("/api/dataservices/yaml/{recordType}/import")]
        public async Task<InvokeResult<object>> ApplyYamlAsync(string recordType, IFormFile file)
        {
            using (var strm = file.OpenReadStream())
            {
                return await _yamlServices.DeserializeFromYamlAsync(recordType, strm, OrgEntityHeader, UserEntityHeader);
            }
        }

        /// <summary>
        /// Get all objects of a certain type for this organization.
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        [HttpGet("/api/dataservices/objects/{objectType}")]
        public async Task<ListResponse<EntityHeader>> GetObjectsOfType(string objectType)
        {
            var objects = await _dataServicesManager.GetAllObjectsOfType(objectType, OrgEntityHeader, UserEntityHeader);
            return ListResponse<EntityHeader>.Create(objects);
        }

        ///// <summary>
        ///// Import YAML objct
        ///// </summary>
        ///// <param name="yaml"></param>
        ///// <returns></returns>
        //[HttpGet("/api/dataservices/yaml/import")]
        //public async Task<InvokeResult> ImportObject(String yaml)
        //{
        //    return await _yamlServices.ApplyXaml(yaml, OrgEntityHeader, UserEntityHeader);
        //}
    }
}
