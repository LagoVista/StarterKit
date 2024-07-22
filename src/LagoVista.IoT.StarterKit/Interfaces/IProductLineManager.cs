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
