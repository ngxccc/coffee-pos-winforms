using CoffeePOS.Shared.Enums;

namespace CoffeePOS.Shared.Dtos;

public record CreateUserDto(
    string Username,
    string FullName,
    UserRole Role,
    string Password,
    string ConfirmPassword
);
