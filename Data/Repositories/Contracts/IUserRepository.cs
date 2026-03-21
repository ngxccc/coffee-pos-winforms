using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Data.Repositories.Contracts;

public interface IUserRepository
{
    Task<List<UserGridDto>> GetAllUsersAsync();
    Task<AuthUserDto?> AuthenticateAsync(string username, string password);
    Task InsertUserAsync(string username, string fullName, int role, string passwordHash);
    Task UpdateUserProfileAsync(int userId, string username, string fullName, int role);
    Task SetUserActiveStatusAsync(int targetUserId, bool isActive);
    Task DeactivateUserAsync(int adminId, int targetUserId);
    Task UpdatePasswordAsync(int userId, string newPasswordHash);
}
