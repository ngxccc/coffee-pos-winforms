using CoffeePOS.Models;

namespace CoffeePOS.Services;

public interface ICategoryService
{
    Task<List<Category>> GetAllCategoriesAsync();
    Task<List<Category>> GetSelectableCategoriesAsync();
}
