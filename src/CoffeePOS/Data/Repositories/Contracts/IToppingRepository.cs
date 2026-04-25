using CoffeePOS.Shared.Dtos.Product;

namespace CoffeePOS.Data.Repositories.Contracts;

public interface IToppingRepository
{
    Task<List<ToppingDto>> GetToppingsAsync(bool isDeleted = false);
    Task<int> InsertToppingAsync(UpsertToppingDto dto);
    Task<bool> UpdateToppingAsync(UpsertToppingDto dto);
    Task<bool> SoftDeleteToppingAsync(int id);
    Task<bool> RestoreToppingAsync(int id);
}
