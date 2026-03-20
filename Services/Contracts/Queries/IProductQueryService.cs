using CoffeePOS.Models;
using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Services.Contracts.Queries;

public interface IProductQueryService
{
    Task<List<Product>> GetAllProductsAsync();
    Task<Product?> GetProductByIdAsync(int productId);
    Task<List<ProductGridDto>> GetProductGridAsync(bool isDeleted = false);
}
