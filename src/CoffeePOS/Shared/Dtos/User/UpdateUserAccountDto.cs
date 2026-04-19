using CoffeePOS.Shared.Enums;

namespace CoffeePOS.Shared.Dtos.User;

public record UpdateUserAccountDto(
    int TargetUserId,
    string Username,
    string FullName,
    UserRole Role,
    string NewPassword,
    string ConfirmPassword
);
