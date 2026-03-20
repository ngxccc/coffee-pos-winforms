using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Services.Contracts.Commands;

public interface IUserService
{
    Task<AuthUserDto?> AuthenticateAsync(string username, string password);
    Task ChangePasswordAsync(int userId, string username, string currentPassword, string newPassword, string confirmPassword);
    Task DeactivateUserAsync(int adminId, int targetUserId);
}
