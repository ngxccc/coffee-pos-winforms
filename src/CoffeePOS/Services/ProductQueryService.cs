using CoffeePOS.Data.Repositories.Contracts;
using CoffeePOS.Services.Contracts.Queries;
using CoffeePOS.Shared.Dtos;
using Microsoft.Extensions.Caching.Memory;

namespace CoffeePOS.Services;

public class ProductQueryService(IProductRepository productRepo, ICategoryRepository categoryRepo, IToppingRepository toppingRepo, IMemoryCache memoryCache) : IProductQueryService
{
    private const string TOPPINGS_CACHE_KEY = "ALL_TOPPINGS";
    private readonly TimeSpan CACHE_DURATION = TimeSpan.FromHours(24);

    public Task<List<ProductDetailDto>> GetAllProductsAsync() => productRepo.GetAllProductsAsync();

    public Task<ProductDetailDto?> GetProductByIdAsync(int productId)
    {
        if (productId <= 0) throw new ArgumentException("Sản phẩm không hợp lệ!");
        return productRepo.GetProductByIdAsync(productId);
    }

    public async Task<List<ProductGridDto>> GetProductGridAsync(bool isDeleted = false)
    {
        var allCategories = await categoryRepo.GetAllCategoriesAsync();
        var products = isDeleted
            ? await productRepo.GetDeletedProductsAsync()
            : await productRepo.GetAllProductsAsync();

        return [.. products
            .Select(p => new ProductGridDto(
                p.Id,
                p.Name,
                p.Price,
                allCategories.FirstOrDefault(c => c.Id == p.CategoryId)?.Name ?? "---",
                p.CategoryId,
                isDeleted,
                p.ImageUrl))];
    }

    public async Task<List<ToppingGridDto>> GetAllToppingsAsync()
    {
        if (memoryCache.TryGetValue(TOPPINGS_CACHE_KEY, out List<ToppingGridDto>? cachedToppings))
        {
            return cachedToppings ?? [];
        }

        var toppings = await toppingRepo.GetAllToppingsAsync();
        memoryCache.Set(TOPPINGS_CACHE_KEY, toppings, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CACHE_DURATION
        });

        return toppings;
    }
}
