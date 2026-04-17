namespace CoffeePOS.Shared.Dtos;

public record CreateBillDto(
    int BuzzerNumber,
    int CreatedByUserId,
    decimal TotalAmount,
    List<CreateBillItemDto> Items
);
