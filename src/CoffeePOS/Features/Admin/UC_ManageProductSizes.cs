using AntdUI;
using CoffeePOS.Features.Admin.Controls;
using CoffeePOS.Services.Contracts.Commands;
using CoffeePOS.Services.Contracts.Queries;
using CoffeePOS.Shared.Dtos.Product;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Admin;

public partial class UC_ManageProductSizes : UserControl
{
    private readonly int _productId;
    private readonly string _productName;

    private readonly IProductSizeService _sizeService;
    private readonly IProductSizeQueryService _sizeQueryService;

    private List<ProductSizeDto> _allSizes = [];

    public UC_ManageProductSizes(
        int productId,
        string productName,
        IProductSizeService sizeService,
        IProductSizeQueryService sizeQueryService)
    {
        _productId = productId;
        _productName = productName;
        _sizeService = sizeService;
        _sizeQueryService = sizeQueryService;

        InitializeComponent();
        SetupTable();
        SetupEvents();
        Load += (s, e) => { _ = LoadDataAsync(); };
    }

    private void SetupTable()
    {
        _tableSizes.Columns =
        [
            DtoHelper.CreateCol<ProductSizeDto>(nameof(ProductSizeDto.SizeName), c =>
            {
                c.Align = ColumnAlign.Center;
                c.SortOrder = true;
            }),
            DtoHelper.CreateCol<ProductSizeDto>(nameof(ProductSizeDto.PriceAdjustment), c => {
                c.Align = ColumnAlign.Right;
                c.DisplayFormat = "N0";
                c.SortOrder = true;
            }),
            new Column("action", "Thao tác")
            {
                Align = ColumnAlign.Center,
                Fixed = true,
                Render = (value, record, rowIndex) => new CellButton[] {
                    new("edit", "Sửa") { Type = TTypeMini.Primary },
                    new("delete", "Xóa") { Type = TTypeMini.Error }
                }
            }
        ];
        _tableSizes.CellButtonClick += TableSizes_CellButtonClick;
    }

    private void SetupEvents()
    {
        _btnAdd.Click += HandleAddSize;
    }

    private async Task LoadDataAsync()
    {
        try
        {
            await Spin.open(_tableSizes, async cfg =>
            {
                _allSizes = await _sizeQueryService.GetSizesByProductIdAsync(_productId);
                Invoke(() => _tableSizes.DataSource = _allSizes);
            });
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Error($"Lỗi tải cấu hình Size: {ex.Message}", owner: this);
        }
    }

    private void TableSizes_CellButtonClick(object sender, TableButtonEventArgs e)
    {
        if (e.Record is not ProductSizeDto selectedItem) return;

        if (e.Btn.Id == "edit") HandleEditSize(selectedItem);
        if (e.Btn.Id == "delete") HandleDeleteSize(selectedItem);
    }

    private void HandleAddSize(object? sender, EventArgs e)
    {
        var editor = new UC_ProductSizeEditor();

        ModalHelper.OpenModalWithComplexValidator(this,
            $"THÊM SIZE CHO: {_productName.ToUpper()}",
            editor,
            () =>
            {
                ProductSizePayload? payload = null;
                bool isValid = false;

                Invoke(() =>
                {
                    if (editor.ValidateInput())
                    {
                        var tempPayload = editor.GetPayload();

                        if (_allSizes.Any(s => s.SizeName.Equals(tempPayload.SizeName, StringComparison.OrdinalIgnoreCase)))
                        {
                            MessageBoxHelper.Warning($"Kích cỡ '{tempPayload.SizeName}' đã tồn tại cho món này!", owner: this);
                        }
                        else
                        {
                            payload = tempPayload;
                            isValid = true;
                        }
                    }
                });

                return (isValid, payload);
            },
            async (payload) => ExecuteSaveSize(payload, isUpdate: false, targetSizeId: 0)
        );
    }

    private void HandleEditSize(ProductSizeDto selectedItem)
    {
        var editor = new UC_ProductSizeEditor(selectedItem.SizeName, selectedItem.PriceAdjustment);

        ModalHelper.OpenModalWithComplexValidator(this,
            $"SỬA GIÁ SIZE {selectedItem.SizeName}",
            editor,
            () =>
            {
                ProductSizePayload? payload = null;
                bool isValid = false;

                Invoke(() =>
                {
                    if (editor.ValidateInput())
                    {
                        payload = editor.GetPayload();
                        isValid = true;
                    }
                });

                return (isValid, payload);
            },
            async (payload) => ExecuteSaveSize(payload, isUpdate: true, targetSizeId: selectedItem.Id)
        );
    }

    private async void HandleDeleteSize(ProductSizeDto selectedItem)
    {
        if (!MessageBoxHelper.ConfirmWarning($"Bạn có chắc muốn xóa vĩnh viễn Size '{selectedItem.SizeName}' của món này?", "Xác nhận xóa", this))
            return;

        try
        {
            await _sizeService.DeleteProductSizeAsync(selectedItem.Id);
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Error($"Lỗi xóa Size: {ex.Message}", owner: this);
        }
    }

    private void ExecuteSaveSize(ProductSizePayload payload, bool isUpdate, int targetSizeId)
    {
        Target target = new(this);
        AntdUI.Message.loading(target, "Đang xử lý...", async msg =>
        {
            msg.ID = "save_size";
            try
            {
                var command = new UpsertProductSizeDto(targetSizeId, _productId, payload.SizeName, payload.PriceAdjustment);

                if (isUpdate) await _sizeService.UpdateProductSizeAsync(command);
                else await _sizeService.AddProductSizeAsync(command);

                Invoke(() => MessageBoxHelper.Success("Lưu cấu hình Size thành công!", owner: this, type: FeedbackType.Message));
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                Invoke(() => MessageBoxHelper.Error(ex.Message, owner: this, type: FeedbackType.Message));
            }
            finally
            {
                if (!IsDisposed) Invoke(() => AntdUI.Message.close_id("save_size"));
            }
        }, UiTheme.BodyFont);
    }
}
