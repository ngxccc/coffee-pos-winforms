using CoffeePOS.Data.Repositories.Contracts;
using CoffeePOS.Services.Contracts.Commands;
using CoffeePOS.Shared.Dtos.Product;

namespace CoffeePOS.Services;

public class ToppingService(IToppingRepository repository) : IToppingService
{
    private readonly IToppingRepository _repository = repository;

    public async Task<int> AddToppingAsync(UpsertToppingDto dto)
    {
        ValidateTopping(dto);
        return await _repository.InsertToppingAsync(dto);
    }

    public async Task<bool> UpdateToppingAsync(UpsertToppingDto dto)
    {
        ValidateTopping(dto);
        return await _repository.UpdateToppingAsync(dto);
    }

    public async Task<bool> SoftDeleteToppingAsync(int id)
    {
        return await _repository.SoftDeleteToppingAsync(id);
    }

    public async Task<bool> RestoreToppingAsync(int id)
    {
        return await _repository.RestoreToppingAsync(id);
    }

    // WHY: Centralized validation. Fail-fast approach to protect database integrity.
    private static void ValidateTopping(UpsertToppingDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Tên Topping không được để trống.");

        if (dto.Price < 0)
            throw new ArgumentException("Giá Topping không được âm.");
    }
}
