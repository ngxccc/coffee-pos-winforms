using CoffeePOS.Models;

namespace CoffeePOS.Data.Repositories;

public interface ICategoryRepository
{
    List<Category> GetCategories();
}
