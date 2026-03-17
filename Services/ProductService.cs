using CoffeePOS.Data.Repositories;
using CoffeePOS.Models;

namespace CoffeePOS.Services;

public class ProductService(IProductRepository productRepo) : IProductService
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

    private static void ValidateCommonRules(Product product)
    {
        if (string.IsNullOrWhiteSpace(product.Name)) throw new ArgumentException("Tên sản phẩm không được để trống!");
        if (product.CategoryId <= 0) throw new ArgumentException("Vui lòng chọn danh mục hợp lệ!");
        if (product.Price <= 0) throw new ArgumentException("Bán hàng từ thiện à? Giá phải lớn hơn 0!");
    }
}
