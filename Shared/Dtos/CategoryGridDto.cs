using System.ComponentModel;

namespace CoffeePOS.Shared.Dtos;

// record trong C# 9+
// chuyên dùng để khai báo các kiểu dữ liệu Bất biến (Immutable)
public record CategoryGridDto(
    [property: DisplayName("Mã")] int Id,
    [property: DisplayName("Tên Danh Mục")] string Name
);
