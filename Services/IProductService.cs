using CoffeePOS.Models;

namespace CoffeePOS.Services;

public interface IProductService
{
    Task AddProductAsync(Product product);
    Task UpdateProductAsync(Product product);
}
