
using CoffeePOS.Shared.Dtos.User;

namespace CoffeePOS.Services.Contracts.Commands;

public interface IUserService
{
    Task<AuthUserDto?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default);
    Task AddUserAsync(CreateUserDto command);
    Task ChangePasswordAsync(int userId, string username, string currentPassword, string newPassword, string confirmPassword);
    Task UpdateUserAccountAsync(int adminId, UpdateUserAccountDto command);
    Task SetUserActiveStatusAsync(int adminId, int targetUserId, bool isActive);
    Task DeactivateUserAsync(int adminId, int targetUserId);
}
