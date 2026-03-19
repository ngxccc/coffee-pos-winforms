using CoffeePOS.Data.Repositories;
using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Services;

public class CategoryQueryService(ICategoryRepository categoryRepo) : ICategoryQueryService
{
    public async Task<List<CategoryGridDto>> GetCategoryGridAsync(bool isDeleted = false)
    {
        var rawCategories = isDeleted
            ? await categoryRepo.GetDeletedCategoriesAsync()
            : await categoryRepo.GetAllCategoriesAsync();

        return [.. rawCategories.Select(c => new CategoryGridDto(c.Id, c.Name))];
    }
}
