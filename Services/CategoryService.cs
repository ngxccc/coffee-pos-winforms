using CoffeePOS.Data.Repositories;
using CoffeePOS.Models;

namespace CoffeePOS.Services;

public class CategoryService(ICategoryRepository categoryRepo) : ICategoryService
{
    public Task<List<Category>> GetAllCategoriesAsync() => categoryRepo.GetAllCategoriesAsync();

    public async Task<List<Category>> GetSelectableCategoriesAsync()
    {
        var categories = await categoryRepo.GetAllCategoriesAsync();
        return [.. categories.Where(category => category.Id > 0)];
    }
}
