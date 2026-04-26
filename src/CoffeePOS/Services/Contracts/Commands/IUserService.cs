
using CoffeePOS.Features.Sidebar;
using CoffeePOS.Shared.Dtos.User;
using CoffeePOS.Shared.Enums;

namespace CoffeePOS.Services.Contracts.Commands;

public interface IUserService
{
    Task<AuthUserDto?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default);
    Task AddUserAsync(CreateUserDto command);
    Task ChangePasswordAsync(int userId, string username, ChangePasswordPayload payload);
    Task UpdateUserAccountAsync(int adminId, UpdateUserAccountDto command);
    Task SetUserActiveStatusAsync(int adminId, int targetUserId, UserRole userRole, bool isActive);
    Task DeactivateUserAsync(int adminId, int targetUserId);
}
