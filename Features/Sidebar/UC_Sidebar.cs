using FontAwesome.Sharp;

namespace CoffeePOS.Features.Sidebar;

public class UC_Sidebar : UserControl
{
    public event EventHandler? OnHomeClicked;

    public UC_Sidebar()
    {
        Width = 80;
        Dock = DockStyle.Left;
        BackColor = Color.FromArgb(30, 30, 30);

        IconButton btnHome = new()
        {
            IconChar = IconChar.Home,
            IconColor = Color.White,
            IconSize = 32,
            Dock = DockStyle.Top,
            Height = 80,
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.Transparent,
            TextImageRelation = TextImageRelation.Overlay
        };
        btnHome.FlatAppearance.BorderSize = 0;

        btnHome.Click += (s, e) => OnHomeClicked?.Invoke(this, EventArgs.Empty);

        Controls.Add(btnHome);
    }
}
