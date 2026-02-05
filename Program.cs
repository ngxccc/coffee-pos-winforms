using CoffeePOS.Data.Repositories;
using CoffeePOS.Data.Repositories.Impl;
using CoffeePOS.Forms;

namespace CoffeePOS;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();

        string connStr = "Host=localhost;Port=5432;Username=postgres;Password=7ee7e924e9bc432aba7529d89c98bb6f;Database=CoffeePOS";

        IBillRepository billRepo = new BillRepository(connStr);

        Application.Run(new MainForm(billRepo));
    }
}
