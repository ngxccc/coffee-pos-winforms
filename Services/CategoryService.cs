using CoffeePOS.Data.Repositories.Contracts;
using CoffeePOS.Models;
using CoffeePOS.Services.Contracts.Commands;
using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Services;

public class CategoryService(ICategoryRepository categoryRepo) : ICategoryService
{
    public async Task AddCategoryAsync(UpsertCategoryDto command)
    {
        var category = MapToCategory(command);
        if (string.IsNullOrWhiteSpace(category.Name)) throw new ArgumentException("Tên danh mục không được để trống!");

        var all = await categoryRepo.GetAllCategoriesAsync();
        if (all.Any(c => c.Name.Equals(category.Name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Danh mục này đã tồn tại!");

        category.Name = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(category.Name.ToLower());
        await categoryRepo.AddCategoryAsync(category);
    }

    public async Task UpdateCategoryAsync(UpsertCategoryDto command)
    {
        var category = MapToCategory(command);
        if (string.IsNullOrWhiteSpace(category.Name)) throw new ArgumentException("Tên danh mục không được để trống!");

        var all = await categoryRepo.GetAllCategoriesAsync();
        // Khác ID nhưng trùng tên
        if (all.Any(c => c.Id != category.Id && c.Name.Equals(category.Name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Tên danh mục đã bị trùng với danh mục khác!");

        category.Name = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(category.Name.ToLower());
        await categoryRepo.UpdateCategoryAsync(category);
    }

    public async Task DeleteCategoryAsync(int id)
    {
        if (id <= 0) throw new ArgumentException("ID không hợp lệ!");
        await categoryRepo.DeleteCategoryAsync(id);
    }

    public async Task RestoreCategoryAsync(int categoryId)
    {
        if (categoryId <= 0) throw new ArgumentException("ID danh mục không hợp lệ!");

        var category = await categoryRepo.GetDeletedCategoryByIdAsync(categoryId)
            ?? throw new ArgumentException("Danh mục không tồn tại hoặc chưa bị xóa!");

        var activeCategories = await categoryRepo.GetAllCategoriesAsync();
        if (activeCategories.Any(c => c.Name.Equals(category.Name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Tên danh mục '{category.Name}' đã được sử dụng bởi một danh mục đang hoạt động!");

        await categoryRepo.RestoreCategoryAsync(categoryId);
    }

    private static Category MapToCategory(UpsertCategoryDto command)
    {
        return new Category
        {
            Id = command.Id,
            Name = command.Name
        };
    }
}
