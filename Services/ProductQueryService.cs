using CoffeePOS.Data.Repositories;
using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Services;

public class ProductQueryService(IProductRepository productRepo, ICategoryRepository categoryRepo) : IProductQueryService
{
    public async Task<List<ProductGridDto>> GetProductGridAsync(bool isDeleted = false)
    {
        var allCategories = await categoryRepo.GetAllCategoriesAsync();
        var products = isDeleted
            ? await productRepo.GetDeletedProductsAsync()
            : await productRepo.GetAllProductsAsync();

        return [.. products
            .Select(p => new ProductGridDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                CategoryId = p.CategoryId,
                CategoryName = allCategories.FirstOrDefault(c => c.Id == p.CategoryId)?.Name ?? "---"
            })];
    }
}
