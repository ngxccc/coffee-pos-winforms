
using CoffeePOS.Shared.Dtos.Product;

namespace CoffeePOS.Data.Repositories.Contracts;

public interface IToppingRepository
{
    Task<List<ToppingGridDto>> GetAllToppingsAsync(bool isDeleted = false);
}
