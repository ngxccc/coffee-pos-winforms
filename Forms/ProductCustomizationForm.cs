using CoffeePOS.Services.Contracts.Queries;
using CoffeePOS.Shared.Dtos;
using FontAwesome.Sharp;

namespace CoffeePOS.Forms;

public class ProductCustomizationForm : Form
{
    // UI COMPONENTS
    private Panel _pnlContainer = null!;
    private GroupBox _grpSize = null!;
    private RadioButton _rbS = null!, _rbM = null!, _rbL = null!;
    private GroupBox _grpTopping = null!;
    private CheckedListBox _lstToppings = null!;
    private GroupBox _grpQuantity = null!;
    private NumericUpDown _numQuantity = null!;
    private IconButton _btnSave = null!, _btnCancel = null!;

    // STATE
    private readonly IProductQueryService _productQueryService;
    private readonly ProductDetailDto _product;
    private CartItemDto? _existingItem;
    private List<ToppingGridDto> _allToppings = [];

    // CONSTRUCTOR - Add Mode (new item)
    public ProductCustomizationForm(ProductDetailDto product, IProductQueryService productQueryService)
    {
        _product = product;
        _productQueryService = productQueryService;
        _existingItem = null;

        InitializeForm();
    }

    // CONSTRUCTOR - Edit Mode (existing item)
    public ProductCustomizationForm(CartItemDto existingItem, ProductDetailDto product, IProductQueryService productQueryService)
    {
        _product = product;
        _productQueryService = productQueryService;
        _existingItem = existingItem;

        InitializeForm();
    }

    private void InitializeForm()
    {
        // Basic Form Setup
        Text = $"Tùy chỉnh - {_product.Name}";
        Width = 500;
        Height = 700;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Padding = new Padding(20);
        BackColor = Color.White;
        Font = new Font("Segoe UI", 10, FontStyle.Regular);

        // Main Container Panel
        _pnlContainer = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = Color.White
        };

        Controls.Add(_pnlContainer);

        BuildUI();
        LoadToppingsAsync();
        LoadState();
    }

    private void BuildUI()
    {
        int yPos = 10;

        // Title
        var lblTitle = new Label
        {
            Text = $"Sản phẩm: {_product.Name}",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = Color.FromArgb(64, 64, 64),
            AutoSize = true,
            Location = new Point(10, yPos)
        };
        _pnlContainer.Controls.Add(lblTitle);
        yPos += 40;

        // Base Price Display
        var lblPrice = new Label
        {
            Text = $"Giá cơ: {_product.Price:N0} đ",
            Font = new Font("Segoe UI", 10, FontStyle.Regular),
            ForeColor = Color.FromArgb(231, 76, 60),
            AutoSize = true,
            Location = new Point(10, yPos)
        };
        _pnlContainer.Controls.Add(lblPrice);
        yPos += 35;

        // SIZE SECTION
        _grpSize = BuildSizeGroup(yPos);
        _pnlContainer.Controls.Add(_grpSize);
        yPos += 150;

        // TOPPING SECTION
        _grpTopping = BuildToppingGroup(yPos);
        _pnlContainer.Controls.Add(_grpTopping);
        yPos += 220;

        // QUANTITY SECTION
        _grpQuantity = BuildQuantityGroup(yPos);
        _pnlContainer.Controls.Add(_grpQuantity);
        yPos += 110;

        // BUTTONS
        var pnlButtons = BuildButtonsPanel(yPos);
        _pnlContainer.Controls.Add(pnlButtons);
    }

    private GroupBox BuildSizeGroup(int yPos)
    {
        var grp = new GroupBox
        {
            Text = "Kích Cỡ",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Location = new Point(10, yPos),
            Size = new Size(450, 130),
            ForeColor = Color.FromArgb(64, 64, 64)
        };

        _rbS = new RadioButton
        {
            Text = "S (Nhỏ)",
            Location = new Point(20, 30),
            Size = new Size(120, 30),
            Font = new Font("Segoe UI", 9),
            Checked = true
        };

        _rbM = new RadioButton
        {
            Text = "M (Vừa)",
            Location = new Point(20, 65),
            Size = new Size(120, 30),
            Font = new Font("Segoe UI", 9),
            Checked = false
        };

        _rbL = new RadioButton
        {
            Text = "L (Lớn)",
            Location = new Point(20, 100),
            Size = new Size(120, 30),
            Font = new Font("Segoe UI", 9),
            Checked = false
        };

        grp.Controls.Add(_rbS);
        grp.Controls.Add(_rbM);
        grp.Controls.Add(_rbL);
        return grp;
    }

    private GroupBox BuildToppingGroup(int yPos)
    {
        var grp = new GroupBox
        {
            Text = "Topping (Lựa chọn tùy ý)",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Location = new Point(10, yPos),
            Size = new Size(450, 200),
            ForeColor = Color.FromArgb(64, 64, 64)
        };

        _lstToppings = new CheckedListBox
        {
            Location = new Point(15, 30),
            Size = new Size(420, 160),
            Font = new Font("Segoe UI", 9),
            CheckOnClick = true,
            BorderStyle = BorderStyle.FixedSingle
        };

        grp.Controls.Add(_lstToppings);
        return grp;
    }

    private GroupBox BuildQuantityGroup(int yPos)
    {
        var grp = new GroupBox
        {
            Text = "Số lượng",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Location = new Point(10, yPos),
            Size = new Size(450, 90),
            ForeColor = Color.FromArgb(64, 64, 64)
        };

        _numQuantity = new NumericUpDown
        {
            Location = new Point(20, 40),
            Size = new Size(120, 30),
            Value = 1,
            Minimum = 1,
            Maximum = 100,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            TextAlign = HorizontalAlignment.Center
        };

        grp.Controls.Add(_numQuantity);
        return grp;
    }

    private Panel BuildButtonsPanel(int yPos)
    {
        var pnl = new Panel
        {
            Location = new Point(10, yPos),
            Size = new Size(450, 60),
            BackColor = Color.Transparent
        };

        _btnSave = new IconButton
        {
            Text = "LƯU",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Size = new Size(210, 45),
            Location = new Point(0, 5),
            ImageAlign = ContentAlignment.MiddleRight,
            TextImageRelation = TextImageRelation.TextBeforeImage,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(52, 152, 219),
            ForeColor = Color.White,
            Cursor = Cursors.Hand
        };
        _btnSave.FlatAppearance.BorderSize = 0;
        _btnSave.Click += (s, e) =>
        {
            DialogResult = DialogResult.OK;
            Close();
        };

        _btnCancel = new IconButton
        {
            Text = "HỦY",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Size = new Size(210, 45),
            Location = new Point(225, 5),
            ImageAlign = ContentAlignment.MiddleRight,
            TextImageRelation = TextImageRelation.TextBeforeImage,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(189, 195, 199),
            ForeColor = Color.White,
            Cursor = Cursors.Hand
        };
        _btnCancel.FlatAppearance.BorderSize = 0;
        _btnCancel.Click += (s, e) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };

        pnl.Controls.Add(_btnSave);
        pnl.Controls.Add(_btnCancel);
        return pnl;
    }

    private async void LoadToppingsAsync()
    {
        try
        {
            _allToppings = await _productQueryService.GetAllToppingsAsync();
            _lstToppings.DataSource = _allToppings;
            _lstToppings.DisplayMember = "Name";
            _lstToppings.ValueMember = "Id";

            ApplyExistingToppingsSelection();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi tải topping: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LoadState()
    {
        if (_existingItem == null)
            return; // Add mode: use defaults

        // Edit mode: restore state
        // Size
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

        // Quantity
        _numQuantity.Value = _existingItem.Quantity;

        // Toppings sẽ được check sau khi danh sách toppings load xong.
    }

    private void ApplyExistingToppingsSelection()
    {
        if (_existingItem == null)
            return;

        var selectedToppingIds = _existingItem.Toppings.Select(t => t.ToppingId).ToHashSet();
        for (int i = 0; i < _lstToppings.Items.Count; i++)
        {
            if (_lstToppings.Items[i] is ToppingGridDto topping && selectedToppingIds.Contains(topping.Id))
            {
                _lstToppings.SetItemChecked(i, true);
            }
        }
    }

    public CartItemDto GetFinalCartItem()
    {
        string size = _rbS.Checked ? "S" : _rbM.Checked ? "M" : "L";

        var selectedToppings = new List<CartToppingDto>();
        for (int i = 0; i < _lstToppings.CheckedItems.Count; i++)
        {
            if (_lstToppings.CheckedItems[i] is ToppingGridDto topping)
            {
                selectedToppings.Add(new CartToppingDto(
                    ToppingId: topping.Id,
                    ToppingName: topping.Name,
                    Price: topping.Price
                ));
            }
        }

        var cartItem = new CartItemDto
        {
            ProductId = _product.Id,
            ProductName = _product.Name,
            SizeName = size,
            BasePrice = _product.Price,
            Quantity = (int)_numQuantity.Value,
            Toppings = selectedToppings
        };

        return cartItem;
    }
}
