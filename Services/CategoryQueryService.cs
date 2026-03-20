using CoffeePOS.Data.Repositories.Contracts;
using CoffeePOS.Services.Contracts.Queries;
using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Services;

public class CategoryQueryService(ICategoryRepository categoryRepo) : ICategoryQueryService
{
    public async Task<List<CategoryOptionDto>> GetAllCategoriesAsync()
    {
        var categories = await categoryRepo.GetAllCategoriesAsync();
        return [.. categories.Select(c => new CategoryOptionDto(c.Id, c.Name))];
    }

    public Task<List<CategoryOptionDto>> GetSelectableCategoriesAsync() => GetAllCategoriesAsync();

    public async Task<CategoryDetailDto?> GetCategoryByIdAsync(int id)
    {
        var category = await categoryRepo.GetCategoryByIdAsync(id);
        return category is null ? null : new CategoryDetailDto(category.Id, category.Name);
    }

    public async Task<List<CategoryGridDto>> GetCategoryGridAsync(bool isDeleted = false)
    {
        var rawCategories = isDeleted
            ? await categoryRepo.GetDeletedCategoriesAsync()
            : await categoryRepo.GetAllCategoriesAsync();

        return [.. rawCategories.Select(c => new CategoryGridDto(c.Id, c.Name))];
    }
}
