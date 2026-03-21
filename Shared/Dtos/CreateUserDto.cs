namespace CoffeePOS.Shared.Dtos;

public record CreateUserDto(
    string Username,
    string FullName,
    int Role,
    string Password,
    string ConfirmPassword
);
