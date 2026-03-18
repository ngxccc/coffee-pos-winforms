using CoffeePOS.Models;

namespace CoffeePOS.Services;

public interface ICategoryQueryService
{
    Task<List<Category>> GetCategoryGridAsync(bool isDeleted = false);
}
