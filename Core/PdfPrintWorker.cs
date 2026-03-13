using Microsoft.Extensions.Hosting;

namespace CoffeePOS.Core;

public class PdfPrintWorker(PdfPrintQueue printQueue) : BackgroundService
{
    private readonly PdfPrintQueue _printQueue = printQueue;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var job = await _printQueue.DequeueJobAsync(stoppingToken);

                await InvoiceGenerator.GenerateAndOpenPdfAsync(job.BillId, job.BuzzerNumber, job.TotalAmount, job.Details);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Lỗi in PDF Background]: {ex.Message}");
            }
        }
    }
}
