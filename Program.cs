using CoffeePOS.Data;
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

        var host = CreateHostBuilder().UseSerilog().Build();

        var config = host.Services.GetRequiredService<IConfiguration>();
        string connStr = config.GetConnectionString("DefaultConnection")
                         ?? throw new Exception("Chưa cấu hình ConnectionString!");

        Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(config)
        .CreateLogger();

        // Thay vì: Application.Run(new MainForm(...));
        // Bây giờ Container sẽ tự lo việc new MainForm và bơm Repo vào nó.
        try
        {
            Log.Information("=== KHỞI ĐỘNG PHẦN MỀM COFFEE POS ===");

            DbInitializer.Initialize(connStr);
            Core.TimeKeeper.Initialize(connStr);
            Core.InvoiceGenerator.Initialize();

            // Tự động gọi ExecuteAsync của tất cả các BackgroundService
            host.Start();

            var appState = host.Services.GetRequiredService<Core.AppStateManager>();
            Application.Run(appState);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Phần mềm sập toàn tập lúc khởi động!");
            MessageBox.Show($"Lỗi khởi động: {ex.Message}\nKiểm tra lại appsettings.json!", "Critical Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            host.StopAsync().GetAwaiter().GetResult();
            host.Dispose();
            Log.Information("=== TẮT PHẦN MỀM ===");
            Log.CloseAndFlush();
        }
    }

    static IHostBuilder CreateHostBuilder() =>
        Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                // context.Configuration chính là file json đã nạp ở trên
                string connStr = context.Configuration.GetConnectionString("DefaultConnection")
                                 ?? throw new InvalidOperationException("Chuỗi kết nối 'DefaultConnection' không tìm thấy!");

                var dataSource = NpgsqlDataSource.Create(connStr);

                services.AddSingleton(dataSource);
                services.AddSingleton<Core.AppStateManager>();
                services.AddSingleton<Core.IUserSession, Core.UserSession>();
                services.AddSingleton<Core.PdfPrintQueue>();

                services.AddHostedService<Core.PdfPrintWorker>();
                services.AddHostedService<Core.TrashCleanupWorker>();

                services.Scan(scan => scan
                    .FromAssemblies(typeof(Program).Assembly) // Quét từ lõi project

                    // QUY TẮC A: Repositories và Services
                    // Tự động map Interface -> Class (vd: IProductRepository -> ProductRepository)
                    .AddClasses(classes => classes.Where(c =>
                        c.Name.EndsWith("Repository") ||
                        c.Name.EndsWith("Service")))
                    .AsImplementedInterfaces()
                    .WithSingletonLifetime()

                    // QUY TẮC B: Tất cả các Forms
                    // Map chính nó (AsSelf) để gọi đẻ ra Form
                    .AddClasses(classes => classes.Where(c => c.Name.EndsWith("Form")))
                    .AsSelf()
                    .WithTransientLifetime()

                    // QUY TẮC C: Tất cả các UserControls (UC_*)
                    .AddClasses(classes => classes.Where(c => c.Name.StartsWith("UC_")))
                    .AsSelf()
                    .WithTransientLifetime()
                );
            });
}
