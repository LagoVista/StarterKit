// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 4311df05235bbc90117c9e5d658322e751958cce47ae346369e636d51cab5ae1
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.IoT.StarterKit.Models;
using System.Threading.Tasks;

namespace LagoVista.IoT.StarterKit
{
    public interface IProductLineManager
    {
        Task<InvokeResult> AddProductLineAsync(ProductLine productLine, EntityHeader org, EntityHeader user);
        Task<InvokeResult> UpdateProductLineAsync(ProductLine productLine, EntityHeader org, EntityHeader user);
        Task<ListResponse<ProductLineSummary>> GetProductLinesAsync(ListRequest listRequest, EntityHeader org, EntityHeader user);
        Task<ProductLine> GetProductLineAsync(string id, EntityHeader org, EntityHeader user);
        Task<InvokeResult> DeleteProductLineAsync(string id, EntityHeader org, EntityHeader user);

    }
}
