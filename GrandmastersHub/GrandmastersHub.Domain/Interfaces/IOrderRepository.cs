using GrandmastersHub.Domain.Entities;

namespace GrandmastersHub.Domain.Interfaces;

public interface IOrderRepository
{
    Task<IEnumerable<Order>> GetAllAsync();

    Task<Order?> GetByIdAsync(int id);

    Task<IEnumerable<Order>> GetByUserIdAsync(int userId);

    Task<Order> AddAsync(Order order);

    Task UpdateAsync(Order order);

    Task DeleteAsync(int id);
}