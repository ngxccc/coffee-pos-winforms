using CoffeePOS.Models;
using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Services;

public interface ICategoryQueryService
{
    Task<List<Category>> GetAllCategoriesAsync();
    Task<List<Category>> GetSelectableCategoriesAsync();
    Task<Category?> GetCategoryByIdAsync(int id);
    Task<List<CategoryGridDto>> GetCategoryGridAsync(bool isDeleted = false);
}
