using CoffeePOS.Models;

namespace CoffeePOS.Data.Repositories;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllCategoriesAsync();
    Task<Category?> GetCategoryByIdAsync(int id);
    Task AddCategoryAsync(Category category);
    Task UpdateCategoryAsync(Category category);
    Task<bool> DeleteCategoryAsync(int id);
    Task<List<Category>> GetDeletedCategoriesAsync();
    Task<Category?> GetDeletedCategoryByIdAsync(int categoryId);
    Task<bool> RestoreCategoryAsync(int categoryId);
}
