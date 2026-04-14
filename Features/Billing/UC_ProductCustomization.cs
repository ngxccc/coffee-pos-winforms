using CoffeePOS.Forms.Core;
using CoffeePOS.Services.Contracts.Queries;
using CoffeePOS.Shared.Dtos;
using CoffeePOS.Shared.Helpers;
namespace CoffeePOS.Features.Billing;

public class UC_ProductCustomization : UserControl, IValidatableComponent<CartItemDto>
{
    private RadioButton _rbS = null!;
    private RadioButton _rbM = null!;
    private RadioButton _rbL = null!;
    private CheckedListBox _lstToppings = null!;
    private NumericUpDown _numQuantity = null!;

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

        var lblTitle = new Label
        {
            Text = $"Sản phẩm: {_product.Name}",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = Color.FromArgb(64, 64, 64),
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 5)
        };

        var lblPrice = new Label
        {
            Text = $"Giá gốc: {_product.Price:N0} đ",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(231, 76, 60),
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 15)
        };

        var grpSize = new GroupBox
        {
            Text = "Kích cỡ",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(64, 64, 64),
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 15),
            AutoSize = true,
            MinimumSize = new Size(0, 70),
        };
        var sizeFlow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(10)
        };

        _rbS = new RadioButton
        {
            Text = "S (Nhỏ)",
            AutoSize = true,
            Font = new Font("Segoe UI", 10),
            Checked = true,
            Cursor = Cursors.Hand
        };
        _rbM = new RadioButton
        {
            Text = "M (Vừa)",
            AutoSize = true,
            Font = new Font("Segoe UI", 10),
            Cursor = Cursors.Hand,
            Margin = new Padding(20, 0, 0, 0)
        };
        _rbL = new RadioButton
        {
            Text = "L (Lớn)",
            AutoSize = true,
            Font = new Font("Segoe UI", 10),
            Cursor = Cursors.Hand,
            Margin = new Padding(20, 0, 0, 0)
        };
        sizeFlow.Controls.AddRange([_rbS, _rbM, _rbL]);
        grpSize.Controls.Add(sizeFlow);

        var grpTopping = new GroupBox
        {
            Text = "Topping (Lựa chọn tuỳ ý)",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(64, 64, 64),
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 15)
        };

        _lstToppings = new CheckedListBox
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10),
            CheckOnClick = true,
            BorderStyle = BorderStyle.None,
            Margin = new Padding(10)
        };
        grpTopping.Controls.Add(_lstToppings);

        var grpQuantity = new GroupBox
        {
            Text = "Số lượng",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(64, 64, 64),
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 10),
            AutoSize = true,
            MinimumSize = new Size(0, 70)
        };

        _numQuantity = new NumericUpDown
        {
            Value = 1,
            Minimum = 1,
            Maximum = 100,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            TextAlign = HorizontalAlignment.Center,
            Width = 100,
            Location = new Point(15, 30)
        };
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
        for (int i = 0; i < _lstToppings.Items.Count; i++)
        {
            if (_lstToppings.Items[i] is ToppingGridDto topping && selectedIds.Contains(topping.Id))
            {
                _lstToppings.SetItemChecked(i, true);
            }
        }
    }
}
