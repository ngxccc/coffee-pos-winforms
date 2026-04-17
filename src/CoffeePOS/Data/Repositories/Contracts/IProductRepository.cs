using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Data.Repositories.Contracts;

public interface IProductRepository
{
    Task<List<ProductDetailDto>> GetAllProductsAsync();
    Task<ProductDetailDto?> GetProductByIdAsync(int productId);
    Task AddProductAsync(UpsertProductDto command);
    Task UpdateProductAsync(UpsertProductDto command);
    Task<bool> DeleteProductAsync(int productId);
    Task<List<ProductDetailDto>> GetDeletedProductsAsync();
    Task<ProductDetailDto?> GetDeletedProductByIdAsync(int productId);
    Task<bool> RestoreProductAsync(int productId);
}
