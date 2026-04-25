using CoffeePOS.Shared.Dtos.Product;

namespace CoffeePOS.Services.Contracts.Queries;

public interface IToppingQueryService
{
    Task<List<ToppingDto>> GetToppingsAsync(bool isDeleted);
}
