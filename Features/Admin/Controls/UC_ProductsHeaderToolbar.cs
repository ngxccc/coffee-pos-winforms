using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Admin.Controls;

public class UC_ProductsHeaderToolbar : BaseAdminHeaderToolbar
{
    protected override string Title => "QUẢN LÝ SẢN PHẨM";
    protected override string SearchPlaceholder => "Nhập tên món để tìm...";
    protected override bool ShowTrashMode => true;
}
