using GrandmastersHub.Application.DTOs.Catalog;
using GrandmastersHub.Application.Interfaces;

namespace GrandmastersHub.Application.Services
{
    public class CategoryService : ICategoryService
    {
        // Person 2 inject the database repository here.
        
        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync() => new List<CategoryDto>();
        public async Task<CategoryDto?> GetCategoryByIdAsync(int id) => null;
        public async Task<CategoryDto> CreateCategoryAsync(CategoryDto categoryDto) => categoryDto;
        public async Task<bool> UpdateCategoryAsync(int id, CategoryDto categoryDto) => true;
        public async Task<bool> DeleteCategoryAsync(int id) => true;
    }
}