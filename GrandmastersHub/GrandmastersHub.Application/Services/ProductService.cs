using GrandmastersHub.Application.DTOs.Catalog;
using GrandmastersHub.Application.Interfaces;

namespace GrandmastersHub.Application.Services
{
    public class ProductService : IProductService
    {
        // Person 2 inject the database repository here.

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync() => new List<ProductDto>();
        public async Task<ProductDto?> GetProductByIdAsync(int id) => null;
        public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId) => new List<ProductDto>();
        public async Task<ProductDto> CreateProductAsync(ProductDto productDto) => productDto;
        public async Task<bool> UpdateProductAsync(int id, ProductDto productDto) => true;
        public async Task<bool> DeleteProductAsync(int id) => true;
    }
}