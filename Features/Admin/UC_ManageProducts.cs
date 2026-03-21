using CoffeePOS.Core;
using CoffeePOS.Features.Admin.Controls;
using CoffeePOS.Forms;
using CoffeePOS.Services.Contracts.Commands;
using CoffeePOS.Services.Contracts.Queries;
using CoffeePOS.Shared.Dtos;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Admin;

public class UC_ManageProducts : UserControl
{
    private readonly IProductService _productService;
    private readonly IProductQueryService _productQueryService;
    private readonly IFormFactory _formFactory;

    // UI Controls
    private UC_ProductsHeaderToolbar _toolbar = null!;
    private DataGridView _dgvProducts = null!;
    private StatefulSortableGrid<ProductGridDto> _productsGrid = null!;

    // State
    private List<ProductGridDto> _allProducts = [];
    private List<ProductGridDto> _filteredProducts = [];

    public UC_ManageProducts(IProductService productService, IProductQueryService productQueryService, IFormFactory formFactory)
    {
        _productService = productService;
        _productQueryService = productQueryService;
        _formFactory = formFactory;

        InitializeUI();
        _ = LoadDataAsync();
    }

    private void InitializeUI()
    {
        Dock = DockStyle.Fill;
        BackColor = Color.White;

        _toolbar = new UC_ProductsHeaderToolbar();
        _toolbar.SearchChanged += ApplyFilterAndSort;
        _toolbar.AddClicked += AddProductAsync;
        _toolbar.EditClicked += EditProductAsync;
        _toolbar.DeleteClicked += DeleteProductAsync;
        _toolbar.TrashModeChanged += ChkTrashMode_CheckedChanged;

        _dgvProducts = new DataGridView
        {
            Dock = DockStyle.Fill
        };
        _dgvProducts.ApplyStandardAdminStyle();
        _dgvProducts.CellDoubleClick += EditProductAsync;

        _productsGrid = new StatefulSortableGrid<ProductGridDto>(_dgvProducts);
        _productsGrid.AttachSortHandler();
        _productsGrid.SortChanged += ApplyFilterAndSort;

        Controls.Add(_dgvProducts);
        Controls.Add(_toolbar);
    }

    private async Task LoadDataAsync()
    {
        try
        {
            _productsGrid.CapturePosition();

            _allProducts = await _productQueryService.GetProductGridAsync(_toolbar.IsTrashMode);
            ApplyFilterAndSort();

            _productsGrid.RestorePosition();
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Error($"Lỗi tải dữ liệu: {ex.Message}", owner: this);
        }
    }

    private async void ChkTrashMode_CheckedChanged(object? sender, EventArgs e)
    {
        _dgvProducts.BackgroundColor = _toolbar.IsTrashMode ? Color.MistyRose : Color.WhiteSmoke;

        await LoadDataAsync();
    }

    private async void DeleteProductAsync(object? sender, EventArgs e)
    {
        if (_dgvProducts.SelectedRows.Count == 0) return;

        var selectedRow = _dgvProducts.SelectedRows[0];
        int productId = (int)selectedRow.Cells[nameof(ProductGridDto.Id)].Value;
        string productName = selectedRow.Cells[nameof(ProductGridDto.Name)].Value.ToString()!;

        try
        {
            if (_toolbar.IsTrashMode)
            {
                if (MessageBoxHelper.ConfirmWarning($"Khôi phục '{productName}' trở lại Menu bán hàng?",
            "Xác nhận", this))
                {
                    await _productService.RestoreProductAsync(productId);
                }
            }
            else
            {
                if (MessageBoxHelper.ConfirmWarning($"Xóa món '{productName}' khỏi Menu bán hàng?\n(Dữ liệu báo cáo cũ vẫn được giữ nguyên)",
            "Xác nhận", this))
                {
                    await _productService.DeleteProductAsync(productId);
                }
            }
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Warning(ex.Message, "Cảnh báo", this);
        }


    }

    private async void AddProductAsync(object? sender, EventArgs e)
    {
        using var form = _formFactory.CreateForm<AddProductForm>();
        if (form.ShowDialog(this) == DialogResult.OK) await LoadDataAsync();
    }

    private async void EditProductAsync(object? sender, EventArgs e)
    {
        if (_dgvProducts.SelectedRows.Count == 0) return;
        int productId = (int)_dgvProducts.SelectedRows[0].Cells[nameof(ProductGridDto.Id)].Value;

        try
        {
            var product = await _productQueryService.GetProductByIdAsync(productId);
            if (product == null)
            {
                MessageBoxHelper.Error("Không tìm thấy sản phẩm!", "Lỗi", this);
                return;
            }

            using var form = _formFactory.CreateForm<EditProductForm>();
            form.LoadProductDetails(product);
            if (form.ShowDialog(this) == DialogResult.OK) await LoadDataAsync();
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Error($"Lỗi tải dữ liệu sản phẩm: {ex.Message}", "Lỗi", this);
        }
    }

    private void ApplyFilterAndSort()
    {
        string keyword = _toolbar.SearchText.Trim();

        _filteredProducts = string.IsNullOrEmpty(keyword)
            ? [.. _allProducts]
            : [.. _allProducts.Where(c => c.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))];

        _productsGrid.Bind(_filteredProducts);
    }
}
