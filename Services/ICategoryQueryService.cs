using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Services;

public interface ICategoryQueryService
{
    Task<List<CategoryGridDto>> GetCategoryGridAsync(bool isDeleted = false);
}
