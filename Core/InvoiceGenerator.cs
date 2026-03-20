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

    public static async Task GenerateAndOpenPdfAsync(IPdfPayload payload)
    {
        var (document, fileNamePrefix) = payload switch
        {
            BillPrintPayload bill => (CreateBillDocument(bill), $"Bill_{bill.BillId}_{(bill.IsReprint ? "reprint_" : "")}"),
            ShiftReportPrintPayload report => (CreateShiftReportDocument(report), $"ZReport_{report.CashierName}_"),
            _ => throw new NotSupportedException("Loại tài liệu in không được hỗ trợ!")
        };

        string directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Invoices");
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

        string filePath = Path.Combine(directory, $"{fileNamePrefix}{DateTime.Now:yyyyMMdd_HHmmss_fff}.pdf");

        await Task.Run(() =>
        {
            document.GeneratePdf(filePath);
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

                if (!string.IsNullOrWhiteSpace(item.Note))
                {
                    column.Item().PaddingLeft(10).Text($"- Ghi chú: {item.Note}").FontSize(9).FontColor(Colors.Grey.Darken2).Italic();
                }
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

    private static Document CreateBillDocument(BillPrintPayload payload)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                ApplyStandardPageSettings(page, 10);

                page.Header().Element(x => ComposeHeader(x, payload.BillId, payload.BuzzerNumber));
                page.Content().Element(x => ComposeContent(x, payload.Details));
                page.Footer().Element(x => ComposeFooter(x, payload.TotalAmount));

                if (payload.IsReprint)
                {
                    page.Background().Column(col =>
                    {
                        for (int i = 0; i < 15; i++)
                        {
                            col.Item()
                               .Height(120)
                               .AlignBottom()
                               .Unconstrained()
                               .Rotate(-45)
                               .Text("BẢN SAO")
                               .FontSize(50)
                               .FontColor(Colors.Grey.Lighten3)
                               .SemiBold();
                        }
                    });
                }
            });
        });
    }

    private static Document CreateShiftReportDocument(ShiftReportPrintPayload payload)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                ApplyStandardPageSettings(page, 11);

                page.Content().Column(col =>
                {
                    col.Spacing(5);
                    col.Item().Text("BIÊN LAI CHỐT CA").FontSize(16).SemiBold().AlignCenter();

                    col.Item().Text($"Thu ngân: {payload.CashierName}");
                    col.Item().Text($"Bắt đầu: {payload.StartTime:dd/MM/yyyy HH:mm}");
                    col.Item().Text($"Kết thúc: {payload.EndTime:dd/MM/yyyy HH:mm}");

                    col.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Tổng số hóa đơn:");
                        r.RelativeItem().AlignRight().Text(payload.TotalBills.ToString());
                    });
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Tiền hệ thống:");
                        r.RelativeItem().AlignRight().Text($"{payload.ExpectedCash:N0} đ");
                    });
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Tiền mặt thực tế:").SemiBold();
                        r.RelativeItem().AlignRight().Text($"{payload.ActualCash:N0} đ").SemiBold();
                    });

                    string varianceSign = payload.Variance > 0 ? "+" : "";
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Độ lệch:");
                        r.RelativeItem().AlignRight().Text($"{varianceSign}{payload.Variance:N0} đ").FontColor(payload.Variance < 0 ? Colors.Red.Medium : Colors.Black);
                    });

                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Lý do lệch tiền:");
                        r.RelativeItem().AlignRight().Text(payload.Note.ToString());
                    });

                    col.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    col.Item().PaddingTop(15).AlignCenter().Text("Chữ ký thu ngân");
                    col.Item().PaddingTop(30).AlignCenter().Text("........................................");
                });
            });
        });
    }

    private static void ApplyStandardPageSettings(PageDescriptor page, float fontSize = 10)
    {
        page.ContinuousSize(80, Unit.Millimetre);
        page.Margin(2, Unit.Millimetre);
        page.PageColor(Colors.White);
        page.DefaultTextStyle(x => x.FontSize(fontSize).FontFamily(Fonts.Arial));
    }
}
