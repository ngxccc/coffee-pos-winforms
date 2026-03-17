using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Services;

public interface IProductQueryService
{
    Task<List<ProductGridDto>> GetProductGridAsync();
}
