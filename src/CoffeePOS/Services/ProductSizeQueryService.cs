using CoffeePOS.Data.Repositories.Contracts;
using CoffeePOS.Services.Contracts.Queries;
using CoffeePOS.Shared.Dtos.Product;

namespace CoffeePOS.Services;

public class ProductSizeQueryService(IProductSizeRepository repository) : IProductSizeQueryService
{
    private readonly IProductSizeRepository _repository = repository;

    // PERF: Time Complexity O(1) database lookup via indexed ProductId.
    // TODO: Tích hợp IMemoryCache tại đây để giảm tải Network I/O nếu scale up.
    public async Task<List<ProductSizeDto>> GetSizesByProductIdAsync(int productId)
    {
        return await _repository.GetSizesByProductIdAsync(productId);
    }
}
