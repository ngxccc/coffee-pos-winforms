using System.Diagnostics;
using CoffeePOS.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CoffeePOS.Core;

public static class InvoiceGenerator
{
    // Bắt buộc phải khai báo License (Bản miễn phí Community)
    public static void Initialize()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public static async Task GenerateAndOpenPdfAsync(int billId, int buzzerNumber, decimal totalAmount, List<BillDetail> details, bool isReprint)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                // page.Size(new PageSize(80, 2000, Unit.Millimetre)); // Kích thước bill thường dùng A5 hoặc in nhiệt (Roll80mm)
                page.ContinuousSize(80, Unit.Millimetre);
                page.Margin(2, Unit.Millimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                page.Header().Element(x => ComposeHeader(x, billId, buzzerNumber));
                page.Content().Element(x => ComposeContent(x, details));
                page.Footer().Element(x => ComposeFooter(x, totalAmount));

                if (isReprint)
                {
                    page.Background()
                        .AlignMiddle()
                        .Rotate(-45)
                        .Unconstrained()
                        .Text("BẢN SAO")
                        .FontSize(50)
                        .FontColor(Colors.Grey.Lighten2)
                        .SemiBold();
                }
            });
        });

        string directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Invoices");
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

        string filePath = Path.Combine(directory, $"Bill_{billId}_{(isReprint ? "reprint_" : "")}{DateTime.Now:yyyyMMdd_HHmmss}.pdf");

        document.GeneratePdf(filePath);

        await Task.Run(() =>
        {
            Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
        });
    }

    private static void ComposeHeader(IContainer container, int billId, int buzzerNumber)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("COFFEE POS").FontSize(20).SemiBold().FontColor(Colors.Blue.Darken2).AlignCenter();
                column.Item().Text($"Hóa đơn số: #{billId}");
                column.Item().Text($"Thẻ rung số: {buzzerNumber}");
                column.Item().Text($"Ngày: {DateTime.Now:dd/MM/yyyy HH:mm}");
            });
        });
    }

    private static void ComposeContent(IContainer container, List<BillDetail> details)
    {
        container.PaddingVertical(1, Unit.Centimetre).Column(column =>
        {
            column.Spacing(5);
            column.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).Row(row =>
            {
                row.RelativeItem(3).Text("Tên món").SemiBold();
                row.RelativeItem(1).AlignRight().Text("SL").SemiBold();
                row.RelativeItem(2).AlignRight().Text("Đơn giá").SemiBold();
                row.RelativeItem(3).AlignRight().Text("Thành tiền").SemiBold();
            });

            foreach (var item in details)
            {
                column.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).PaddingBottom(5).Row(row =>
                {
                    row.RelativeItem(3).Text(item.ProductName);
                    row.RelativeItem(1).AlignRight().Text(item.Quantity.ToString());
                    row.RelativeItem(2).AlignRight().Text($"{item.Price:N0}đ");
                    row.RelativeItem(3).AlignRight().Text($"{item.Quantity * item.Price:N0}đ");
                });
            }
        });
    }

    private static void ComposeFooter(IContainer container, decimal totalAmount)
    {
        container.AlignRight().Text(text =>
        {
            text.Span("Tổng cộng: ").SemiBold().FontSize(12);
            text.Span($"{totalAmount:N0} VNĐ").Bold().FontSize(14).FontColor(Colors.Red.Medium);
        });
    }
}
