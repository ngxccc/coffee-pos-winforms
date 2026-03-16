using CoffeePOS.Models;

namespace CoffeePOS.Data.Repositories;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllCategoriesAsync();
}
