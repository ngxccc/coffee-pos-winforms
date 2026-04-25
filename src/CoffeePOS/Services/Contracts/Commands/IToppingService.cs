using CoffeePOS.Shared.Dtos.Product;

namespace CoffeePOS.Services.Contracts.Commands;

public interface IToppingService
{
    Task<int> AddToppingAsync(UpsertToppingDto dto);
    Task<bool> UpdateToppingAsync(UpsertToppingDto dto);
    Task<bool> SoftDeleteToppingAsync(int id);
    Task<bool> RestoreToppingAsync(int id);
}
