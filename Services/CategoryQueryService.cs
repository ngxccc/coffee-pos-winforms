using CoffeePOS.Data.Repositories.Contracts;
using CoffeePOS.Models;
using CoffeePOS.Services.Contracts.Queries;
using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Services;

public class CategoryQueryService(ICategoryRepository categoryRepo) : ICategoryQueryService
{
    public Task<List<Category>> GetAllCategoriesAsync() => categoryRepo.GetAllCategoriesAsync();

    public Task<List<Category>> GetSelectableCategoriesAsync() => categoryRepo.GetAllCategoriesAsync();

    public Task<Category?> GetCategoryByIdAsync(int id) => categoryRepo.GetCategoryByIdAsync(id);

    public async Task<List<CategoryGridDto>> GetCategoryGridAsync(bool isDeleted = false)
    {
        var rawCategories = isDeleted
            ? await categoryRepo.GetDeletedCategoriesAsync()
            : await categoryRepo.GetAllCategoriesAsync();

        return [.. rawCategories.Select(c => new CategoryGridDto(c.Id, c.Name))];
    }
}
