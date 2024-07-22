using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.Validation;
using LagoVista.IoT.StarterKit.Interfaces;
using LagoVista.IoT.StarterKit.Models;
using System;
using System.Threading.Tasks;

namespace LagoVista.IoT.StarterKit.Managers
{
    public class ProductLineManager : ManagerBase, IProductLineManager
    {
        private readonly IProductLineRepo _productLineRepo;

        public ProductLineManager(IProductLineRepo productLineRepo, ILogger logger, IAppConfig appConfig, IDependencyManager dependencyManager, ISecurity security) :
            base(logger, appConfig, dependencyManager, security)
        {
            _productLineRepo = productLineRepo ?? throw new ArgumentNullException(nameof(productLineRepo));
        }

        public async Task<InvokeResult> AddProductLineAsync(ProductLine productLine, EntityHeader org, EntityHeader user)
        {
            ValidationCheck(productLine, Actions.Create);
            await _productLineRepo.AddProductLineAsync(productLine);
            return InvokeResult.Success;
        }

        public async Task<InvokeResult> DeleteProductLineAsync(string id, EntityHeader org, EntityHeader user)
        {
            var productLine = await _productLineRepo.GetProductLineAsync(id);
            await ConfirmNoDepenenciesAsync(productLine);
            await AuthorizeAsync(productLine, AuthorizeResult.AuthorizeActions.Delete, user, org);
            await _productLineRepo.DeleteProductLineAsync(id);
            return InvokeResult.Success;
        }

        public async Task<ProductLine> GetProductLineAsync(string id, EntityHeader org, EntityHeader user)
        {
            var productLine = await _productLineRepo.GetProductLineAsync(id);
            await AuthorizeAsync(productLine, AuthorizeResult.AuthorizeActions.Read, user, org);
            return productLine;
        }

        public async Task<ListResponse<ProductLineSummary>> GetProductLinesAsync(ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org.Id, typeof(ProductLine));
            return await _productLineRepo.GetProductLinesAsync(listRequest, org.Id);
        }

        public async Task<InvokeResult> UpdateProductLineAsync(ProductLine productLine, EntityHeader org, EntityHeader user)
        {
            ValidationCheck(productLine, Actions.Update);
            await _productLineRepo.UpdateProductLineAsync(productLine);
            return InvokeResult.Success;
        }
    }
}
