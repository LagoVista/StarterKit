// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 31ff94b03e700e6539f1f7df1f2c9c55b81a9f7fdfe55384038298d409451459
// IndexVersion: 2
// --- END CODE INDEX META ---
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

    /// <summary>
    /// Product lines are used to help group products together.  They can be used to drive common objects and to do templates across a set of products.  For example you might have a product line for "Smart Thermostats" that includes multiple products from different manufacturers, but they all share the same set of objects and to do templates.
    /// </summary>
    [Authorize]
    public class ProductLineController : LagoVistaBaseController
    {
        private readonly IProductLineManager _productLineManager;

        /// <summary>
        /// Create a product line instance.
        /// </summary>
        /// <param name="productLineManager"></param>
        /// <param name="userManager"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ProductLineController(IProductLineManager productLineManager, UserManager<AppUser> userManager, IAdminLogger logger) : base(userManager, logger)
        {
            this._productLineManager = productLineManager ?? throw new ArgumentNullException(nameof(productLineManager));
        }

        /// <summary>
        /// Add a new product line
        /// </summary>
        /// <param name="errorCode"></param>
        /// <returns></returns>
        [HttpPost("/api/productline")]
        public Task<InvokeResult> AddProductLineAsync([FromBody] ProductLine errorCode)
        {
            return _productLineManager.AddProductLineAsync(errorCode, OrgEntityHeader, UserEntityHeader);
        }

        /// <summary>
        /// Update product line
        /// </summary>
        /// <param name="errorCode"></param>
        /// <returns></returns>
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


        /// <summary>
        /// create product line
        /// </summary>
        /// <returns></returns>
        [HttpGet("/api/productline/factory")]
        public DetailResponse<ProductLine> ProductLineFactory()
        {
            var errorCode = DetailResponse<ProductLine>.Create();
            SetAuditProperties(errorCode.Model);
            SetOwnedProperties(errorCode.Model);
            return errorCode;
        }

        /// <summary>
        /// Create product line object
        /// </summary>
        /// <returns></returns>
        [HttpGet("/api/productline/object/factory")]
        public DetailResponse<ProductLineObject> ProductLineObjectFactory()
        {
            return DetailResponse<ProductLineObject>.Create();
        }


        /// <summary>
        /// Create to do template
        /// </summary>
        /// <returns></returns>
        [HttpGet("/api/productline/todotemplate/factory")]
        public DetailResponse<ToDoTemplate> CreateToDoTemplate()
        {
            return DetailResponse<ToDoTemplate>.Create();
        }

       /// <summary>
       /// Get proudct lines for org.
       /// </summary>
       /// <returns></returns>
        [HttpGet("/api/productlines")]
        public Task<ListResponse<ProductLineSummary>> GetProductLinesForFor()
        {
            return _productLineManager.GetProductLinesAsync(GetListRequestFromHeader(), OrgEntityHeader, UserEntityHeader);
        }

        /// <summary>
        /// Delte product line
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("/api/productline/{id}")]
        public async Task<InvokeResult> DeleteProductLineAsync(string id)
        {
            return await _productLineManager.DeleteProductLineAsync(id, OrgEntityHeader, UserEntityHeader);
        }

    }
}
