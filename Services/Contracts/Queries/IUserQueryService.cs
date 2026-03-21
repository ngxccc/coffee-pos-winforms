using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Services.Contracts.Queries;

public interface IUserQueryService
{
    Task<List<UserGridDto>> GetUserGridAsync();
}
