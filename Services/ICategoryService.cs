using CoffeePOS.Models;

namespace CoffeePOS.Services;

public interface ICategoryService
{
    Task<List<Category>> GetSelectableCategoriesAsync();
}
