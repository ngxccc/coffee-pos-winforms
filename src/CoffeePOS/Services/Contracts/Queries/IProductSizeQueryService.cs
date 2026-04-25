using CoffeePOS.Shared.Dtos.Product;

namespace CoffeePOS.Services.Contracts.Queries;

public interface IProductSizeQueryService
{
    Task<List<ProductSizeDto>> GetSizesByProductIdAsync(int productId);
}
