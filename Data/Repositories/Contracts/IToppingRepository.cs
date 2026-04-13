using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Data.Repositories.Contracts;

public interface IToppingRepository
{
    Task<List<ToppingGridDto>> GetAllToppingsAsync(bool isDeleted = false);
}
