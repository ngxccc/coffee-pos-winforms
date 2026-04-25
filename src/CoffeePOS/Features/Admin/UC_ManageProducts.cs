using AntdUI;
using CoffeePOS.Core;
using CoffeePOS.Features.Admin.Controls;
using CoffeePOS.Services.Contracts.Commands;
using CoffeePOS.Services.Contracts.Queries;
using CoffeePOS.Shared.Constants;
using CoffeePOS.Shared.Dtos.Product;
using CoffeePOS.Shared.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePOS.Features.Admin;

public partial class UC_ManageProducts : UserControl
{
    private readonly IProductService _productService;
    private readonly IProductQueryService _productQueryService;
    private readonly ICategoryQueryService _categoryQueryService;
    private readonly IUiFactory _uiFactory;

    private List<ProductGridDto> _allProducts = [];
    private List<ProductGridDto> _filteredProducts = [];

    public UC_ManageProducts(
        IProductService productService,
        IProductQueryService productQueryService,
        ICategoryQueryService categoryQueryService,
        IUiFactory uiFactory)
    {
        _productService = productService;
        _productQueryService = productQueryService;
        _categoryQueryService = categoryQueryService;
        _uiFactory = uiFactory;

        InitializeComponent();
        SetupTable();
        SetupEvents();

        Load += async (s, e) => await LoadDataAsync();
    }

    private void SetupTable()
    {
        _tableProducts.Columns =
        [
            new Column(nameof(ProductGridDto.Id), DtoInfo.GetName<ProductGridDto>(nameof(ProductGridDto.Id)))
            {
                Align = ColumnAlign.Center
            },
            new Column(nameof(ProductGridDto.Name), DtoInfo.GetName<ProductGridDto>(nameof(ProductGridDto.Name))),
            new Column(nameof(ProductGridDto.CategoryName), DtoInfo.GetName<ProductGridDto>(nameof(ProductGridDto.CategoryName))),
            new Column(nameof(ProductGridDto.Price), DtoInfo.GetName<ProductGridDto>(nameof(ProductGridDto.Price)))
            {
                DisplayFormat = "N0",
                Align = ColumnAlign.Right
            },
            new Column("action", "Thao tác")
            {
                Align = ColumnAlign.Center,
                Fixed = true,

                Render = (value, record, rowIndex) => {
                    if (_switchTrash.Checked)
                        return new CellButton("restore", "Khôi phục") {
                            Type = TTypeMini.Success
                        };

                    return new CellButton[] {
                        new("sizes", "Size") {
                            Type = TTypeMini.Default,
                        },
                        new("edit", "Sửa") {
                            Type = TTypeMini.Primary,
                        },
                        new("delete", "Xoá") {
                            Type = TTypeMini.Error
                        }
                    };
                }
            }
        ];

        _tableProducts.CellButtonClick += TableProducts_CellButtonClick;
    }

    private void SetupEvents()
    {
        _txtSearch.TextChanged += ApplyFilterAndSort;
        _switchTrash.CheckedChanged += HandleTrashModeChanged;
        _btnAdd.Click += HandleAddProduct;
    }

    private async Task LoadDataAsync()
    {
        try
        {
            await Spin.open(_tableProducts, async cfg =>
            {
                _allProducts = await _productQueryService.GetProductGridAsync(_switchTrash.Checked);
                ApplyFilterAndSort(this, EventArgs.Empty);
            });
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Error($"Lỗi tải dữ liệu: {ex.Message}", type: FeedbackType.Message);
        }
    }

    private void ApplyFilterAndSort(object? sender, EventArgs e)
    {
        string keyword = _txtSearch.Text.Trim();

        _filteredProducts = string.IsNullOrEmpty(keyword)
            ? [.. _allProducts]
            : [.. _allProducts.Where(c => c.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))];

        _tableProducts.SuspendLayout();
        _tableProducts.DataSource = _filteredProducts;
        _tableProducts.ResumeLayout(true);
    }

    private void TableProducts_CellButtonClick(object sender, TableButtonEventArgs e)
    {
        if (e.Record is not ProductGridDto selectedItem) return;

        if (e.Btn.Id == "sizes") HandleManageSizes(selectedItem);
        if (e.Btn.Id == "edit") HandleEditProduct(selectedItem);
        if (e.Btn.Id == "delete") HandleDeleteProduct(selectedItem);
        if (e.Btn.Id == "restore") HandleRestoreProduct(selectedItem);
    }

    private async void HandleTrashModeChanged(object? sender, BoolEventArgs e)
    {
        _tableProducts.BackColor = _switchTrash.Checked ? Color.MistyRose : UiTheme.Surface;
        _tableProducts.DataSource = null;
        await LoadDataAsync();
    }

    private async void HandleAddProduct(object? sender, EventArgs e)
    {
        try
        {
            var productEditor = _uiFactory.CreateControl<UC_ProductEditor>(0);

            Form form = FindForm() ?? throw new InvalidOperationException("Lỗi UI: UserControl chưa được gắn vào Form chính.");

            var config = new Modal.Config(form, "THÊM SẢN PHẨM MỚI", productEditor)
            {
                Font = UiTheme.BodyFont,
                OkText = "Lưu",
                CancelText = "Huỷ",
                OnOk = (cfg) =>
                {
                    if (!productEditor.ValidateInput()) return false;

                    ExecuteSaveProductAsync(productEditor.GetPayload(), isUpdate: false, productId: 0);
                    return true;
                }
            };
            Modal.open(config);
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Error($"Lỗi mở form: {ex.Message}", owner: this);
        }
    }

    private void HandleEditProduct(ProductGridDto selectedItem)
    {
        try
        {
            var uiFields = _uiFactory.CreateControl<UC_ProductEditor>(selectedItem.Id);

            Form form = FindForm() ?? throw new InvalidOperationException("Lỗi UI.");

            var config = new Modal.Config(form, $"CẬP NHẬT: {selectedItem.Name}", uiFields)
            {
                Font = UiTheme.BodyFont,
                OkText = "Cập nhật",
                CancelText = "Huỷ",
                OnOk = (cfg) =>
                {
                    if (!uiFields.ValidateInput()) return false;

                    ExecuteSaveProductAsync(uiFields.GetPayload(), isUpdate: true, productId: selectedItem.Id, uiFields.OriginalImageUrl);
                    return true;
                }
            };
            Modal.open(config);
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Error($"Lỗi mở form: {ex.Message}", owner: this);
        }
    }

    private async void HandleDeleteProduct(ProductGridDto selectedItem)
    {
        try
        {
            if (MessageBoxHelper.ConfirmWarning($"Xóa món '{selectedItem.Name}' khỏi Menu?\n(Báo cáo cũ vẫn giữ nguyên)", "Xác nhận", this))
            {
                await _productService.DeleteProductAsync(selectedItem.Id);
            }
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Warning(ex.Message, "Cảnh báo", this);
        }
    }

    private async void HandleRestoreProduct(ProductGridDto selectedItem)
    {
        try
        {
            if (MessageBoxHelper.ConfirmWarning($"Khôi phục '{selectedItem.Name}' trở lại Menu bán hàng?", "Xác nhận", this))
            {
                await _productService.RestoreProductAsync(selectedItem.Id);
            }
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Warning(ex.Message, "Cảnh báo", this);
        }
    }

    private void HandleManageSizes(ProductGridDto selectedItem)
    {
        var ucSizes = _uiFactory.CreateControl<UC_ManageProductSizes>(
            selectedItem.Id,
            selectedItem.Name
        );

        Form form = FindForm() ?? throw new InvalidOperationException("Lỗi UI.");

        var config = new Modal.Config(form, $"CẤU HÌNH SIZE: {selectedItem.Name.ToUpper()}", ucSizes)
        {
            Font = UiTheme.BodyFont,
            OkText = "Đóng",
            CancelText = null,
            OnOk = (cfg) => { return true; }
        };

        Modal.open(config);
    }

    private void ExecuteSaveProductAsync(ProductPayload payload, bool isUpdate, int productId, string? oldImageUrl = null)
    {
        Target target = new(this);

        AntdUI.Message.loading(target, "Đang xử lý...", async msg =>
        {
            msg.ID = "save_prod";

            try
            {
                string finalFileName = await Task.Run(() => SaveSelectedImageAndResolveName(payload.ImageUrl, oldImageUrl ?? string.Empty));
                var dto = new UpsertProductDto(
                    productId,
                    payload.Name,
                    payload.Price,
                    payload.CategoryId,
                    finalFileName
                );

                if (isUpdate) await _productService.UpdateProductAsync(dto);
                else await _productService.AddProductAsync(dto);

                if (isUpdate && !string.IsNullOrWhiteSpace(oldImageUrl) && finalFileName != oldImageUrl)
                    await Task.Run(() => TryDeletePreviousImage(oldImageUrl));

                MessageBoxHelper.Success("Lưu thành công!", owner: this, type: FeedbackType.Message);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                Invoke(() => MessageBoxHelper.Error(ex.Message, type: FeedbackType.Message));
            }
            finally
            {
                Invoke(() => AntdUI.Message.close_id("save_prod"));
            }
        }, UiTheme.BodyFont);
    }

    private static string SaveSelectedImageAndResolveName(string selectedImagePath, string currentSavedImage)
    {
        if (string.IsNullOrWhiteSpace(selectedImagePath))
        {
            return currentSavedImage;
        }

        if (Uri.TryCreate(selectedImagePath, UriKind.Absolute, out Uri? uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
        {
            return selectedImagePath;
        }

        // WHY: Fallback về xử lý Local File. Bắt buộc phải có file vật lý mới copy.
        if (!File.Exists(selectedImagePath))
        {
            return currentSavedImage;
        }

        var directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "Products");
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var extension = Path.GetExtension(selectedImagePath);
        string finalFileName = $"prod_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString("N")[..8]}{extension}";
        var destinationPath = Path.Combine(directory, finalFileName);

        File.Copy(selectedImagePath, destinationPath, true);

        return finalFileName;
    }

    private static void TryDeletePreviousImage(string? currentSavedImage)
    {
        // WHY: Graceful exit nếu sản phẩm cũ chưa từng có ảnh.
        if (string.IsNullOrWhiteSpace(currentSavedImage))
        {
            return;
        }

        try
        {
            var oldPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "Products", currentSavedImage);

            if (File.Exists(oldPath))
            {
                File.Delete(oldPath);
            }
        }
        catch
        {
            // HACK: Silently swallow (nuốt lỗi) I/O Exceptions (như file đang bị khóa, không có quyền truy cập).
            // WHY: Việc xóa ảnh cũ thất bại KHÔNG ĐƯỢC PHÉP làm crash luồng lưu Database chính của hệ thống.
            // TODO: Ghi log tên file bị kẹt này vào Serilog để chạy Cron-job dọn rác (Garbage Collector) vào nửa đêm.
        }
    }
}
