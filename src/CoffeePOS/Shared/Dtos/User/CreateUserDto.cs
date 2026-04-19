using CoffeePOS.Shared.Enums;

namespace CoffeePOS.Shared.Dtos.User;

public record CreateUserDto(
    string Username,
    string FullName,
    UserRole Role,
    string Password,
    string ConfirmPassword
);
