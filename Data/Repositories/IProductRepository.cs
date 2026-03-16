using CoffeePOS.Models;

namespace CoffeePOS.Data.Repositories;

public interface IProductRepository
{
    Task<List<Product>> GetAllProductsAsync();
    Task AddProductAsync(Product product);
    Task UpdateProductAsync(Product product);
    Task<bool> DeleteProductAsync(int productId);
}
