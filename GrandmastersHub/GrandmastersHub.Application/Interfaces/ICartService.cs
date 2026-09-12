using GrandmastersHub.Application.DTOs.Cart;

namespace GrandmastersHub.Application.Interfaces;

public interface ICartService
{
    Task<CartDto> GetAsync(int userId);
    Task<CartDto> AddAsync(int userId, AddCartItemRequest request);
    Task<CartDto> UpdateAsync(int userId, int itemId, int quantity);
    Task<CartDto> RemoveAsync(int userId, int itemId);
}
