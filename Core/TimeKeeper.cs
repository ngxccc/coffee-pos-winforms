using System.Diagnostics;
using Npgsql;

namespace CoffeePOS.Core;

public static class TimeKeeper
{
    private static DateTime _serverStartTime;
    private static Stopwatch _appUptime = null!;
    private static bool _isInitialized = false;
    private static readonly string _connStr = "";

    private static DateTime FetchServerTimeFromDB()
    {
        try
        {
            using var conn = new NpgsqlConnection(_connStr);
            conn.Open();

            // SELECT NOW() trả về DateTime bao gồm cả TimeZone
            using var cmd = new NpgsqlCommand("SELECT NOW()", conn);
            var result = cmd.ExecuteScalar();

            if (result != null && result != DBNull.Value)
            {
                return Convert.ToDateTime(result);
            }
        }
        catch (Exception ex)
        {
            // Fallback nếu mất mạng/lỗi DB: Dùng giờ máy tính tạm
            Debug.WriteLine($"Lỗi lấy giờ Server: {ex.Message}");
        }

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
