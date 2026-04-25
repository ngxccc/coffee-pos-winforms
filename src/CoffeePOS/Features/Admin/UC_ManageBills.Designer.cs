using System;
using System.Drawing;
using System.Windows.Forms;
using AntdUI;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Admin;

partial class UC_ManageBills
{
    private System.ComponentModel.IContainer components = null!;

    private AntdUI.Table _tableBills = null!;
    private AntdUI.DatePicker _dpFrom = null!;
    private AntdUI.DatePicker _dpTo = null!;
    private AntdUI.Button _btnLoad = null!;
    private AntdUI.Button _btnExport = null!;
    private AntdUI.Label _lblSummary = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        Dock = DockStyle.Fill;
        BackColor = UiTheme.Surface;

        AntdUI.Panel hostPanel = new()
        {
            Dock = DockStyle.Fill,
            Radius = 8,
            Back = UiTheme.Surface,
            Padding = new Padding(UiTheme.BlockGap)
        };

        // --- HEADER CONTROLS ---
        _dpFrom = new AntdUI.DatePicker
        {
            Value = DateTime.Today,
            Font = UiTheme.BodyFont,
            PlaceholderText = "Từ ngày"
        };

        _dpTo = new AntdUI.DatePicker
        {
            Value = DateTime.Today,
            Font = UiTheme.BodyFont,
            PlaceholderText = "Đến ngày"
        };

        _btnLoad = new AntdUI.Button
        {
            Text = "Tải dữ liệu",
            Type = TTypeMini.Primary,
            Font = UiTheme.BodyFont
        };

        _btnExport = new AntdUI.Button
        {
            Text = "Xuất CSV",
            Type = TTypeMini.Success,
            Font = UiTheme.BodyFont
        };

        _lblSummary = new AntdUI.Label
        {
            Text = "Tổng đơn: 0 | Đơn hợp lệ: 0 | Đơn hủy: 0 | Doanh thu thực thu: 0 đ",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = UiTheme.BrandPrimary,
            TextAlign = ContentAlignment.MiddleRight,
            Height = 20,
            Margin = new Padding(0)
        };

        // --- LAYOUT HEADER ---
        AntdUI.GridPanel filterLayout = new()
        {
            Span = "140 14 140 10 120 120 100%",
            Height = 45,
            Margin = new Padding(0),
        };

        AntdUI.Label lblDash = new() { Text = "-", TextAlign = ContentAlignment.MiddleCenter };

        filterLayout.Controls.Add(_btnExport);
        filterLayout.Controls.Add(_btnLoad);
        filterLayout.Controls.Add(new Control()); // Spacer
        filterLayout.Controls.Add(_dpTo);
        filterLayout.Controls.Add(lblDash);
        filterLayout.Controls.Add(_dpFrom);

        AntdUI.StackPanel headerPanel = new()
        {
            Dock = DockStyle.Top,
            Vertical = true,
            Margin = new Padding(0, 0, 0, 10),
            Height = 70,
        };

        headerPanel.Controls.Add(_lblSummary);
        headerPanel.Controls.Add(filterLayout);

        // --- TABLE ---
        _tableBills = new AntdUI.Table
        {
            Dock = DockStyle.Fill,
            Radius = 8,
            Bordered = true,
            EmptyHeader = true,
            EmptyText = "Không có hóa đơn nào trong thời gian này",
            AutoSizeColumnsMode = AntdUI.ColumnsMode.Fill,
            Font = UiTheme.BodyFont
        };

        hostPanel.Controls.Add(_tableBills);
        hostPanel.Controls.Add(headerPanel);
        Controls.Add(hostPanel);
    }
}
