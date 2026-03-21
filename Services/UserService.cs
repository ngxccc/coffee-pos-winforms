using CoffeePOS.Data.Repositories.Contracts;
using CoffeePOS.Services.Contracts.Commands;
using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Services;

public class UserService(IUserRepository userRepo) : IUserService
{
    public async Task<AuthUserDto?> AuthenticateAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Vui lòng nhập đủ Username và Password!");

        return await userRepo.AuthenticateAsync(username.Trim(), password);
    }

    public async Task AddUserAsync(CreateUserDto command)
    {
        string username = command.Username.Trim();
        string fullName = command.FullName.Trim();

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Vui lòng nhập đầy đủ tài khoản và họ tên!");

        if (command.Role is not (0 or 1))
            throw new ArgumentException("Vai trò không hợp lệ!");

        if (command.Password.Length < 6)
            throw new ArgumentException("Mật khẩu phải có ít nhất 6 ký tự!");

        if (command.Password != command.ConfirmPassword)
            throw new ArgumentException("Mật khẩu và xác nhận mật khẩu không khớp!");

        string passwordHash = BCrypt.Net.BCrypt.HashPassword(command.Password, workFactor: 11);
        await userRepo.InsertUserAsync(username, fullName, command.Role, passwordHash);
    }

    public async Task ChangePasswordAsync(int userId, string username, string currentPassword, string newPassword, string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            throw new ArgumentException("Vui lòng nhập đầy đủ các trường mật khẩu!");

        if (newPassword != confirmPassword)
            throw new ArgumentException("Mật khẩu mới và Xác nhận không khớp!");

        if (newPassword.Length < 6)
            throw new ArgumentException("Mật khẩu mới phải có ít nhất 6 ký tự!");

        var verifiedUser = await userRepo.AuthenticateAsync(username, currentPassword) ?? throw new InvalidOperationException("Mật khẩu hiện tại không chính xác!");
        string newHash = BCrypt.Net.BCrypt.HashPassword(newPassword, workFactor: 11);
        await userRepo.UpdatePasswordAsync(userId, newHash);
    }

    public async Task ResetUserPasswordAsync(int adminId, ResetUserPasswordDto command)
    {
        if (command.TargetUserId <= 0)
            throw new ArgumentException("Người dùng không hợp lệ!");

        if (adminId == command.TargetUserId)
            throw new InvalidOperationException("Không thể reset mật khẩu tài khoản đang đăng nhập ở màn hình này.");

        if (string.IsNullOrWhiteSpace(command.NewPassword) || string.IsNullOrWhiteSpace(command.ConfirmPassword))
            throw new ArgumentException("Vui lòng nhập đầy đủ mật khẩu mới và xác nhận.");

        if (command.NewPassword.Length < 6)
            throw new ArgumentException("Mật khẩu mới phải có ít nhất 6 ký tự!");

        if (command.NewPassword != command.ConfirmPassword)
            throw new ArgumentException("Mật khẩu mới và xác nhận không khớp!");

        string newHash = BCrypt.Net.BCrypt.HashPassword(command.NewPassword, workFactor: 11);
        await userRepo.UpdatePasswordAsync(command.TargetUserId, newHash);
    }

    public async Task SetUserActiveStatusAsync(int adminId, int targetUserId, bool isActive)
    {
        if (targetUserId <= 0)
            throw new ArgumentException("Người dùng không hợp lệ!");

        if (!isActive && adminId == targetUserId)
            throw new InvalidOperationException("Không thể tự khóa tài khoản của chính bạn!");

        await userRepo.SetUserActiveStatusAsync(targetUserId, isActive);
    }

    public Task DeactivateUserAsync(int adminId, int targetUserId) => userRepo.DeactivateUserAsync(adminId, targetUserId);
}
