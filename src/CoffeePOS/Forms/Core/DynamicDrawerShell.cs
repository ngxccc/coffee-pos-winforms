using AntdUI;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Forms.Core;

// WHY: Class Generic <T> để nhận bất kỳ Payload nào (CartItemDto, UserDto...) từ Content
public class DynamicDrawerShell<T> : UserControl
{
    private readonly IValidatableComponent<T> _contentComponent;

    public event Action<T>? OnSaved;
    public event Action? OnCancelled;

    public DynamicDrawerShell(string title, Control contentControl, int width = 450)
    {
        if (contentControl is not IValidatableComponent<T> validatable)
        {
            throw new ArgumentException("Content control MUST implement IValidatableComponent<T>");
        }
        _contentComponent = validatable;

        Width = width;
        Dock = DockStyle.Fill;
        BackColor = UiTheme.Surface;

        BuildLayout(title, contentControl);
    }

    private void BuildLayout(string title, Control contentControl)
    {
        SuspendLayout();

        AntdUI.Panel pnlHeader = new()
        {
            Dock = DockStyle.Top,
            Height = 60,
            Padding = new Padding(20, 0, 20, 0),
            BorderWidth = 1F,
            BorderColor = UiTheme.SurfaceAlt
        };
        AntdUI.Label lblTitle = new()
        {
            Text = title,
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            ForeColor = UiTheme.BrandPrimary,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };
        pnlHeader.Controls.Add(lblTitle);

        AntdUI.Panel pnlFooter = new()
        {
            Dock = DockStyle.Bottom,
            Height = 80,
            Padding = new Padding(20),
            BorderWidth = 1F,
            BorderColor = UiTheme.SurfaceAlt
        };
        pnlFooter.SuspendLayout();

        AntdUI.Button btnSave = new()
        {
            Text = "XÁC NHẬN",
            Type = TTypeMini.Primary,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            Size = new Size(160, 45),
            Dock = DockStyle.Right,
            Radius = 8,
            Cursor = Cursors.Hand
        };
        btnSave.Click += BtnSave_Click;

        AntdUI.Button btnCancel = new()
        {
            Text = "HỦY",
            Type = TTypeMini.Default,
            Ghost = true,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            Size = new Size(100, 45),
            Dock = DockStyle.Right,
            Margin = new Padding(0, 0, 15, 0),
            Radius = 8,
            Cursor = Cursors.Hand
        };
        btnCancel.Click += BtnCancel_Click;

        pnlFooter.Controls.Add(btnCancel);
        pnlFooter.Controls.Add(btnSave);

        contentControl.Dock = DockStyle.Fill;

        Controls.Add(contentControl);
        Controls.Add(pnlFooter);
        Controls.Add(pnlHeader);

        pnlFooter.ResumeLayout(false);
        ResumeLayout(false);
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (_contentComponent.ValidateInput())
        {
            var payload = _contentComponent.GetPayload();
            OnSaved?.Invoke(payload);

            FindForm()?.Close();
        }
    }

    private void BtnCancel_Click(object? sender, EventArgs e)
    {
        OnCancelled?.Invoke();
        FindForm()?.Close();
    }
}
