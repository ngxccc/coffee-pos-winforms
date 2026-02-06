using System.Diagnostics;

namespace CoffeePOS.Core;

public static class TimeKeeper
{
    private static DateTime _serverStartTime;
    private static Stopwatch _appUptime = null!;
    private static bool _isInitialized = false;

    // Giả lập hàm gọi xuống DB lấy giờ (Sau này thay bằng Repo call)
    // Query: SELECT GETDATE() FROM System
    private static DateTime FetchServerTimeFromDB()
    {
        // Tạm thời return DateTime.Now để test, sau này thay code SQL vào đây
        return DateTime.Now;
    }

    public static void Initialize()
    {
        _serverStartTime = FetchServerTimeFromDB();
        _appUptime = new Stopwatch();
        _appUptime.Start();
        _isInitialized = true;
    }

    public static DateTime Now
    {
        get
        {
            if (!_isInitialized)
            {
                // Fallback nếu quên Init
                return DateTime.Now;
            }

            // Công thức: Giờ Server lúc mở app + Thời gian App đã chạy
            return _serverStartTime.Add(_appUptime.Elapsed);
        }
    }

    // Hàm tính thời gian ngồi (Duration)
    // checkInTime: Là giờ server đã lưu trong DB lúc khách vào
    public static TimeSpan GetDuration(DateTime checkInTime)
    {
        // Lấy giờ hiện tại (đã sync) trừ đi giờ checkin
        var duration = Now - checkInTime;
        return duration < TimeSpan.Zero ? TimeSpan.Zero : duration;
    }

    public static void Resync()
    {
        var newServerTime = FetchServerTimeFromDB();
        _serverStartTime = newServerTime;
        _appUptime.Restart();
    }
}
