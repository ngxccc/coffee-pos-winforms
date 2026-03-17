using CoffeePOS.Models;

namespace CoffeePOS.Services;

public interface IProductService
{
    Task<List<Product>> GetAllProductsAsync();
    Task AddProductAsync(Product product);
    Task UpdateProductAsync(Product product);
    Task<bool> DeleteProductAsync(int productId);
}
