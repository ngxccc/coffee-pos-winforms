using CoffeePOS.Data.Repositories.Contracts;
using CoffeePOS.Services.Contracts.Queries;
using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Services;

public class ProductQueryService(IProductRepository productRepo, ICategoryRepository categoryRepo) : IProductQueryService
{
    public async Task<List<ProductDetailDto>> GetAllProductsAsync()
    {
        var products = await productRepo.GetAllProductsAsync();
        return [.. products.Select(p => new ProductDetailDto(
            p.Id,
            p.Name,
            p.Price,
            p.CategoryId,
            p.ImageUrl))];
    }

    public async Task<ProductDetailDto?> GetProductByIdAsync(int productId)
    {
        if (productId <= 0) throw new ArgumentException("Sản phẩm không hợp lệ!");

        var product = await productRepo.GetProductByIdAsync(productId);
        return product is null
            ? null
            : new ProductDetailDto(
                product.Id,
                product.Name,
                product.Price,
                product.CategoryId,
                product.ImageUrl);
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
