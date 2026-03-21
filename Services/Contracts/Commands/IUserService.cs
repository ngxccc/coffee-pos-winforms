using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Services.Contracts.Commands;

public interface IUserService
{
    Task<AuthUserDto?> AuthenticateAsync(string username, string password);
    Task AddUserAsync(CreateUserDto command);
    Task ChangePasswordAsync(int userId, string username, string currentPassword, string newPassword, string confirmPassword);
    Task ResetUserPasswordAsync(int adminId, ResetUserPasswordDto command);
    Task SetUserActiveStatusAsync(int adminId, int targetUserId, bool isActive);
    Task DeactivateUserAsync(int adminId, int targetUserId);
}
