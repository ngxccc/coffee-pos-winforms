using CoffeePOS.Shared.Dtos.Product;

namespace CoffeePOS.Services.Contracts.Commands;

public interface IProductSizeService
{
    Task<int> AddProductSizeAsync(UpsertProductSizeDto dto);
    Task<bool> UpdateProductSizeAsync(UpsertProductSizeDto dto);
    Task<bool> DeleteProductSizeAsync(int id);
}
