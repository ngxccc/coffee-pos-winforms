using System.ComponentModel;

namespace CoffeePOS.Shared.Dtos;

public record UserGridDto(
    [property: DisplayName("Mã")] int Id,
    [property: DisplayName("Tài khoản")] string Username,
    [property: DisplayName("Họ tên")] string FullName,
    [property: Browsable(false)] int Role,
    [property: DisplayName("Vai trò")] string RoleName,
    [property: Browsable(false)] bool IsActive,
    [property: DisplayName("Trạng thái")] string Status
);
