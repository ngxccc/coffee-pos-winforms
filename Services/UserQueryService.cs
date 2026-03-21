using CoffeePOS.Data.Repositories.Contracts;
using CoffeePOS.Services.Contracts.Queries;
using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Services;

public class UserQueryService(IUserRepository userRepo) : IUserQueryService
{
    public Task<List<UserGridDto>> GetUserGridAsync() => userRepo.GetAllUsersAsync();
}
