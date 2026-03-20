using CoffeePOS.Core;
using CoffeePOS.Data.Repositories;
using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Services;

public class BillService(IBillRepository billRepo, IUserSession session) : IBillService
{
    public Task<int> ProcessFullOrderAsync(CreateBillDto command)
    {
        if (!session.IsLoggedIn || session.CurrentUser is null)
            throw new UnauthorizedAccessException("Chưa đăng nhập không thể tạo bill!");

        if (command.BuzzerNumber <= 0) throw new ArgumentException("Số thẻ rung phải lớn hơn 0!");
        if (command.Items.Count == 0) throw new ArgumentException("Không thể tạo hóa đơn trống!");
        if (command.TotalAmount <= 0) throw new ArgumentException("Tổng tiền hóa đơn không hợp lệ!");
        if (command.CreatedByUserId != session.CurrentUser.Id)
            throw new UnauthorizedAccessException("Người dùng tạo hóa đơn không hợp lệ!");

        return billRepo.ProcessFullOrderAsync(command);
    }

    public Task CancelBillAsync(int billId) => billRepo.CancelBillAsync(billId);
}
