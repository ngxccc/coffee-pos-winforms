using CoffeePOS.Data.Repositories.Contracts;
using CoffeePOS.Models;
using CoffeePOS.Services.Contracts.Commands;
using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Services;

public class ProductQueryService(IProductRepository productRepo, ICategoryRepository categoryRepo) : IProductQueryService
{
    public Task<List<Product>> GetAllProductsAsync() => productRepo.GetAllProductsAsync();

    public Task<Product?> GetProductByIdAsync(int productId)
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
                p.IsDeleted,
                p.ImageUrl))];
    }
}
