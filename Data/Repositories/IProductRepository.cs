using CoffeePOS.Models;

namespace CoffeePOS.Data.Repositories;

public interface IProductRepository
{
    Task<List<Product>> GetAllProductsAsync();
    Task<bool> DeleteProductAsync(int productId);
}
