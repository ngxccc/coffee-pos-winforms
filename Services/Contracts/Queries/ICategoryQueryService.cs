using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Services.Contracts.Queries;

public interface ICategoryQueryService
{
    Task<List<CategoryOptionDto>> GetAllCategoriesAsync();
    Task<List<CategoryOptionDto>> GetSelectableCategoriesAsync();
    Task<CategoryDetailDto?> GetCategoryByIdAsync(int id);
    Task<List<CategoryGridDto>> GetCategoryGridAsync(bool isDeleted = false);
}
