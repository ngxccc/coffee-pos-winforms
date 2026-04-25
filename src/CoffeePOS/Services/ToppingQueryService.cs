using CoffeePOS.Data.Repositories.Contracts;
using CoffeePOS.Services.Contracts.Queries;
using CoffeePOS.Shared.Dtos.Product;

namespace CoffeePOS.Services;

public class ToppingQueryService(IToppingRepository repository) : IToppingQueryService
{
    private readonly IToppingRepository _repository = repository;

    public async Task<List<ToppingDto>> GetToppingsAsync(bool isDeleted)
    {
        return await _repository.GetToppingsAsync(isDeleted);
    }
}
