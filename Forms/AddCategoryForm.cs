using CoffeePOS.Services.Contracts.Commands;
using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Forms;

public class AddCategoryForm(ICategoryService categoryService) : BaseCategoryForm(categoryService, "THÊM DANH MỤC MỚI", "LƯU")
{
    protected override Task PersistAsync(UpsertCategoryDto command)
        => CategoryService.AddCategoryAsync(command);
}
