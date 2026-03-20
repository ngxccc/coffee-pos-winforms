using CoffeePOS.Models;

namespace CoffeePOS.Services;

public interface ICategoryService
{
    Task AddCategoryAsync(Category category);
    Task UpdateCategoryAsync(Category category);
    Task DeleteCategoryAsync(int id);
    Task RestoreCategoryAsync(int categoryId);
}
