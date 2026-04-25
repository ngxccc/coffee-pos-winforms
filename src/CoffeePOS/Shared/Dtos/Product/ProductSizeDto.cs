using System.ComponentModel;

namespace CoffeePOS.Shared.Dtos.Product;

public record ProductSizeDto(
    [property: DisplayName("ID")]
    int Id,

    [property: DisplayName("Kích cỡ")]
    string SizeName,

    [property: DisplayName("Điều chỉnh giá (VNĐ)")]
    decimal PriceAdjustment
);
