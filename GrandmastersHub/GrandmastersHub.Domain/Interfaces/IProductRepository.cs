using GrandmastersHub.Domain.Entities;

namespace GrandmastersHub.Domain.Interfaces;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync();

    Task<Product?> GetByIdAsync(int id);

    Task<Product?> GetBySlugAsync(string slug);

    Task<Product> AddAsync(Product product);

    Task UpdateAsync(Product product);

    Task DeleteAsync(int id);
}