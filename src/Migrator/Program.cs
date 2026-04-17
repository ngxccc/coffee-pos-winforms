using System.Reflection;
using DbUp;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

// PERF: Lấy chuỗi kết nối từ section ConnectionStrings
var connectionString = config.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Không tìm thấy DefaultConnection trong appsettings.json!");

EnsureDatabase.For.PostgresqlDatabase(connectionString);

var upgrader = DeployChanges.To
    .PostgresqlDatabase(connectionString)
    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
    .WithVariablesDisabled()
    .LogToConsole()
    .Build();

var result = upgrader.PerformUpgrade();

if (!result.Successful)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(result.Error);
    Console.ResetColor();

    return -1;
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Database setup complete!");
Console.ResetColor();

return 0;
