using System.Diagnostics;
using CoffeePOS.Core;
using CoffeePOS.Extensions;
using CoffeePOS.Shared.Enums;
using CoffeePOS.Shared.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Serilog;

namespace CoffeePOS;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        // WHY: GDI+ text rendering optimization for High-DPI screens
        AntdUI.Config.TextRenderingHighQuality = true;
        AntdUI.Config.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
        AntdUI.Config.Theme().Dark("#000", "#fff").Light("#fff", "#000").FormBorderColor();

        var bootstrapConfig = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(bootstrapConfig)
            .CreateLogger();

        IHost? host = null;

        try
        {
            Log.Information("=== KHỞI ĐỘNG PHẦN MỀM COFFEE POS ===");
            var startupTimer = Stopwatch.StartNew();

            SqlFileLoader.ValidateAllSqlKeys();
            Log.Information("Startup phase SqlFileLoader.ValidateAllSqlKeys: {ElapsedMs} ms", startupTimer.ElapsedMilliseconds);

            string connStr = bootstrapConfig.GetConnectionString("DefaultConnection")
                             ?? throw new Exception("Chưa cấu hình ConnectionString!");

            host = CreateHostBuilder(connStr).UseSerilog().Build();
            Log.Information("Startup phase Host.Build: {ElapsedMs} ms", startupTimer.ElapsedMilliseconds);

            host.Start();
            Log.Information("Startup phase host.Start: {ElapsedMs} ms", startupTimer.ElapsedMilliseconds);

            var appState = host.Services.GetRequiredService<AppStateManager>();
            Log.Information("Startup phase Resolve AppStateManager: {ElapsedMs} ms", startupTimer.ElapsedMilliseconds);

            Log.Information("Startup complete - opening first form at {ElapsedMs} ms", startupTimer.ElapsedMilliseconds);
            Application.Run(appState);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Phần mềm sập toàn tập lúc khởi động!");
            MessageBoxHelper.Error($"Lỗi khởi động: {ex.Message}", "Critical Error");
        }
        finally
        {
            if (host != null)
            {
                host.StopAsync().GetAwaiter().GetResult();
                host.Dispose();
            }
            Log.Information("=== TẮT PHẦN MỀM ===");
            Log.CloseAndFlush();
        }
    }

    static IHostBuilder CreateHostBuilder(string connStr) =>
        Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                var dataSourceBuilder = new NpgsqlDataSourceBuilder(connStr);
                dataSourceBuilder.MapEnum<UserRole>("user_role");

                var npgsqlDataSource = dataSourceBuilder.Build();

                services.AddSingleton(npgsqlDataSource);
                services.AddSingleton<AppStateManager>();
                services.AddSingleton<IUserSession, UserSession>();
                services.AddSingleton<IUiFactory, UiFactory>();
                services.AddSingleton<PdfPrintQueue>();
                services.AddMemoryCache();

                services.AddHostedService<PdfPrintWorker>();
                services.AddHostedService<TrashCleanupWorker>();

                services.AddCoffeePosServices();
            });
}
