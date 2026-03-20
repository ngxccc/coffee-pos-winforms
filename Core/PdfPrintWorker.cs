using Microsoft.Extensions.Hosting;
using Serilog;

namespace CoffeePOS.Core;

// C# 12 Primary Constructor
public class PdfPrintWorker(PdfPrintQueue printQueue) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            // SỨC MẠNH CỦA AWAIT FOREACH: Ngắn gọn, thanh lịch, tự động chờ, tự động dừng!
            await foreach (var job in printQueue.StreamJobsAsync(stoppingToken))
            {
                try
                {
                    // Xử lý Job
                    await InvoiceGenerator.GenerateAndOpenPdfAsync(job);
                }
                catch (Exception ex)
                {
                    Log.Error($"[Lỗi in PDF Background - Bỏ qua Bill này]: {ex.Message}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            Log.Information("[Thợ in PDF]: Đang dọn dẹp và tắt máy...");
        }
    }
}
