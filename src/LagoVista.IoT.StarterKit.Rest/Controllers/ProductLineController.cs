using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.IoT.Deployment.Models;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.IoT.StarterKit.Models;
using LagoVista.IoT.Web.Common.Controllers;
using LagoVista.UserAdmin.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace LagoVista.IoT.StarterKit.Rest.Controllers
{

    [Authorize]
    public class ProductLineController : LagoVistaBaseController
    {
        private readonly IProductLineManager _productLineManager;

        public ProductLineController(IProductLineManager productLineManager, UserManager<AppUser> userManager, IAdminLogger logger) : base(userManager, logger)
        {
            this._productLineManager = productLineManager ?? throw new ArgumentNullException(nameof(productLineManager));
        }


        [HttpPost("/api/productline")]
        public Task<InvokeResult> AddProductLineAsync([FromBody] ProductLine errorCode)
        {
            return _productLineManager.AddProductLineAsync(errorCode, OrgEntityHeader, UserEntityHeader);
        }

        [HttpPut("/api/productline")]
        public Task<InvokeResult> UpdateProductLineAsync([FromBody] ProductLine errorCode)
        {
            SetUpdatedProperties(errorCode);
            return _productLineManager.UpdateProductLineAsync(errorCode, OrgEntityHeader, UserEntityHeader);
        }

        /// <summary>
        /// Device Error Code - Get Device Error Code
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("/api/productline/{id}")]
        public async Task<DetailResponse<ProductLine>> GetProductLineAsync(string id)
        {
            var productLine = await _productLineManager.GetProductLineAsync(id, OrgEntityHeader, UserEntityHeader);
            return DetailResponse<ProductLine>.Create(productLine);
        }

        [HttpGet("/api/productline/factory")]
        public DetailResponse<ProductLine> ProductLineFactory()
        {
            var errorCode = DetailResponse<ProductLine>.Create();
            SetAuditProperties(errorCode.Model);
            SetOwnedProperties(errorCode.Model);
            return errorCode;
        }


        [HttpGet("/api/productline/object/factory")]
        public DetailResponse<ProductLineObject> ProductLineObjectFactory()
        {
            return DetailResponse<ProductLineObject>.Create();
        }


        [HttpGet("/api/productline/todotemplate/factory")]
        public DetailResponse<ToDoTemplate> CreateToDoTemplate()
        {
            return DetailResponse<ToDoTemplate>.Create();
        }

       
        [HttpGet("/api/productlines")]
        public Task<ListResponse<ProductLineSummary>> GetErrorCodesForOrg()
        {
            return _productLineManager.GetProductLinesAsync(GetListRequestFromHeader(), OrgEntityHeader, UserEntityHeader);
        }

        [HttpDelete("/api/productline/{id}")]
        public async Task<InvokeResult> DeleteErrorCodeAsync(string id)
        {
            return await _productLineManager.DeleteProductLineAsync(id, OrgEntityHeader, UserEntityHeader);
        }

    }
}
