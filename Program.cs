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

        var bootstrapConfig = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(bootstrapConfig)
            .CreateLogger();

        IHost? host = null;

        try
        {
            Log.Information("=== KHỞI ĐỘNG PHẦN MỀM COFFEE POS ===");

            string connStr = bootstrapConfig.GetConnectionString("DefaultConnection")
                             ?? throw new Exception("Chưa cấu hình ConnectionString!");

            host = CreateHostBuilder(connStr).UseSerilog().Build();

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
