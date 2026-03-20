using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Services.Contracts.Commands;

public interface ICategoryService
{
    Task AddCategoryAsync(UpsertCategoryDto command);
    Task UpdateCategoryAsync(UpsertCategoryDto command);
    Task DeleteCategoryAsync(int id);
    Task RestoreCategoryAsync(int categoryId);
}
