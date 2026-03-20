using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Data.Repositories.Contracts;

public interface ICategoryRepository
{
    Task<List<CategoryDetailDto>> GetAllCategoriesAsync();
    Task<CategoryDetailDto?> GetCategoryByIdAsync(int id);
    Task AddCategoryAsync(UpsertCategoryDto command);
    Task UpdateCategoryAsync(UpsertCategoryDto command);
    Task<bool> DeleteCategoryAsync(int id);
    Task<List<CategoryDetailDto>> GetDeletedCategoriesAsync();
    Task<CategoryDetailDto?> GetDeletedCategoryByIdAsync(int categoryId);
    Task<bool> RestoreCategoryAsync(int categoryId);
}
