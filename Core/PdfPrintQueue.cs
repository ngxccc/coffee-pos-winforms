using System.Threading.Channels;

namespace CoffeePOS.Core;

public class PdfPrintQueue
{
    private readonly Channel<PdfJobPayload> _queue = Channel.CreateUnbounded<PdfJobPayload>();

    public async ValueTask EnqueueJobAsync(PdfJobPayload job)
    {
        await _queue.Writer.WriteAsync(job);
    }

    public async ValueTask<PdfJobPayload> DequeueJobAsync(CancellationToken cancellationToken)
    {
        return await _queue.Reader.ReadAsync(cancellationToken);
    }
}
