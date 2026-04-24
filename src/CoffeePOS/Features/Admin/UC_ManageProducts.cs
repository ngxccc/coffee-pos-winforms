using AntdUI;
using CoffeePOS.Features.Admin.Controls;
using CoffeePOS.Services.Contracts.Commands;
using CoffeePOS.Services.Contracts.Queries;
using CoffeePOS.Shared.Dtos.Product;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Admin;

public partial class UC_ManageProducts : UserControl
{
    private readonly IProductService _productService;
    private readonly IProductQueryService _productQueryService;
    private readonly ICategoryQueryService _categoryQueryService;

    private List<ProductGridDto> _allProducts = [];
    private List<ProductGridDto> _filteredProducts = [];

    public UC_ManageProducts(IProductService productService, IProductQueryService productQueryService, ICategoryQueryService categoryQueryService)
    {
        _productService = productService;
        _productQueryService = productQueryService;
        _categoryQueryService = categoryQueryService;

        InitializeComponent();
        SetupTable();
        SetupEvents();

        _ = LoadDataAsync();
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
            _allProducts = await _productQueryService.GetProductGridAsync(_switchTrash.Checked);
            ApplyFilterAndSort(this, EventArgs.Empty);
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

        _tableProducts.DataSource = _filteredProducts;
    }

    private void TableProducts_CellButtonClick(object sender, TableButtonEventArgs e)
    {
        if (e.Record is not ProductGridDto selectedItem) return;

        if (e.Btn.Id == "edit")
        {
            HandleEditProduct(selectedItem);
        }
        if (e.Btn.Id == "delete")
        {
            HandleDeleteProduct(selectedItem);
        }
        if (e.Btn.Id == "restore")
        {
            HandleRestoreProduct(selectedItem);
        }
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
            var categories = await _categoryQueryService.GetSelectableCategoriesAsync();
            var uiFields = new UC_ProductEditor(categories);

            Form form = FindForm() ?? throw new InvalidOperationException("Lỗi UI: UserControl chưa được gắn vào Form chính.");

            var config = new Modal.Config(form, "THÊM SẢN PHẨM MỚI", uiFields)
            {
                Font = UiTheme.BodyFont,
                OkText = "Lưu",
                CancelText = "Huỷ",
                OnOk = (cfg) =>
                {
                    if (!uiFields.ValidateInput()) return false;

                    ExecuteSaveProductAsync(uiFields.GetPayload(), isUpdate: false, productId: 0);
                    return false;
                }
            };
            Modal.open(config);
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Error($"Lỗi mở form: {ex.Message}", owner: this);
        }
    }

    private async void HandleEditProduct(ProductGridDto selectedItem)
    {
        try
        {
            var product = await _productQueryService.GetProductByIdAsync(selectedItem.Id);
            if (product == null)
            {
                MessageBoxHelper.Error("Không tìm thấy sản phẩm!", type: FeedbackType.Message);
                return;
            }

            var categories = await _categoryQueryService.GetSelectableCategoriesAsync();
            var uiFields = new UC_ProductEditor(categories, product);

            Form form = FindForm() ?? throw new InvalidOperationException("Lỗi UI: UserControl chưa được gắn vào Form chính.");

            var config = new Modal.Config(form, $"CẬP NHẬT: {product.Name}", uiFields)
            {
                Font = UiTheme.BodyFont,
                OkText = "Cập nhật",
                CancelText = "Huỷ",
                OnOk = (cfg) =>
                {
                    if (!uiFields.ValidateInput()) return false;

                    ExecuteSaveProductAsync(uiFields.GetPayload(), isUpdate: true, productId: product.Id, product.ImageUrl);
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
