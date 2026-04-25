using AntdUI;
using CoffeePOS.Forms.Core;
using CoffeePOS.Services.Contracts.Queries;
using CoffeePOS.Shared.Dtos.Bill;
using CoffeePOS.Shared.Dtos.Product;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Products;

public partial class UC_ProductCustomization : UserControl, IValidatableComponent<CartItemDto>
{
    private readonly IProductQueryService _productQueryService;
    private readonly ProductDetailDto _product;
    private readonly CartItemDto? _existingItem;

    private List<ToppingDto> _allToppings = [];
    private bool _loaded;
    private readonly decimal _basePrice;
    private decimal _currentSizeAdjustment = 0;

    public UC_ProductCustomization(ProductDetailDto product, IProductQueryService productQueryService)
    {
        _product = product;
        _basePrice = product.Price;
        _productQueryService = productQueryService;
        _existingItem = null;

        InitializeComponent();

        BindSizes();
        WireEvents();
    }

    public UC_ProductCustomization(CartItemDto existingItem, ProductDetailDto product, IProductQueryService productQueryService)
        : this(product, productQueryService)
    {
        _existingItem = existingItem;
    }

    private void BindSizes()
    {
        _segSize.Items.Clear();

        if (_product.Sizes == null || _product.Sizes.Count == 0)
        {
            // HACK: Nếu món không có size (ví dụ: Bánh ngọt), ẩn luôn thanh Segmented
            // _segSize.Items.Add(new SegmentedItem { ID = "S", Text = "Size S", Tag = 0m });
            // _currentSizeAdjustment = 0m;
            // _segSize.SelectIndex = 0;
            return;
        }

        foreach (var size in _product.Sizes)
        {
            // Format UI: L (+10,000đ)
            string displayText = $"{size.SizeName} (+{size.PriceAdjustment:N0}đ)";

            _segSize.Items.Add(new SegmentedItem
            {
                ID = size.SizeName,
                Text = displayText,
                Tag = size.PriceAdjustment
            });
        }

        _segSize.SelectIndex = 0;
        _currentSizeAdjustment = _product.Sizes[0].PriceAdjustment;
    }

    private void WireEvents()
    {
        _segSize.ItemClick += (s, e) => CalculateTotal();
        _numQuantity.ValueChanged += (s, e) => CalculateTotal();

        _tableToppings.Columns =
        [
            new ColumnCheck("IsSelected").SetFixed(),
            DtoHelper.CreateCol<ToppingDto>(nameof(ToppingDto.Name), c =>
            {
                c.Align = ColumnAlign.Left;
                c.SortOrder = true;
            }),
            DtoHelper.CreateCol<ToppingDto>(nameof(ToppingDto.Price), c =>
            {
                c.Align = ColumnAlign.Right;
                c.DisplayFormat = "{0:N0} đ";
                c.SortOrder = true;
            })
        ];
        _tableToppings.CheckedChanged += (s, e) => CalculateTotal();
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (_loaded) return;

        _loaded = true;
        await LoadToppingsAsync();

        if (_existingItem != null) LoadState();

        CalculateTotal();
    }

    private async Task LoadToppingsAsync()
    {
        _allToppings = await _productQueryService.GetAllToppingsAsync();

        foreach (var topping in _allToppings)
        {
            topping.IsSelected = false;
        }

        if (_existingItem != null)
        {
            // Nếu là mode sửa món, chọn lại các topping cũ
            var selectedIds = _existingItem.Toppings.Select(t => t.Id).ToHashSet();
            // Shallow copy
            var rowsToSelect = _allToppings.Where(t => selectedIds.Contains(t.Id)).ToArray();
            foreach (var topping in rowsToSelect) topping.IsSelected = true;
        }
        _tableToppings.DataSource = _allToppings;
    }

    private void CalculateTotal()
    {
        if (!_loaded) return;

        if (!(_product.Sizes?.Count == 0))
        {
            _currentSizeAdjustment = _product.Sizes![_segSize.SelectIndex].PriceAdjustment;
        }

        var selectedToppings = _allToppings.Where(t => t.IsSelected).ToList();

        decimal toppingTotal = selectedToppings.Sum(t => t.Price);
        decimal total = (_basePrice + _currentSizeAdjustment + toppingTotal) * (int)_numQuantity.Value;

        _lblTotalPrice.Text = $"Tổng: {total:N0} đ";
    }

    private void LoadState()
    {
        // WHY: Map existing item properties back to UI components for editing mode
        if (_existingItem == null) return;

        if (_existingItem.SizeName == "S") _segSize.SelectIndex = 0;
        else if (_existingItem.SizeName == "M") _segSize.SelectIndex = 1;
        else if (_existingItem.SizeName == "L") _segSize.SelectIndex = 2;

        _numQuantity.Value = _existingItem.Quantity;
        if (!string.IsNullOrWhiteSpace(_existingItem.Note))
        {
            _cboNote.SelectedValue = [.. _existingItem.Note
            .Split([','], StringSplitOptions.RemoveEmptyEntries)
            .Select(n => n.Trim())
            .Cast<object>()];
        }
    }

    public bool ValidateInput() => true; // Customization always has a default state (Size S, Qty 1)

    public CartItemDto GetPayload()
    {
        string size = _segSize?.Items[_segSize.SelectIndex]?.ID ?? "M";
        int qty = (int)_numQuantity.Value;
        decimal basePrice = _basePrice + _currentSizeAdjustment;
        var selectedToppings = _allToppings.Where(t => t.IsSelected).ToList();
        string noteText = "";
        if (_cboNote.SelectedValue != null)
        {
            var notes = _cboNote.SelectedValue.Cast<string>().Where(n => !string.IsNullOrWhiteSpace(n));
            noteText = string.Join(", ", notes);
        }

        return new CartItemDto
        {
            ProductId = _product.Id,
            ProductName = _product.Name,
            SizeName = size,
            BasePrice = basePrice,
            ImageUrl = _product.ImageUrl,
            Toppings = selectedToppings,
            Quantity = qty,
            Note = noteText
        };
    }
}
