
using CoffeePOS.Shared.Dtos.Product;

namespace CoffeePOS.Services.Contracts.Commands;

public interface IProductService
{
    Task AddProductAsync(UpsertProductDto command);
    Task UpdateProductAsync(UpsertProductDto command);
    Task<bool> DeleteProductAsync(int productId);
    Task RestoreProductAsync(int productId);
}
