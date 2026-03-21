using CoffeePOS.Services.Contracts.Commands;
using CoffeePOS.Services.Contracts.Queries;
using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Forms;

public class AddProductForm(IProductService productService, ICategoryQueryService categoryQueryService) : BaseProductForm(
    productService,
    categoryQueryService,
    "THÊM SẢN PHẨM MỚI",
    "LƯU",
    Color.FromArgb(46, 204, 113))
{
    protected override Task PersistAsync(UpsertProductDto command)
        => ProductService.AddProductAsync(command);

    protected override string SuccessMessage => "Thêm món mới thành công!";
}
