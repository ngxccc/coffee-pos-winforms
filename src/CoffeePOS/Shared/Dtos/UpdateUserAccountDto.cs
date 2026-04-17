using CoffeePOS.Shared.Enums;

namespace CoffeePOS.Shared.Dtos;

public record UpdateUserAccountDto(
    int TargetUserId,
    string Username,
    string FullName,
    UserRole Role,
    string NewPassword,
    string ConfirmPassword
);
