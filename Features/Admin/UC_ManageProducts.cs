using CoffeePOS.Forms;
using CoffeePOS.Services;
using CoffeePOS.Shared.Dtos;
using CoffeePOS.Shared.Helpers;
using FontAwesome.Sharp;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePOS.Features.Admin;

public partial class UC_ManageProducts : UserControl
{
    private readonly IProductService _productService;
    private readonly IProductQueryService _productQueryService;
    private readonly IServiceProvider _serviceProvider;

    // UI Controls
    private DataGridView _dgvProducts = null!;
    private TextBox _txtSearch = null!;
    private IconButton _btnAdd = null!;
    private IconButton _btnEdit = null!;
    private IconButton _btnDelete = null!;
    private CheckBox _chkTrashMode = null!;

    // State
    private List<ProductGridDto> _allProducts = [];
    private List<ProductGridDto> _filteredProducts = [];
    private string? _sortColumnName;
    private bool _sortAscending = true;
    private int _savedScrollPosition = -1;
    private int _savedSelectedRowId = -1;

    public UC_ManageProducts(IProductService productService, IProductQueryService productQueryService, IServiceProvider serviceProvider)
    {
        _productService = productService;
        _productQueryService = productQueryService;
        _serviceProvider = serviceProvider;

        InitializeUI();
        _ = LoadDataAsync();
    }

    private void InitializeUI()
    {
        Dock = DockStyle.Fill;
        BackColor = Color.White;

        Panel pnlTop = new()
        {
            Dock = DockStyle.Top,
            Height = 80,
            Padding = new Padding(0, 10, 0, 10)
        };

        Label lblTitle = new()
        {
            Text = "QUẢN LÝ SẢN PHẨM",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 122, 204),
            AutoSize = true,
            Location = new Point(0, 20)
        };

        _txtSearch = new TextBox
        {
            Width = 300,
            Font = new Font("Segoe UI", 12),
            Location = new Point(250, 22),
            PlaceholderText = "Nhập tên món để tìm..."
        };
        _txtSearch.OnDebouncedTextChanged(300, ApplyFilterAndSort);

        _chkTrashMode = new CheckBox
        {
            Text = "Xem Thùng Rác",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = Color.Red,
            AutoSize = true,
            Location = new Point(570, 25),
            Cursor = Cursors.Hand
        };
        _chkTrashMode.CheckedChanged += ChkTrashMode_CheckedChanged;

        FlowLayoutPanel flpButtons = new()
        {
            Dock = DockStyle.Right,
            Width = 400,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 10, 0, 0)
        };

        _btnDelete = UIHelper.CreateActionButton("Xóa", IconChar.Trash, Color.FromArgb(231, 76, 60), DeleteProductAsync);
        _btnEdit = UIHelper.CreateActionButton("Sửa", IconChar.Pen, Color.FromArgb(243, 156, 18), EditProductAsync);
        _btnAdd = UIHelper.CreateActionButton("Thêm Mới", IconChar.Plus, Color.FromArgb(46, 204, 113), AddProductAsync);

        flpButtons.Controls.AddRange([_btnDelete, _btnEdit, _btnAdd]);

        pnlTop.Controls.Add(lblTitle);
        pnlTop.Controls.Add(_txtSearch);
        pnlTop.Controls.Add(flpButtons);
        pnlTop.Controls.Add(_chkTrashMode);

        _dgvProducts = new DataGridView
        {
            Dock = DockStyle.Fill
        };
        _dgvProducts.ApplyStandardAdminStyle();
        _dgvProducts.ColumnHeaderMouseClick += DgvProducts_ColumnHeaderMouseClick;
        _dgvProducts.CellDoubleClick += EditProductAsync;

        Controls.Add(_dgvProducts);
        Controls.Add(pnlTop);
    }

    private async Task LoadDataAsync()
    {
        try
        {
            _dgvProducts.SavePosition(ref _savedScrollPosition, ref _savedSelectedRowId);

            _allProducts = await _productQueryService.GetProductGridAsync(_chkTrashMode.Checked);
            ApplyFilterAndSort();

            _dgvProducts.RestorePosition(_savedScrollPosition, _savedSelectedRowId);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}");
        }
    }

    private async void ChkTrashMode_CheckedChanged(object? sender, EventArgs e)
    {
        _dgvProducts.BackgroundColor = _chkTrashMode.Checked ? Color.MistyRose : Color.WhiteSmoke;

        _btnDelete.Text = _chkTrashMode.Checked ? " Khôi phục" : " Xóa";
        _btnDelete.IconChar = _chkTrashMode.Checked ? IconChar.TrashRestore : IconChar.Trash;
        _btnDelete.BackColor = _chkTrashMode.Checked ? Color.FromArgb(46, 204, 113) : Color.FromArgb(231, 76, 60);

        _btnAdd.Visible = !_chkTrashMode.Checked;
        _btnEdit.Visible = !_chkTrashMode.Checked;

        await LoadDataAsync();
    }

    private void DgvProducts_ColumnHeaderMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
    {
        _dgvProducts.ToggleSortState(e.ColumnIndex, ref _sortColumnName, ref _sortAscending);
        ApplyFilterAndSort();
    }

    private async void DeleteProductAsync(object? sender, EventArgs e)
    {
        if (_dgvProducts.SelectedRows.Count == 0) return;

        var selectedRow = _dgvProducts.SelectedRows[0];
        int productId = (int)selectedRow.Cells[nameof(ProductGridDto.Id)].Value;
        string productName = selectedRow.Cells[nameof(ProductGridDto.Name)].Value.ToString()!;

        try
        {
            if (_chkTrashMode.Checked)
            {
                if (MessageBox.Show($"Khôi phục '{productName}' trở lại Menu bán hàng?",
            "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    await _productService.RestoreProductAsync(productId);
                }
            }
            else
            {
                if (MessageBox.Show($"Xóa món '{productName}' khỏi Menu bán hàng?\n(Dữ liệu báo cáo cũ vẫn được giữ nguyên)",
            "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    await _productService.DeleteProductAsync(productId);
                }
            }
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }


    }

    private async void AddProductAsync(object? sender, EventArgs e)
    {
        var form = _serviceProvider.GetRequiredService<ProductDetailForm>();
        if (form.ShowDialog() == DialogResult.OK) await LoadDataAsync();
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
                MessageBox.Show("Không tìm thấy sản phẩm!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var form = _serviceProvider.GetRequiredService<ProductDetailForm>();
            form.LoadProductDetails(product);
            if (form.ShowDialog() == DialogResult.OK) await LoadDataAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi tải dữ liệu sản phẩm: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ApplyFilterAndSort()
    {
        string keyword = _txtSearch.Text.Trim();

        _filteredProducts = string.IsNullOrEmpty(keyword)
            ? [.. _allProducts]
            : [.. _allProducts.Where(c => c.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))];

        // Sắp xếp dữ liệu bằng hàm Extension Reflection siêu việt (Sort)
        var finalData = _filteredProducts.DynamicSort(_sortColumnName, _sortAscending).ToList();

        // Đổ vào Grid. (DataGrid sẽ tự đọc [DisplayName] và [Browsable] để tự vẽ cột!)
        _dgvProducts.DataSource = finalData;

        _dgvProducts.UpdateSortGlyphs(_sortColumnName, _sortAscending);
    }
}
