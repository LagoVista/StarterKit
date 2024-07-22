using LagoVista.Core.Models.UIMetaData;
using LagoVista.IoT.StarterKit.Models;
using System.Threading.Tasks;
using LagoVista.Core.Validation;

namespace LagoVista.IoT.StarterKit.Interfaces
{
    public interface IProductLineRepo
    {
        Task AddProductLineAsync(ProductLine produceLine);
        Task UpdateProductLineAsync(ProductLine productLine);
        Task<ListResponse<ProductLineSummary>> GetProductLinesAsync(ListRequest listRequest, string orgId);
        Task<ProductLine> GetProductLineAsync(string id);
        Task DeleteProductLineAsync(string id);
    }
}
