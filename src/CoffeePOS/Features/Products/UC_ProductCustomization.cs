using CoffeePOS.Forms.Core;
using CoffeePOS.Services.Contracts.Queries;
using CoffeePOS.Shared.Dtos;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Products;

public partial class UC_ProductCustomization : UserControl, IValidatableComponent<CartItemDto>
{
    private readonly IProductQueryService _productQueryService;
    private readonly ProductDetailDto _product;
    private readonly CartItemDto? _existingItem;
    private readonly Dictionary<int, AntdUI.Checkbox> _toppingChecks = [];

    private List<ToppingGridDto> _allToppings = [];
    private bool _loaded;

    public UC_ProductCustomization(ProductDetailDto product, IProductQueryService productQueryService)
    {
        _product = product;
        _productQueryService = productQueryService;
        _existingItem = null;

        InitializeComponent();

        WireEvents();
    }

    public UC_ProductCustomization(CartItemDto existingItem, ProductDetailDto product, IProductQueryService productQueryService)
        : this(product, productQueryService)
    {
        _existingItem = existingItem;
    }

    private void WireEvents()
    {
        _segSize.SelectIndexChanged += (s, e) => UpdateTotalPrice();
        _numQuantity.ValueChanged += (s, e) => UpdateTotalPrice();
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (_loaded) return;

        _loaded = true;
        await LoadToppingsAsync();

        if (_existingItem != null) LoadState();

        // BUGFIX: Calculate initial total price after all UI values are set
        // Without this, the price stays at 0 until user changes a value
        UpdateTotalPrice();
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
                    Tag = topping.Id,
                    Margin = new Padding(0, 4, 0, 4),
                    Height = 30,
                    Width = 180
                };

                cb.CheckedChanged += (s, e) => UpdateTotalPrice();

                _toppingChecks[topping.Id] = cb;
                _pnlToppings.Controls.Add(cb);
            }
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Error($"Lỗi nạp topping: {ex.Message}", owner: this);
        }
    }

    private void UpdateTotalPrice()
    {
        if (!_loaded) return;

        decimal toppingTotal = 0;
        foreach (var topping in _allToppings)
        {
            if (_toppingChecks.TryGetValue(topping.Id, out var cb) && cb.Checked)
            {
                toppingTotal += topping.Price;
            }
        }

        // Tương lai nếu Size M, L có giá khác thì sẽ check _rbM.Checked, _rbL.Checked ở đây
        decimal basePrice = _product.Price;

        decimal finalPrice = (basePrice + toppingTotal) * _numQuantity.Value;

        _lblTotalPrice.Text = $"Tổng: {finalPrice:N0} đ";
    }

    private void LoadState()
    {
        // WHY: Map existing item properties back to UI components for editing mode
        if (_existingItem == null) return;

        if (_existingItem.SizeName == "S") _segSize.SelectIndex = 0;
        else if (_existingItem.SizeName == "M") _segSize.SelectIndex = 1;
        else if (_existingItem.SizeName == "L") _segSize.SelectIndex = 2;

        _numQuantity.Value = _existingItem.Quantity;

        var selectedIds = _existingItem.Toppings.Select(t => t.ToppingId).ToHashSet();
        foreach (var id in selectedIds)
        {
            if (_toppingChecks.TryGetValue(id, out var cb)) cb.Checked = true;
        }
    }

    public bool ValidateInput() => true; // Customization always has a default state (Size S, Qty 1)

    public CartItemDto GetPayload()
    {
        string size = _segSize?.Items[_segSize.SelectIndex]?.ID ?? "M";
        int qty = (int)_numQuantity.Value;

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
            ImageUrl = _product.ImageUrl,
            Toppings = selectedToppings,
            Quantity = qty
        };
    }
}
