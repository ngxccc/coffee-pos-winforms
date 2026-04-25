using AntdUI;
using CoffeePOS.Core;
using CoffeePOS.Features.Admin.Controls;
using CoffeePOS.Services.Contracts.Commands;
using CoffeePOS.Services.Contracts.Queries;
using CoffeePOS.Shared.Dtos.User;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Admin;

public partial class UC_ManageUsers : UserControl
{
    private readonly IUserService _userService;
    private readonly IUserQueryService _userQueryService;
    private readonly IUserSession _session;

    private List<UserGridDto> _allUsers = [];
    private List<UserGridDto> _filteredUsers = [];

    private readonly System.Windows.Forms.Timer _searchDebouncer = new()
    {
        Interval = 300
    };

    public UC_ManageUsers(IUserService userService, IUserQueryService userQueryService, IUserSession session)
    {
        _userService = userService;
        _userQueryService = userQueryService;
        _session = session;

        InitializeComponent();
        SetupTable();
        SetupEvents();

        _ = LoadDataAsync();
    }

    private void SetupTable()
    {
        _tableUsers.Columns =
        [
            DtoHelper.CreateCol<UserGridDto>(nameof(UserGridDto.Id), c =>
            {
                c.Align = ColumnAlign.Center;
                c.SortOrder = true;
            }),
            DtoHelper.CreateCol<UserGridDto>(nameof(UserGridDto.Username), c => c.SortOrder = true),
            DtoHelper.CreateCol<UserGridDto>(nameof(UserGridDto.FullName), c => c.SortOrder = true),
            DtoHelper.CreateCol<UserGridDto>(nameof(UserGridDto.Role), c =>
            {
                c.Align = ColumnAlign.Center;
                c.SortOrder = true;
            }),
            DtoHelper.CreateCol<UserGridDto>(nameof(UserGridDto.Status), c =>
            {
                c.Align = ColumnAlign.Center;
                c.SortOrder = true;
                c.Render = (value, record, rowIndex) =>
                {
                    var u = (UserGridDto)record;
                    return new CellBadge(u.IsActive ? TState.Success : TState.Error, u.IsActive ? "Hoạt động" : "Đã khoá");
                };
            }),
            new Column("action", "Thao tác")
            {
                Align = ColumnAlign.Center,
                Fixed = true,
                Render = (value, record, rowIndex) =>
                {
                    var u = (UserGridDto)record;
                    return new CellButton[] {
                        new("edit", "Cập nhật", TTypeMini.Primary),
                        new("toggle", u.IsActive ? "Khóa" : "Mở khóa", u.IsActive ? TTypeMini.Error : TTypeMini.Success)
                    };
                }
            }
        ];

        _tableUsers.CellButtonClick += TableUsers_CellButtonClick;
    }

    private void SetupEvents()
    {
        _txtSearch.TextChanged += (s, e) =>
        {
            _searchDebouncer.Stop();
            _searchDebouncer.Start();
        };

        _searchDebouncer.Tick += ExecuteFilterAndSort;
        _btnAdd.Click += HandleAddUser;
    }

    private async Task LoadDataAsync()
    {
        try
        {
            await Spin.open(_tableUsers, async cfg =>
            {
                _allUsers = await _userQueryService.GetUserGridAsync();

                Invoke(() =>
                {
                    ExecuteFilterAndSort(this, EventArgs.Empty);
                });
            });
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Error($"Lỗi tải danh sách nhân viên: {ex.Message}", owner: this, type: FeedbackType.Message);
        }
    }

    private void ExecuteFilterAndSort(object? sender, EventArgs e)
    {
        _searchDebouncer.Stop();

        string keyword = _txtSearch.Text.Trim();

        _filteredUsers = string.IsNullOrEmpty(keyword)
            ? [.. _allUsers]
            : [.. _allUsers.Where(u =>
                u.Username.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                u.FullName.Contains(keyword, StringComparison.OrdinalIgnoreCase))];

        _tableUsers.DataSource = _filteredUsers;
    }

    private void TableUsers_CellButtonClick(object sender, TableButtonEventArgs e)
    {
        if (e.Record is not UserGridDto selectedUser) return;

        if (e.Btn.Id == "edit") HandleEditUser(selectedUser);
        if (e.Btn.Id == "toggle") HandleToggleStatus(selectedUser);
    }

    private void HandleAddUser(object? sender, EventArgs e)
    {
        var userAccountEditor = new UC_UserAccountEditor(null);
        Form form = FindForm() ?? throw new InvalidOperationException("Lỗi UI.");

        var config = new Modal.Config(form, "THÊM NHÂN VIÊN MỚI", userAccountEditor)
        {
            Font = UiTheme.BodyFont,
            OkText = "Lưu",
            CancelText = "Hủy",
            OnOk = (cfg) =>
            {
                bool isValid = false;
                UserAccountPayload? payload = null;

                Invoke(() =>
                {
                    if (userAccountEditor.ValidateInput())
                    {
                        payload = userAccountEditor.GetPayload();
                        isValid = true;
                    }
                });

                if (!isValid || payload == null) return false;

                ExecuteSaveUserAsync(payload, isUpdate: false, targetUserId: 0);
                return true;
            }
        };

        Modal.open(config);
    }

    private void HandleEditUser(UserGridDto selectedUser)
    {
        var userAccountEditor = new UC_UserAccountEditor(selectedUser);
        Form form = FindForm() ?? throw new InvalidOperationException("Lỗi UI.");

        var config = new Modal.Config(form, $"CẬP NHẬT TÀI KHOẢN: {selectedUser.Username}", userAccountEditor)
        {
            Font = UiTheme.BodyFont,
            OkText = "Cập nhật",
            CancelText = "Hủy",
            OnOk = (cfg) =>
            {
                bool isValid = false;
                UserAccountPayload? payload = null;

                Invoke(() =>
                {
                    if (userAccountEditor.ValidateInput())
                    {
                        payload = userAccountEditor.GetPayload();
                        isValid = true;
                    }
                });

                if (!isValid || payload == null) return false;

                ExecuteSaveUserAsync(payload, isUpdate: true, targetUserId: selectedUser.Id);
                return true;
            }
        };

        Modal.open(config);
    }

    private void ExecuteSaveUserAsync(UserAccountPayload payload, bool isUpdate, int targetUserId)
    {
        Target target = new(this);
        AntdUI.Message.loading(target, "Đang xử lý...", async msg =>
        {
            msg.ID = "save_user";
            try
            {
                if (isUpdate)
                {
                    var command = new UpdateUserAccountDto(targetUserId, payload.Username, payload.FullName, payload.Role, payload.Password, payload.ConfirmPassword);
                    await _userService.UpdateUserAccountAsync(_session.CurrentUser!.Id, command);
                }
                else
                {
                    var command = new CreateUserDto(payload.Username, payload.FullName, payload.Role, payload.Password, payload.ConfirmPassword);
                    await _userService.AddUserAsync(command);
                }

                Invoke(() => MessageBoxHelper.Success("Lưu thành công!", owner: this, type: FeedbackType.Message));
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                Invoke(() => MessageBoxHelper.Error(ex.Message, owner: this, type: FeedbackType.Message));
            }
            finally
            {
                Invoke(() => AntdUI.Message.close_id("save_user"));
            }
        });
    }

    private async void HandleToggleStatus(UserGridDto selectedUser)
    {
        bool nextState = !selectedUser.IsActive;
        string actionText = nextState ? "mở khóa" : "khóa";

        if (!MessageBoxHelper.ConfirmWarning($"Bạn có chắc muốn {actionText} tài khoản '{selectedUser.Username}'?", "Xác nhận", this))
            return;

        try
        {
            await _userService.SetUserActiveStatusAsync(_session.CurrentUser!.Id, selectedUser.Id, nextState);
            MessageBoxHelper.Success($"Đã {actionText} thành công!", owner: this, type: FeedbackType.Message);
            await LoadDataAsync();
        }
        catch (ArgumentException ex)
        {
            MessageBoxHelper.Warning(ex.Message, "Lỗi", this);
        }
        catch (InvalidOperationException ex)
        {
            MessageBoxHelper.Warning(ex.Message, "Cảnh báo", this);
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Error($"Lỗi cập nhật trạng thái: {ex.Message}", owner: this);
        }
    }
}
