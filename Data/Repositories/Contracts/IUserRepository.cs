using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Data.Repositories.Contracts;

public interface IUserRepository
{
    Task<AuthUserDto?> AuthenticateAsync(string username, string password);
    Task DeactivateUserAsync(int adminId, int targetUserId);
    Task UpdatePasswordAsync(int userId, string newPasswordHash);
}
