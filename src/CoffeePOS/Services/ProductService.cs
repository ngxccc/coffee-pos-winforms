using CoffeePOS.Data.Repositories.Contracts;
using CoffeePOS.Services.Contracts.Commands;
using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Services;

public class ProductService(IProductRepository productRepo, ICategoryRepository categoryRepo) : IProductService
{
    public async Task AddProductAsync(UpsertProductDto command)
    {
        ValidateCommonRules(command);

        var existingProducts = await productRepo.GetAllProductsAsync();
        if (existingProducts.Any(p => p.Name.Equals(command.Name, StringComparison.CurrentCultureIgnoreCase)))
            throw new InvalidOperationException($"Món '{command.Name}' đã tồn tại trong Menu!");

        string normalizedName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(command.Name.ToLower());
        await productRepo.AddProductAsync(command with { Name = normalizedName });
    }

    public async Task UpdateProductAsync(UpsertProductDto command)
    {
        ValidateCommonRules(command);

        var existingProducts = await productRepo.GetAllProductsAsync();
        if (existingProducts.Any(p => p.Id != command.Id && p.Name.Equals(command.Name, StringComparison.CurrentCultureIgnoreCase)))
            throw new InvalidOperationException($"Không thể đổi tên. Món '{command.Name}' đã bị trùng với một món khác!");

        string normalizedName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(command.Name.ToLower());
        await productRepo.UpdateProductAsync(command with { Name = normalizedName });
    }

    public Task<bool> DeleteProductAsync(int productId)
    {
        if (productId <= 0) throw new ArgumentException("Sản phẩm không hợp lệ!");
        return productRepo.DeleteProductAsync(productId);
    }

    private static void ValidateCommonRules(UpsertProductDto command)
    {
        if (string.IsNullOrWhiteSpace(command.Name)) throw new ArgumentException("Tên sản phẩm không được để trống!");
        if (command.CategoryId <= 0) throw new ArgumentException("Vui lòng chọn danh mục hợp lệ!");
        if (command.Price <= 0) throw new ArgumentException("Bán hàng từ thiện à? Giá phải lớn hơn 0!");
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
