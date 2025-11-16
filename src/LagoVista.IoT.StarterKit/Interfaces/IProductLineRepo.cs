// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 094a312c46a0e5f80c0c351f16e1a5eb9eda083955f029c67a64036ffdb3975c
// IndexVersion: 2
// --- END CODE INDEX META ---
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
