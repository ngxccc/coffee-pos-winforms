using CoffeePOS.Models;

namespace CoffeePOS.Data.Repositories;

public interface IProductRepository
{
    List<Category> GetCategories();
    List<Product> GetProducts();
}
