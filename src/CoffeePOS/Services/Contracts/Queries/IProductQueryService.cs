
using CoffeePOS.Shared.Dtos.Product;

namespace CoffeePOS.Services.Contracts.Queries;

public interface IProductQueryService
{
    Task<List<ProductDetailDto>> GetAllProductsAsync();
    Task<ProductDetailDto?> GetProductByIdAsync(int productId);
    Task<List<ProductGridDto>> GetProductGridAsync(bool isDeleted = false);
    Task<List<ToppingGridDto>> GetAllToppingsAsync();
}
