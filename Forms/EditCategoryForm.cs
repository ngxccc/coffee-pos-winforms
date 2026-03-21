using CoffeePOS.Services.Contracts.Commands;
using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Forms;

public class EditCategoryForm(ICategoryService categoryService) : BaseCategoryForm(categoryService, "SỬA DANH MỤC", "CẬP NHẬT")
{
    protected override Task PersistAsync(UpsertCategoryDto command)
        => CategoryService.UpdateCategoryAsync(command);
}
