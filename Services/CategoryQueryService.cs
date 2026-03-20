using CoffeePOS.Data.Repositories.Contracts;
using CoffeePOS.Services.Contracts.Queries;
using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Services;

public class CategoryQueryService(ICategoryRepository categoryRepo) : ICategoryQueryService
{
    public async Task<List<CategoryOptionDto>> GetAllCategoriesAsync()
        => [.. (await categoryRepo.GetAllCategoriesAsync()).Select(c => new CategoryOptionDto(c.Id, c.Name))];

    public Task<List<CategoryOptionDto>> GetSelectableCategoriesAsync() => GetAllCategoriesAsync();

    public Task<CategoryDetailDto?> GetCategoryByIdAsync(int id)
        => categoryRepo.GetCategoryByIdAsync(id);

    public async Task<List<CategoryGridDto>> GetCategoryGridAsync(bool isDeleted = false)
    {
        var rawCategories = isDeleted
            ? await categoryRepo.GetDeletedCategoriesAsync()
            : await categoryRepo.GetAllCategoriesAsync();

        return [.. rawCategories.Select(c => new CategoryGridDto(c.Id, c.Name))];
    }
}
