using CoffeePOS.Core;
using CoffeePOS.Features.Admin.Controls;
using CoffeePOS.Forms;
using CoffeePOS.Services.Contracts.Commands;
using CoffeePOS.Services.Contracts.Queries;
using CoffeePOS.Shared.Dtos;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Admin;

public class UC_ManageUsers : UserControl
{
    private readonly IUserService _userService;
    private readonly IUserQueryService _userQueryService;
    private readonly IUserSession _session;
    private readonly IFormFactory _formFactory;

    private UC_UsersHeaderToolbar _toolbar = null!;
    private DataGridView _dgvUsers = null!;
    private StatefulSortableGrid<UserGridDto> _usersGrid = null!;

    private List<UserGridDto> _allUsers = [];
    private List<UserGridDto> _filteredUsers = [];

    public UC_ManageUsers(IUserService userService, IUserQueryService userQueryService, IUserSession session, IFormFactory formFactory)
    {
        _userService = userService;
        _userQueryService = userQueryService;
        _session = session;
        _formFactory = formFactory;

        InitializeUI();
        _ = LoadDataAsync();
    }

    private void InitializeUI()
    {
        Dock = DockStyle.Fill;
        BackColor = Color.White;

        _toolbar = new UC_UsersHeaderToolbar();
        _toolbar.SearchChanged += ApplyFilterAndSort;
        _toolbar.AddClicked += AddUserAsync;
        _toolbar.ResetPasswordClicked += EditUserAsync;
        _toolbar.ToggleStatusClicked += ToggleUserStatusAsync;

        _dgvUsers = new DataGridView
        {
            Dock = DockStyle.Fill
        };
        _dgvUsers.ApplyStandardAdminStyle();
        _dgvUsers.CellDoubleClick += EditUserAsync;

        _usersGrid = new StatefulSortableGrid<UserGridDto>(_dgvUsers);
        _usersGrid.AttachSortHandler();
        _usersGrid.SortChanged += ApplyFilterAndSort;

        Controls.Add(_dgvUsers);
        Controls.Add(_toolbar);
    }

    private async Task LoadDataAsync()
    {
        try
        {
            _usersGrid.CapturePosition();

            _allUsers = await _userQueryService.GetUserGridAsync();
            ApplyFilterAndSort();

            _usersGrid.RestorePosition();
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Error($"Lỗi tải danh sách nhân viên: {ex.Message}", owner: this);
        }
    }

    private async void AddUserAsync(object? sender, EventArgs e)
    {
        using var dialog = _formFactory.CreateForm<AddUserForm>();
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            await _userService.AddUserAsync(dialog.BuildCommand());
            MessageBoxHelper.Info("Thêm nhân viên thành công!", owner: this);
            await LoadDataAsync();
        }
        catch (ArgumentException ex)
        {
            MessageBoxHelper.Warning(ex.Message, "Lỗi nhập liệu", this);
        }
        catch (InvalidOperationException ex)
        {
            MessageBoxHelper.Warning(ex.Message, "Cảnh báo", this);
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Error($"Lỗi thêm nhân viên: {ex.Message}", owner: this);
        }
    }

    private async void EditUserAsync(object? sender, EventArgs e)
    {
        if (_dgvUsers.SelectedRows.Count == 0) return;

        if (_dgvUsers.SelectedRows[0].DataBoundItem is not UserGridDto selectedUser)
        {
            MessageBoxHelper.Warning("Không thể đọc dữ liệu người dùng đã chọn.", "Lỗi", this);
            return;
        }

        using var dialog = _formFactory.CreateForm<EditUserForm>();
        dialog.LoadUser(selectedUser);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            await _userService.UpdateUserAccountAsync(_session.CurrentUser!.Id, dialog.BuildCommand());

            MessageBoxHelper.Info($"Đã cập nhật tài khoản '{selectedUser.Username}'.", owner: this);
            await LoadDataAsync();
        }
        catch (ArgumentException ex)
        {
            MessageBoxHelper.Warning(ex.Message, "Lỗi nhập liệu", this);
        }
        catch (InvalidOperationException ex)
        {
            MessageBoxHelper.Warning(ex.Message, "Cảnh báo", this);
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Error($"Lỗi cập nhật tài khoản: {ex.Message}", owner: this);
        }
    }

    private async void ToggleUserStatusAsync(object? sender, EventArgs e)
    {
        if (_dgvUsers.SelectedRows.Count == 0) return;

        var selectedRow = _dgvUsers.SelectedRows[0];
        if (selectedRow.DataBoundItem is not UserGridDto selectedUser)
        {
            MessageBoxHelper.Warning("Không thể đọc dữ liệu người dùng đã chọn.", "Lỗi", this);
            return;
        }

        int targetUserId = (int)selectedRow.Cells[nameof(UserGridDto.Id)].Value;
        string username = selectedRow.Cells[nameof(UserGridDto.Username)].Value.ToString()!;
        bool isActive = selectedUser.IsActive;

        bool nextState = !isActive;
        string actionText = nextState ? "mở khóa" : "khóa";

        if (!MessageBoxHelper.ConfirmWarning($"Bạn có chắc muốn {actionText} tài khoản '{username}'?", "Xác nhận", this))
        {
            return;
        }

        try
        {
            await _userService.SetUserActiveStatusAsync(_session.CurrentUser!.Id, targetUserId, nextState);
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
            MessageBoxHelper.Error($"Lỗi cập nhật trạng thái tài khoản: {ex.Message}", owner: this);
        }
    }

    private void ApplyFilterAndSort()
    {
        string keyword = _toolbar.SearchText.Trim();
        _filteredUsers = string.IsNullOrEmpty(keyword)
            ? [.. _allUsers]
            :
            [.. _allUsers.Where(u =>
                u.Username.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                u.FullName.Contains(keyword, StringComparison.OrdinalIgnoreCase))];

        _usersGrid.Bind(_filteredUsers);
    }
}
