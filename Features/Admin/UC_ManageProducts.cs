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
    private DataGridView dgvProducts = null!;
    private TextBox txtSearch = null!;
    private IconButton btnAdd = null!;
    private IconButton btnEdit = null!;
    private IconButton btnDelete = null!;
    private CheckBox chkTrashMode = null!;

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

        txtSearch = new TextBox
        {
            Width = 300,
            Font = new Font("Segoe UI", 12),
            Location = new Point(250, 22),
            PlaceholderText = "Nhập tên món để tìm..."
        };
        txtSearch.TextChanged += (s, e) => ApplyFilterAndSort();

        chkTrashMode = new CheckBox
        {
            Text = "Xem Thùng Rác",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = Color.Red,
            AutoSize = true,
            Location = new Point(570, 25),
            Cursor = Cursors.Hand
        };
        chkTrashMode.CheckedChanged += ChkTrashMode_CheckedChanged;

        FlowLayoutPanel flpButtons = new()
        {
            Dock = DockStyle.Right,
            Width = 400,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 10, 0, 0)
        };

        btnDelete = UIHelper.CreateActionButton("Xóa", IconChar.Trash, Color.FromArgb(231, 76, 60), DeleteProductAsync);
        btnEdit = UIHelper.CreateActionButton("Sửa", IconChar.Pen, Color.FromArgb(243, 156, 18), EditProductAsync);
        btnAdd = UIHelper.CreateActionButton("Thêm Mới", IconChar.Plus, Color.FromArgb(46, 204, 113), AddProductAsync);

        flpButtons.Controls.AddRange([btnDelete, btnEdit, btnAdd]);

        pnlTop.Controls.Add(lblTitle);
        pnlTop.Controls.Add(txtSearch);
        pnlTop.Controls.Add(flpButtons);
        pnlTop.Controls.Add(chkTrashMode);

        dgvProducts = new DataGridView
        {
            Dock = DockStyle.Fill
        };
        dgvProducts.ApplyStandardAdminStyle();
        dgvProducts.ColumnHeaderMouseClick += DgvProducts_ColumnHeaderMouseClick;
        dgvProducts.CellDoubleClick += EditProductAsync;

        Controls.Add(dgvProducts);
        Controls.Add(pnlTop);
    }

    private async Task LoadDataAsync()
    {
        try
        {
            dgvProducts.SavePosition(ref _savedScrollPosition, ref _savedSelectedRowId);

            _allProducts = await _productQueryService.GetProductGridAsync(chkTrashMode.Checked);
            ApplyFilterAndSort();

            dgvProducts.RestorePosition(_savedScrollPosition, _savedSelectedRowId);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}");
        }
    }

    private void RenderGrid(List<ProductGridDto> data)
    {
        dgvProducts.DataSource = null;
        dgvProducts.DataSource = data;

        dgvProducts.Columns["Id"].HeaderText = "Mã";
        dgvProducts.Columns["Id"].FillWeight = 10;

        dgvProducts.Columns["Name"].HeaderText = "Tên Sản Phẩm";
        dgvProducts.Columns["Name"].FillWeight = 40;

        dgvProducts.Columns["Price"].HeaderText = "Giá Bán";
        dgvProducts.Columns["Price"].DefaultCellStyle.Format = "N0";
        dgvProducts.Columns["Price"].FillWeight = 20;

        if (dgvProducts.Columns["CategoryName"] != null)
        {
            dgvProducts.Columns["CategoryName"].HeaderText = "Danh Mục";
            dgvProducts.Columns["CategoryName"].FillWeight = 30;
        }

        if (dgvProducts.Columns["CategoryId"] != null) dgvProducts.Columns["CategoryId"].Visible = false;
        if (dgvProducts.Columns["IsDeleted"] != null) dgvProducts.Columns["IsDeleted"].Visible = false;
        if (dgvProducts.Columns["ImageUrl"] != null) dgvProducts.Columns["ImageUrl"].Visible = false;

        foreach (DataGridViewColumn col in dgvProducts.Columns)
        {
            col.SortMode = DataGridViewColumnSortMode.Programmatic;
            col.HeaderCell.SortGlyphDirection = SortOrder.None;
        }

        if (!string.IsNullOrEmpty(_sortColumnName) && dgvProducts.Columns.Contains(_sortColumnName))
        {
            dgvProducts.Columns[_sortColumnName].HeaderCell.SortGlyphDirection =
                _sortAscending ? SortOrder.Ascending : SortOrder.Descending;
        }
    }

    private async void ChkTrashMode_CheckedChanged(object? sender, EventArgs e)
    {
        dgvProducts.BackgroundColor = chkTrashMode.Checked ? Color.MistyRose : Color.WhiteSmoke;

        btnDelete.Text = chkTrashMode.Checked ? " Khôi phục" : " Xóa";
        btnDelete.IconChar = chkTrashMode.Checked ? IconChar.TrashRestore : IconChar.Trash;
        btnDelete.BackColor = chkTrashMode.Checked ? Color.FromArgb(46, 204, 113) : Color.FromArgb(231, 76, 60);

        btnAdd.Visible = !chkTrashMode.Checked;
        btnEdit.Visible = !chkTrashMode.Checked;

        await LoadDataAsync();
    }

    private void DgvProducts_ColumnHeaderMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
    {
        if (e.ColumnIndex < 0 || e.ColumnIndex >= dgvProducts.Columns.Count) return;

        var column = dgvProducts.Columns[e.ColumnIndex];
        string columnName = column.DataPropertyName;
        if (string.IsNullOrWhiteSpace(columnName)) columnName = column.Name;

        if (!IsSortableColumn(columnName)) return;

        if (string.Equals(_sortColumnName, columnName, StringComparison.Ordinal))
        {
            _sortAscending = !_sortAscending;
        }
        else
        {
            _sortColumnName = columnName;
            _sortAscending = true;
        }

        var source = _filteredProducts.Count > 0 ? _filteredProducts : _allProducts;
        RenderGrid(GetSortedData(source));
    }

    private List<ProductGridDto> GetSortedData(IEnumerable<ProductGridDto> source)
    {
        if (string.IsNullOrEmpty(_sortColumnName)) return [.. source];

        return (_sortColumnName, _sortAscending) switch
        {
            (nameof(ProductGridDto.Id), true) => [.. source.OrderBy(p => p.Id)],
            (nameof(ProductGridDto.Id), false) => [.. source.OrderByDescending(p => p.Id)],
            (nameof(ProductGridDto.Name), true) => [.. source.OrderBy(p => p.Name)],
            (nameof(ProductGridDto.Name), false) => [.. source.OrderByDescending(p => p.Name)],
            (nameof(ProductGridDto.Price), true) => [.. source.OrderBy(p => p.Price)],
            (nameof(ProductGridDto.Price), false) => [.. source.OrderByDescending(p => p.Price)],
            (nameof(ProductGridDto.CategoryName), true) => [.. source.OrderBy(p => p.CategoryName)],
            (nameof(ProductGridDto.CategoryName), false) => [.. source.OrderByDescending(p => p.CategoryName)],
            _ => [.. source]
        };
    }

    private static bool IsSortableColumn(string columnName)
    {
        return columnName == nameof(ProductGridDto.Id)
            || columnName == nameof(ProductGridDto.Name)
            || columnName == nameof(ProductGridDto.Price)
            || columnName == nameof(ProductGridDto.CategoryName);
    }

    private async void DeleteProductAsync(object? sender, EventArgs e)
    {
        if (dgvProducts.SelectedRows.Count == 0) return;

        var selectedRow = dgvProducts.SelectedRows[0];
        int productId = (int)selectedRow.Cells[nameof(ProductGridDto.Id)].Value;
        string productName = selectedRow.Cells[nameof(ProductGridDto.Name)].Value.ToString()!;

        try
        {
            if (chkTrashMode.Checked)
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
        if (dgvProducts.SelectedRows.Count == 0) return;
        int productId = (int)dgvProducts.SelectedRows[0].Cells[nameof(ProductGridDto.Id)].Value;

        try
        {
            var product = await _productService.GetProductByIdAsync(productId);
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
        string keyword = txtSearch.Text.Trim();

        if (string.IsNullOrEmpty(keyword))
        {
            _filteredProducts = [.. _allProducts];
        }
        else
        {
            _filteredProducts = [.. _allProducts.Where(c =>
                c.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))];
        }

        RenderGrid(GetSortedData(_filteredProducts));
    }
}
