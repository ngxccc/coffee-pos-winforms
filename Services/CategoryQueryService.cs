using CoffeePOS.Data.Repositories;
using CoffeePOS.Models;

namespace CoffeePOS.Services;

public class CategoryQueryService(ICategoryRepository categoryRepo) : ICategoryQueryService
{
    public Task<List<Category>> GetCategoryGridAsync(bool isDeleted = false)
    {
        return isDeleted
            ? categoryRepo.GetDeletedCategoriesAsync()
            : categoryRepo.GetAllCategoriesAsync();
    }
}
