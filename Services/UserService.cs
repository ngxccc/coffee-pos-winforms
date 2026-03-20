using CoffeePOS.Data.Repositories.Contracts;
using CoffeePOS.Models;
using CoffeePOS.Services.Contracts.Commands;

namespace CoffeePOS.Services;

public class UserService(IUserRepository userRepo) : IUserService
{
    public async Task<User?> AuthenticateAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Vui lòng nhập đủ Username và Password!");

        return await userRepo.AuthenticateAsync(username.Trim(), password);
    }

    public async Task ChangePasswordAsync(int userId, string username, string currentPassword, string newPassword, string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            throw new ArgumentException("Vui lòng nhập đầy đủ các trường mật khẩu!");

        if (newPassword != confirmPassword)
            throw new ArgumentException("Mật khẩu mới và Xác nhận không khớp!");

        if (newPassword.Length < 6)
            throw new ArgumentException("Mật khẩu mới phải có ít nhất 6 ký tự!");

        var verifiedUser = await userRepo.AuthenticateAsync(username, currentPassword);
        if (verifiedUser == null)
            throw new InvalidOperationException("Mật khẩu hiện tại không chính xác!");

        string newHash = BCrypt.Net.BCrypt.HashPassword(newPassword, workFactor: 11);
        await userRepo.UpdatePasswordAsync(userId, newHash);
    }

    public Task DeactivateUserAsync(int adminId, int targetUserId) => userRepo.DeactivateUserAsync(adminId, targetUserId);
}
