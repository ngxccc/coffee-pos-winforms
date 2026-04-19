
using CoffeePOS.Shared.Dtos.User;

namespace CoffeePOS.Services.Contracts.Queries;

public interface IUserQueryService
{
    Task<List<UserGridDto>> GetUserGridAsync();
}
