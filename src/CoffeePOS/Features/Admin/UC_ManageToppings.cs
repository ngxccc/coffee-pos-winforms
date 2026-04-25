using AntdUI;
using CoffeePOS.Features.Admin.Controls;
using CoffeePOS.Services.Contracts.Commands;
using CoffeePOS.Services.Contracts.Queries;
using CoffeePOS.Shared.Dtos.Product;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Admin;

public partial class UC_ManageToppings : UserControl
{
    private readonly IToppingQueryService _queryService;
    private readonly IToppingService _cmdService;
    private List<ToppingDto> _rawList = [];

    public UC_ManageToppings(IToppingQueryService queryService, IToppingService cmdService)
    {
        _queryService = queryService;
        _cmdService = cmdService;

        InitializeComponent();
        SetupTable();
        WireEvents();

        Load += async (s, e) => { await LoadDataAsync(); };
    }

    private void SetupTable()
    {
        _table.Columns =
        [
            DtoHelper.CreateCol<ToppingDto>(nameof(ToppingDto.Name), c => c.SortOrder = true),
            DtoHelper.CreateCol<ToppingDto>(nameof(ToppingDto.Price), c =>
            {
                c.DisplayFormat = "N0";
                c.Align = ColumnAlign.Right;
                c.SortOrder = true;
            }),
            new Column("action", "Thao tác")
            {
                Align = ColumnAlign.Center,
                Fixed = true,
                Render = (value, record, rowIndex) =>
                {
                    if (_switchTrash.Checked)
                        return new CellButton("restore", "Khôi phục") { Type = TTypeMini.Success };

                    return new CellButton[] {
                        new("edit", "Sửa") { Type = TTypeMini.Primary },
                        new("delete", "Xoá") { Type = TTypeMini.Error }
                    };
                }
            }
        ];
    }

    private void WireEvents()
    {
        _txtSearch.OnDebouncedTextChanged(300, () => Invoke(HandleSearch));
        _switchTrash.CheckedChanged += HandleTrashModeChanged;
        _btnAdd.Click += HandleAddClicked;
        _table.CellButtonClick += HandleTableAction;
    }

    private async Task LoadDataAsync()
    {
        Target target = new(this);
        AntdUI.Message.loading(target, "Đang tải dữ liệu...", async msg =>
        {
            msg.ID = "load_toppings";
            try
            {
                bool isTrash = Invoke(() => _switchTrash.Checked);
                _rawList = await _queryService.GetToppingsAsync(isTrash);
                Invoke(HandleSearch);
            }
            catch (Exception ex)
            {
                Invoke(() => MessageBoxHelper.Error($"Lỗi tải dữ liệu: {ex.Message}", owner: this));
            }
            finally
            {
                Invoke(() => AntdUI.Message.close_id("load_toppings"));
            }
        }, UiTheme.BodyFont);
    }

    private void HandleSearch()
    {
        string kw = _txtSearch.Text.Trim().ToLower();
        if (string.IsNullOrEmpty(kw))
        {
            _table.DataSource = _rawList;
            return;
        }

        _table.DataSource = _rawList.Where(x => x.Name.Contains(kw, StringComparison.CurrentCultureIgnoreCase)).ToList();
    }

    private async void HandleTrashModeChanged(object? sender, BoolEventArgs e)
    {
        _table.BackColor = _switchTrash.Checked ? Color.MistyRose : UiTheme.Surface;
        _btnAdd.Enabled = !_switchTrash.Checked;
        _table.DataSource = null;
        await LoadDataAsync();
    }

    private void HandleAddClicked(object? sender, EventArgs e)
    {
        var editor = new UC_ToppingEditor();
        ModalHelper.OpenModal(this, "THÊM TOPPING MỚI", editor,
            editor.ValidateInput,
            async () => await _cmdService.AddToppingAsync(editor.GetPayload())
        );
    }

    private void HandleTableAction(object sender, TableButtonEventArgs e)
    {
        if (e.Record is not ToppingDto record) return;

        switch (e.Btn.Id)
        {
            case "edit":
                HandleEditTopping(record);
                break;
            case "delete":
                HandleDeleteTopping(record);
                break;
            case "restore":
                HandleRestoreTopping(record);
                break;
        }
    }

    private void HandleEditTopping(ToppingDto record)
    {
        var editor = new UC_ToppingEditor(record.Id, record.Name, record.Price);

        ModalHelper.OpenModal(this, $"SỬA: {record.Name}", editor,
            editor.ValidateInput,
            async () => await _cmdService.UpdateToppingAsync(editor.GetPayload())
        );
    }

    private void HandleDeleteTopping(ToppingDto record)
    {
        ModalHelper.ExecuteActionWithConfirmAndSpin(this,
            $"Xóa topping '{record.Name}'?\n(Sẽ bị ẩn khỏi menu bán hàng)",
            async () => await _cmdService.SoftDeleteToppingAsync(record.Id),
            LoadDataAsync
        );
    }

    private void HandleRestoreTopping(ToppingDto record)
    {
        ModalHelper.ExecuteActionWithConfirmAndSpin(this,
            $"Khôi phục '{record.Name}' trở lại phần mềm?",
            async () => await _cmdService.RestoreToppingAsync(record.Id),
            LoadDataAsync
        );
    }


}
