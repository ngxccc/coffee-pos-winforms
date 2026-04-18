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

        var bootstrapConfig = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        UiTheme.LoadFromConfig(bootstrapConfig);
        UiTheme.ApplyTheme();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(bootstrapConfig)
            .CreateLogger();

        IHost? host = null;

        try
        {
            Log.Information("=== KHỞI ĐỘNG PHẦN MỀM COFFEE POS ===");

            SqlFileLoader.ValidateAllSqlKeys();

            string connStr = bootstrapConfig.GetConnectionString("DefaultConnection")
                             ?? throw new Exception("Chưa cấu hình ConnectionString!");
            host = CreateHostBuilder(connStr).UseSerilog().Build();

            host.Start();

            var appState = host.Services.GetRequiredService<AppStateManager>();
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
