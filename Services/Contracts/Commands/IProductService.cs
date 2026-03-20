using CoffeePOS.Models;

namespace CoffeePOS.Services.Contracts.Commands;

public interface IProductService
{
    Task AddProductAsync(Product product);
    Task UpdateProductAsync(Product product);
    Task<bool> DeleteProductAsync(int productId);
    Task RestoreProductAsync(int productId);
}
