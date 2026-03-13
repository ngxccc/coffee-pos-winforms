using CoffeePOS.Models;

namespace CoffeePOS.Data.Repositories;

public interface IUserRepository
{
    Task<User?> AuthenticateAsync(string username, string password);
    Task DeactivateUserAsync(int adminId, int targetUserId);
}
