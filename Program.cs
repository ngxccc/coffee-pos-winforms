using CoffeePOS.Data;
using CoffeePOS.Data.Repositories;
using CoffeePOS.Data.Repositories.Impl;
using CoffeePOS.Features.Products;
using CoffeePOS.Forms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;

namespace CoffeePOS;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        var host = CreateHostBuilder().Build();

        var config = host.Services.GetRequiredService<IConfiguration>();
        string connStr = config.GetConnectionString("DefaultConnection")
                         ?? throw new Exception("Chưa cấu hình ConnectionString!");

        // Thay vì: Application.Run(new MainForm(...));
        // Bây giờ Container sẽ tự lo việc new MainForm và bơm Repo vào nó.
        try
        {
            DbInitializer.Initialize(connStr);
            Core.TimeKeeper.Initialize(connStr);

            var mainForm = host.Services.GetRequiredService<MainForm>();
            Application.Run(mainForm);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khởi động: {ex.Message}\nKiểm tra lại appsettings.json!", "Critical Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                services.AddSingleton<IBillRepository, BillRepository>();
                services.AddSingleton<IProductRepository, ProductRepository>();

                services.AddTransient<MainForm>();
                services.AddTransient<UC_Menu>();
            });
}
