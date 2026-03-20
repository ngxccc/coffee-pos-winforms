using CoffeePOS.Data.Repositories.Contracts;
using CoffeePOS.Models;
using CoffeePOS.Services.Contracts.Commands;

namespace CoffeePOS.Services;

public class ProductService(IProductRepository productRepo, ICategoryRepository categoryRepo) : IProductService
{
    public async Task AddProductAsync(Product product)
    {
        ValidateCommonRules(product);

        var existingProducts = await productRepo.GetAllProductsAsync();
        if (existingProducts.Any(p => p.Name.Equals(product.Name, StringComparison.CurrentCultureIgnoreCase)))
            throw new InvalidOperationException($"Món '{product.Name}' đã tồn tại trong Menu!");

        product.Name = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(product.Name.ToLower());
        await productRepo.AddProductAsync(product);
    }

    public async Task UpdateProductAsync(Product product)
    {
        ValidateCommonRules(product);

        var existingProducts = await productRepo.GetAllProductsAsync();
        if (existingProducts.Any(p => p.Id != product.Id && p.Name.Equals(product.Name, StringComparison.CurrentCultureIgnoreCase)))
            throw new InvalidOperationException($"Không thể đổi tên. Món '{product.Name}' đã bị trùng với một món khác!");

        product.Name = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(product.Name.ToLower());
        await productRepo.UpdateProductAsync(product);
    }

    public Task<bool> DeleteProductAsync(int productId)
    {
        if (productId <= 0) throw new ArgumentException("Sản phẩm không hợp lệ!");
        return productRepo.DeleteProductAsync(productId);
    }

    private static void ValidateCommonRules(Product product)
    {
        if (string.IsNullOrWhiteSpace(product.Name)) throw new ArgumentException("Tên sản phẩm không được để trống!");
        if (product.CategoryId <= 0) throw new ArgumentException("Vui lòng chọn danh mục hợp lệ!");
        if (product.Price <= 0) throw new ArgumentException("Bán hàng từ thiện à? Giá phải lớn hơn 0!");
    }

    public async Task RestoreProductAsync(int productId)
    {
        if (productId <= 0) throw new ArgumentException("ID sản phẩm không hợp lệ!");

        var product = await productRepo.GetDeletedProductByIdAsync(productId) ?? throw new ArgumentException("Sản phẩm không tồn tại!");

        var category = await categoryRepo.GetCategoryByIdAsync(product.CategoryId) ?? throw new InvalidOperationException("Không thể khôi phục! Danh mục của món này đang bị xóa. Hãy khôi phục Danh mục trước!");

        var activeProducts = await productRepo.GetAllProductsAsync();
        if (activeProducts.Any(p => p.Name.Equals(product.Name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Tên món '{product.Name}' đã được sử dụng bởi một món đang bán!");

        await productRepo.RestoreProductAsync(productId);
    }
}
