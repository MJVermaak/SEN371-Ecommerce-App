using GrandmastersHub.Application.DTOs.Catalog;
using GrandmastersHub.Application.Interfaces;
using GrandmastersHub.Application.Helpers;
using GrandmastersHub.Domain.Interfaces;
using GrandmastersHub.Domain.Entities;

namespace GrandmastersHub.Application.Services
{
    public class ProductService : IProductService
    {
        // Person 2 inject the database repository here.
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }
        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync() => new List<ProductDto>();
        public async Task<ProductDto?> GetProductByIdAsync(int id) => null;
        public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId) => new List<ProductDto>();
        public async Task<ProductDto> CreateProductAsync(ProductDto productDto)
        {
            if (!ValidationHelper.IsValidPrice(productDto.Price))
                throw new ArgumentException("Price must be greater than zero.");

            if (!ValidationHelper.IsValidStock(productDto.StockQuantity))
                throw new ArgumentException("Stock cannot be negative.");

            var cleanName = InputSanitizer.Clean(productDto.Name);
            var slug = cleanName.ToLower().Replace(" ", "-");

            var product = new Product
            {
                Name = cleanName,
                Slug = slug,
                Description = InputSanitizer.Clean(productDto.Description),
                Price = productDto.Price,
                CategoryId = productDto.CategoryId
            };

            var createdProduct = await _productRepository.AddAsync(product);

            productDto.ProductId = createdProduct.ProductId;

            return productDto;
        }
        public async Task<bool> UpdateProductAsync(int id, ProductDto productDto)
        {
            if (!ValidationHelper.IsValidPrice(productDto.Price))
                throw new ArgumentException("Price must be greater than zero.");

            if (!ValidationHelper.IsValidStock(productDto.StockQuantity))
                throw new ArgumentException("Stock cannot be negative.");

            var existingProduct = await _productRepository.GetByIdAsync(id);

            if (existingProduct == null)
                return false;

            existingProduct.Name = InputSanitizer.Clean(productDto.Name);
            existingProduct.Slug = InputSanitizer.Clean(productDto.Name).ToLower().Replace(" ", "-");
            existingProduct.Description = InputSanitizer.Clean(productDto.Description);
            existingProduct.Price = productDto.Price;
            existingProduct.CategoryId = productDto.CategoryId;

            await _productRepository.UpdateAsync(existingProduct);

            return true;
        }
        public async Task<bool> DeleteProductAsync(int id) => true;
    }
}