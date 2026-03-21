using CoffeePOS.Services.Contracts.Commands;
using CoffeePOS.Services.Contracts.Queries;
using CoffeePOS.Shared.Dtos;
namespace CoffeePOS.Forms;

public class EditProductForm(IProductService productService, ICategoryQueryService categoryQueryService) : BaseProductForm(productService, categoryQueryService, "CẬP NHẬT SẢN PHẨM", "CẬP NHẬT", Color.FromArgb(46, 204, 113))
{
    public void LoadProductDetails(ProductDetailDto product)
        => LoadProductInternal(product, $"CẬP NHẬT SẢN PHẨM: {product.Name}");

    protected override Task PersistAsync(UpsertProductDto command)
        => ProductService.UpdateProductAsync(command);

    protected override Task AfterPersistAsync()
    {
        TryDeletePreviousImage();
        return Task.CompletedTask;
    }

    protected override string SuccessMessage => "Cập nhật món thành công!";
}
