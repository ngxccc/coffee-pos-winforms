using CoffeePOS.Services.Contracts.Queries;
using CoffeePOS.Shared.Dtos;
using FontAwesome.Sharp;

namespace CoffeePOS.Forms;

public class ProductCustomizationForm : Form
{
    private GroupBox _grpSize = null!;
    private RadioButton _rbS = null!, _rbM = null!, _rbL = null!;
    private GroupBox _grpTopping = null!;
    private CheckedListBox _lstToppings = null!;
    private GroupBox _grpQuantity = null!;
    private NumericUpDown _numQuantity = null!;
    private IconButton _btnSave = null!, _btnCancel = null!;

    private readonly IProductQueryService _productQueryService;
    private readonly ProductDetailDto _product;
    private readonly CartItemDto? _existingItem;
    private List<ToppingGridDto> _allToppings = [];

    public ProductCustomizationForm(ProductDetailDto product, IProductQueryService productQueryService)
    {
        _product = product;
        _productQueryService = productQueryService;
        _existingItem = null;

        InitializeForm();
    }

    public ProductCustomizationForm(CartItemDto existingItem, ProductDetailDto product, IProductQueryService productQueryService)
    {
        _product = product;
        _productQueryService = productQueryService;
        _existingItem = existingItem;

        InitializeForm();
    }

    // WHY: Move async initialization to OnLoad to maintain UI thread control and prevent race conditions with LoadState
    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        // HACK: Block UI interactions until data dependencies are fully resolved
        Enabled = false;
        try
        {
            await LoadToppingsAsync();
            LoadState();
        }
        finally
        {
            Enabled = true;
        }
    }

    private void InitializeForm()
    {
        Text = $"Tùy chỉnh - {_product.Name}";
        Size = new Size(500, 750);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.White;
        Font = new Font("Segoe UI", 10, FontStyle.Regular);

        BuildUI();
    }

    private void BuildUI()
    {
        // PERF: Universal matrix layout handles all scaling math natively
        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            Padding = new Padding(20)
        };
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        var lblTitle = new Label { Text = $"Sản phẩm: {_product.Name}", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.FromArgb(64, 64, 64), AutoSize = true, Margin = new Padding(0, 0, 0, 5) };
        var lblPrice = new Label { Text = $"Giá cơ: {_product.Price:N0} đ", Font = new Font("Segoe UI", 10), ForeColor = Color.FromArgb(231, 76, 60), AutoSize = true, Margin = new Padding(0, 0, 0, 15) };

        _grpSize = BuildSizeGroup();
        _grpTopping = BuildToppingGroup();
        _grpQuantity = BuildQuantityGroup();
        var pnlButtons = BuildButtonsPanel();

        mainLayout.Controls.Add(lblTitle, 0, 0);
        mainLayout.Controls.Add(lblPrice, 0, 1);
        mainLayout.Controls.Add(_grpSize, 0, 2);
        mainLayout.Controls.Add(_grpTopping, 0, 3);
        mainLayout.Controls.Add(_grpQuantity, 0, 4);
        mainLayout.Controls.Add(pnlButtons, 0, 5);

        // WHY: _grpTopping absorbs 100% of remaining vertical space, letting the CheckedListBox grow dynamically
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));

        Controls.Add(mainLayout);

        AcceptButton = _btnSave;
        CancelButton = _btnCancel;
    }

    private GroupBox BuildSizeGroup()
    {
        var grp = new GroupBox { Text = "Kích Cỡ", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(64, 64, 64), Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 15), AutoSize = true, MinimumSize = new Size(0, 70) };

        // HACK: Inner FlowLayout aligns radio buttons natively without point calculations
        var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(10) };

        _rbS = new RadioButton { Text = "S (Nhỏ)", AutoSize = true, Font = new Font("Segoe UI", 10), Checked = true, Cursor = Cursors.Hand };
        _rbM = new RadioButton { Text = "M (Vừa)", AutoSize = true, Font = new Font("Segoe UI", 10), Cursor = Cursors.Hand, Margin = new Padding(20, 0, 0, 0) };
        _rbL = new RadioButton { Text = "L (Lớn)", AutoSize = true, Font = new Font("Segoe UI", 10), Cursor = Cursors.Hand, Margin = new Padding(20, 0, 0, 0) };

        flow.Controls.AddRange([_rbS, _rbM, _rbL]);
        grp.Controls.Add(flow);
        return grp;
    }

    private GroupBox BuildToppingGroup()
    {
        var grp = new GroupBox { Text = "Topping (Lựa chọn tùy ý)", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(64, 64, 64), Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 15) };

        _lstToppings = new CheckedListBox
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10),
            CheckOnClick = true,
            BorderStyle = BorderStyle.None,
            Margin = new Padding(10)
        };

        grp.Controls.Add(_lstToppings);
        return grp;
    }

    private GroupBox BuildQuantityGroup()
    {
        var grp = new GroupBox { Text = "Số lượng", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(64, 64, 64), Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 20), AutoSize = true, MinimumSize = new Size(0, 70) };

        _numQuantity = new NumericUpDown
        {
            Value = 1,
            Minimum = 1,
            Maximum = 100,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            TextAlign = HorizontalAlignment.Center,
            Width = 100,
            Location = new Point(15, 30) // OK here since it's just one control in the box
        };

        grp.Controls.Add(_numQuantity);
        return grp;
    }

    private TableLayoutPanel BuildButtonsPanel()
    {
        // PERF: 2-column grid ensures 50/50 split for Save/Cancel regardless of form width
        var pnl = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = new Padding(0) };
        pnl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        pnl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

        _btnSave = new IconButton { Text = " LƯU", Font = new Font("Segoe UI", 10, FontStyle.Bold), Dock = DockStyle.Fill, Margin = new Padding(0, 0, 10, 0), IconChar = IconChar.Save, IconSize = 24, ImageAlign = ContentAlignment.MiddleCenter, TextImageRelation = TextImageRelation.ImageBeforeText, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(46, 204, 113), ForeColor = Color.White, Cursor = Cursors.Hand, DialogResult = DialogResult.OK };
        _btnSave.FlatAppearance.BorderSize = 0;

        _btnCancel = new IconButton { Text = " HỦY", Font = new Font("Segoe UI", 10, FontStyle.Bold), Dock = DockStyle.Fill, Margin = new Padding(10, 0, 0, 0), IconChar = IconChar.Times, IconSize = 24, ImageAlign = ContentAlignment.MiddleCenter, TextImageRelation = TextImageRelation.ImageBeforeText, FlatStyle = FlatStyle.Flat, BackColor = Color.Gray, ForeColor = Color.White, Cursor = Cursors.Hand, DialogResult = DialogResult.Cancel };
        _btnCancel.FlatAppearance.BorderSize = 0;

        pnl.Controls.Add(_btnSave, 0, 0);
        pnl.Controls.Add(_btnCancel, 1, 0);
        return pnl;
    }

    private async Task LoadToppingsAsync()
    {
        try
        {
            _allToppings = await _productQueryService.GetAllToppingsAsync();
            _lstToppings.DataSource = _allToppings;
            _lstToppings.DisplayMember = nameof(ToppingGridDto.Name);
            _lstToppings.ValueMember = nameof(ToppingGridDto.Id);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi tải topping: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LoadState()
    {
        if (_existingItem == null) return;

        switch (_existingItem.SizeName)
        {
            case "S": _rbS.Checked = true; break;
            case "M": _rbM.Checked = true; break;
            case "L": _rbL.Checked = true; break;
        }

        _numQuantity.Value = _existingItem.Quantity;

        var selectedIds = _existingItem.Toppings.Select(t => t.ToppingId).ToHashSet();
        for (int i = 0; i < _lstToppings.Items.Count; i++)
        {
            if (_lstToppings.Items[i] is ToppingGridDto topping && selectedIds.Contains(topping.Id))
            {
                _lstToppings.SetItemChecked(i, true);
            }
        }
    }

    public CartItemDto GetFinalCartItem()
    {
        string size = _rbS.Checked ? "S" : _rbM.Checked ? "M" : "L";

        var selectedToppings = _lstToppings.CheckedItems.Cast<ToppingGridDto>()
            .Select(t => new CartToppingDto(t.Id, t.Name, t.Price))
            .ToList();

        return new CartItemDto
        {
            ProductId = _product.Id,
            ProductName = _product.Name,
            SizeName = size,
            BasePrice = _product.Price,
            Quantity = (int)_numQuantity.Value,
            Toppings = selectedToppings
        };
    }
}
