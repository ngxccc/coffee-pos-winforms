using CoffeePOS.Models;

namespace CoffeePOS.Data.Repositories;

public interface IProductRepository
{
    List<Product> GetProducts();
}
