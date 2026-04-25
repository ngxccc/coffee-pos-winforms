using CoffeePOS.Shared.Dtos.Product;

namespace CoffeePOS.Data.Repositories.Contracts;

public interface IProductSizeRepository
{
    Task<List<ProductSizeDto>> GetSizesByProductIdAsync(int productId);
    Task<int> InsertSizeAsync(UpsertProductSizeDto dto);
    Task<bool> UpdateSizeAsync(UpsertProductSizeDto dto);
    Task<bool> DeleteSizeAsync(int id);
}
