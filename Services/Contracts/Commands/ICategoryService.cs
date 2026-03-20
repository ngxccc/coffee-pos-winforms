using CoffeePOS.Models;

namespace CoffeePOS.Services.Contracts.Commands;

public interface ICategoryService
{
    Task AddCategoryAsync(Category category);
    Task UpdateCategoryAsync(Category category);
    Task DeleteCategoryAsync(int id);
    Task RestoreCategoryAsync(int categoryId);
}
