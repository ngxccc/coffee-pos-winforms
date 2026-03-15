using System.Threading.Channels;
using QuestPDF.Fluent;

namespace CoffeePOS.Core;

public class PdfPrintQueue
{
    private readonly Channel<IPdfPayload> _queue = Channel.CreateUnbounded<IPdfPayload>();

    public async ValueTask EnqueueJobAsync(IPdfPayload job)
    {
        await _queue.Writer.WriteAsync(job);
    }

    public async ValueTask<IPdfPayload> DequeueJobAsync(CancellationToken cancellationToken)
    {
        return await _queue.Reader.ReadAsync(cancellationToken);
    }

    //     private async Task ProcessQueueAsync()
    // {
    //     await foreach (var payload in _queue.Reader.ReadAllAsync())
    //     {
    //         try
    //         {
    //             // 🔥 PATTERN MATCHING QUYỀN LỰC CỦA C#
    //             Document document = payload switch
    //             {
    //                 BillPrintPayload bill => GenerateBillDocument(bill),
    //                 ShiftReportPrintPayload report => GenerateZReportDocument(report),
    //                 _ => throw new NotSupportedException("Đéo biết in loại giấy này!")
    //             };

    //             // Lệnh in thực tế xuống máy in (Giữ nguyên)
    //             // document.GeneratePdfAndPrint();
    //         }
    //         catch (Exception ex)
    //         {
    //             Console.WriteLine($"Print Error: {ex.Message}");
    //         }
    //     }
    // }
}
