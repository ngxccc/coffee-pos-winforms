namespace CoffeePOS.Shared.Dtos.User;

public record ResetUserPasswordDto(
    int TargetUserId,
    string NewPassword,
    string ConfirmPassword
);
