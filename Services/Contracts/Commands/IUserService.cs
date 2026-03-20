using CoffeePOS.Models;

namespace CoffeePOS.Services.Contracts.Commands;

public interface IUserService
{
    Task<User?> AuthenticateAsync(string username, string password);
    Task ChangePasswordAsync(int userId, string username, string currentPassword, string newPassword, string confirmPassword);
    Task DeactivateUserAsync(int adminId, int targetUserId);
}
