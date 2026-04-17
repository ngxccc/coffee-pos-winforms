using CoffeePOS.Shared.Dtos;
using CoffeePOS.Shared.Enums;

namespace CoffeePOS.Data.Repositories.Contracts;

public interface IUserRepository
{
    Task<List<UserGridDto>> GetAllUsersAsync();
    Task<AuthUserDto?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default);
    Task InsertUserAsync(string username, string fullName, UserRole role, string passwordHash);
    Task UpdateUserProfileAsync(int userId, string username, string fullName, UserRole role);
    Task SetUserActiveStatusAsync(int targetUserId, bool isActive);
    Task DeactivateUserAsync(int adminId, int targetUserId);
    Task UpdatePasswordAsync(int userId, string newPasswordHash);
}
