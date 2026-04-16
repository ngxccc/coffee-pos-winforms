using CoffeePOS.Forms.Core;
using CoffeePOS.Services.Contracts.Queries;
using CoffeePOS.Shared.Dtos;
using CoffeePOS.Shared.Helpers;
namespace CoffeePOS.Features.Billing;

public class UC_ProductCustomization : UserControl, IValidatableComponent<CartItemDto>
{
    private AntdUI.Radio _rbS = null!;
    private AntdUI.Radio _rbM = null!;
    private AntdUI.Radio _rbL = null!;
    private FlowLayoutPanel _pnlToppings = null!;
    private AntdUI.InputNumber _numQuantity = null!;
    private readonly Dictionary<int, AntdUI.Checkbox> _toppingChecks = [];

    private readonly IProductQueryService _productQueryService;
    private readonly ProductDetailDto _product;
    private readonly CartItemDto? _existingItem;
    private List<ToppingGridDto> _allToppings = [];
    private bool _loaded;

    public UC_ProductCustomization(ProductDetailDto product, IProductQueryService productQueryService)
    {
        _product = product;
        _productQueryService = productQueryService;
        _existingItem = null;

        InitializeControl();
    }

    public UC_ProductCustomization(CartItemDto existingItem, ProductDetailDto product, IProductQueryService productQueryService)
    {
        _product = product;
        _productQueryService = productQueryService;
        _existingItem = existingItem;

        InitializeControl();
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (_loaded)
        {
            return;
        }

        _loaded = true;
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

    private void InitializeControl()
    {
        Dock = DockStyle.Fill;
        BackColor = Color.White;
        Font = new Font("Segoe UI", 10, FontStyle.Regular);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            Padding = new Padding(20)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        var lblTitle = new AntdUI.Label
        {
            Text = $"Sản phẩm: {_product.Name}",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = Color.FromArgb(64, 64, 64),
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 5)
        };

        var lblPrice = new AntdUI.Label
        {
            Text = $"Giá gốc: {_product.Price:N0} đ",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(231, 76, 60),
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 15)
        };

        var grpSize = new AntdUI.Panel
        {
            Radius = 8,
            Back = Color.WhiteSmoke,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 15),
            AutoSize = true,
            MinimumSize = new Size(0, 70),
        };
        var lblSize = new AntdUI.Label
        {
            Text = "Kích cỡ",
            Dock = DockStyle.Top,
            Height = 30,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(64, 64, 64),
            Padding = new Padding(10, 8, 0, 0)
        };
        var sizeFlow = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 40,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(10)
        };

        _rbS = new AntdUI.Radio
        {
            Text = "S (Nhỏ)",
            AutoSize = true,
            Font = new Font("Segoe UI", 10),
            Checked = true,
            Cursor = Cursors.Hand
        };
        _rbM = new AntdUI.Radio
        {
            Text = "M (Vừa)",
            AutoSize = true,
            Font = new Font("Segoe UI", 10),
            Cursor = Cursors.Hand,
            Margin = new Padding(20, 0, 0, 0)
        };
        _rbL = new AntdUI.Radio
        {
            Text = "L (Lớn)",
            AutoSize = true,
            Font = new Font("Segoe UI", 10),
            Cursor = Cursors.Hand,
            Margin = new Padding(20, 0, 0, 0)
        };
        sizeFlow.Controls.AddRange([_rbS, _rbM, _rbL]);
        grpSize.Controls.Add(lblSize);
        grpSize.Controls.Add(sizeFlow);

        var grpTopping = new AntdUI.Panel
        {
            Radius = 8,
            Back = Color.WhiteSmoke,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 15)
        };
        var lblTopping = new AntdUI.Label
        {
            Text = "Topping (Lựa chọn tuỳ ý)",
            Dock = DockStyle.Top,
            Height = 30,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(64, 64, 64),
            Padding = new Padding(10, 8, 0, 0)
        };

        _pnlToppings = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(10)
        };
        grpTopping.Controls.Add(_pnlToppings);
        grpTopping.Controls.Add(lblTopping);

        var grpQuantity = new AntdUI.Panel
        {
            Radius = 8,
            Back = Color.WhiteSmoke,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 10),
            AutoSize = true,
            MinimumSize = new Size(0, 70)
        };
        var lblQuantity = new AntdUI.Label
        {
            Text = "Số lượng",
            Dock = DockStyle.Top,
            Height = 30,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(64, 64, 64),
            Padding = new Padding(10, 8, 0, 0)
        };

        _numQuantity = new AntdUI.InputNumber
        {
            Value = 1,
            Minimum = 1,
            Maximum = 100,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            Width = 100,
            Location = new Point(15, 35),
            ShowControl = true
        };
        grpQuantity.Controls.Add(lblQuantity);
        grpQuantity.Controls.Add(_numQuantity);

        layout.Controls.Add(lblTitle, 0, 0);
        layout.Controls.Add(lblPrice, 0, 1);
        layout.Controls.Add(grpSize, 0, 2);
        layout.Controls.Add(grpTopping, 0, 3);
        layout.Controls.Add(grpQuantity, 0, 4);

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 1F));

        Controls.Add(layout);
    }

    public bool ValidateInput()
    {
        return _loaded;
    }

    public CartItemDto GetPayload()
    {
        string size = _rbS.Checked ? "S" : _rbM.Checked ? "M" : "L";

        var selectedToppings = _allToppings
            .Where(t => _toppingChecks.TryGetValue(t.Id, out var cb) && cb.Checked)
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

    private async Task LoadToppingsAsync()
    {
        try
        {
            _allToppings = await _productQueryService.GetAllToppingsAsync();
            _pnlToppings.Controls.Clear();
            _toppingChecks.Clear();

            foreach (var topping in _allToppings)
            {
                var cb = new AntdUI.Checkbox
                {
                    Text = $"{topping.Name} (+{topping.Price:N0} đ)",
                    AutoSize = true,
                    Tag = topping.Id,
                    Margin = new Padding(0, 4, 0, 4)
                };
                _toppingChecks[topping.Id] = cb;
                _pnlToppings.Controls.Add(cb);
            }
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Error($"Lỗi tải topping: {ex.Message}", owner: this);
        }
    }

    private void LoadState()
    {
        if (_existingItem == null)
        {
            return;
        }

        switch (_existingItem.SizeName)
        {
            case "S":
                _rbS.Checked = true;
                break;
            case "M":
                _rbM.Checked = true;
                break;
            case "L":
                _rbL.Checked = true;
                break;
        }

        _numQuantity.Value = _existingItem.Quantity;

        var selectedIds = _existingItem.Toppings.Select(t => t.ToppingId).ToHashSet();
        foreach (var id in selectedIds)
        {
            if (_toppingChecks.TryGetValue(id, out var checkbox))
            {
                checkbox.Checked = true;
            }
        }
    }
}
