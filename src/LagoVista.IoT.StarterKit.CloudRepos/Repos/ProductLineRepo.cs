using LagoVista.CloudStorage.DocumentDB;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.IoT.Deployment.Admin.Repos;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.IoT.StarterKit;
using LagoVista.IoT.StarterKit.Interfaces;
using LagoVista.IoT.StarterKit.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.IoT.StarterKits.CloudRepos.Repos
{
    public class ProductLineRepo : DocumentDBRepoBase<ProductLine>, IProductLineRepo
    {

        private bool _shouldConsolidateCollections;
        public ProductLineRepo(IStarterKitConnection repoSettings, IAdminLogger logger)
            : base(repoSettings.StarterKitStorage.Uri, repoSettings.StarterKitStorage.AccessKey, repoSettings.StarterKitStorage.ResourceName, logger)
        {
            _shouldConsolidateCollections = repoSettings.ShouldConsolidateCollections;
        }

        protected override bool ShouldConsolidateCollections => _shouldConsolidateCollections;

        public Task AddProductLineAsync(ProductLine produceLine)
        {
            return CreateDocumentAsync(produceLine);
        }

        public Task DeleteProductLineAsync(string id)
        {
            return DeleteDocumentAsync(id);
        }

        public Task<ProductLine> GetProductLineAsync(string id)
        {
            return GetDocumentAsync(id);
        }

        public Task<ListResponse<ProductLineSummary>> GetProductLinesAsync(ListRequest listRequest, string orgId)
        {
            return QuerySummaryAsync<ProductLineSummary, ProductLine>(qry => qry.OwnerOrganization.Id == orgId, qry => qry.Name, listRequest);
        }

        public Task UpdateProductLineAsync(ProductLine productLine)
        {
            return UpsertDocumentAsync(productLine);
        }
    }
}
