using System.Threading.Channels;

namespace CoffeePOS.Core;

public class PdfPrintQueue
{
    private readonly Channel<IPdfPayload> _queue;

    public PdfPrintQueue()
    {
        var options = new BoundedChannelOptions(capacity: 50)
        {
            // Nếu Queue full 50 tờ, Thu ngân bấm phát thứ 51 sẽ phải ĐỢI (Wait)
            FullMode = BoundedChannelFullMode.Wait,

            // Chỉ có 1 thằng nhét (Main Thread) và 1 thằng lôi ra (Worker)
            SingleReader = true,
            SingleWriter = false
        };

        _queue = Channel.CreateBounded<IPdfPayload>(options);
    }

    public async ValueTask EnqueueJobAsync(IPdfPayload job)
    {
        await _queue.Writer.WriteAsync(job);
    }

    public async ValueTask<IPdfPayload> DequeueJobAsync(CancellationToken cancellationToken)
    {
        return await _queue.Reader.ReadAsync(cancellationToken);
    }
}
