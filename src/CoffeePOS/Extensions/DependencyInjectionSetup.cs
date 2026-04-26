using CoffeePOS.Data.Repositories;
using CoffeePOS.Data.Repositories.Contracts;
using CoffeePOS.Features.Admin;
using CoffeePOS.Features.Admin.Controls;
using CoffeePOS.Features.Billing;
using CoffeePOS.Features.Products;
using CoffeePOS.Features.Sidebar;
using CoffeePOS.Forms;
using CoffeePOS.Forms.Core;
using CoffeePOS.Services;
using CoffeePOS.Services.Contracts.Commands;
using CoffeePOS.Services.Contracts.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePOS.Extensions;

public static class DependencyInjectionSetup
{
    // PERF: Zero-reflection AOT-safe dependency registration. Time Complexity: O(1)
    public static IServiceCollection AddCoffeePosServices(this IServiceCollection services)
    {
        // QUY TẮC A: Repositories
        services.AddSingleton<IUserRepository, UserRepository>();
        services.AddSingleton<IProductRepository, ProductRepository>();
        services.AddSingleton<ICategoryRepository, CategoryRepository>();
        services.AddSingleton<IShiftReportRepository, ShiftReportRepository>();
        services.AddSingleton<IBillRepository, BillRepository>();
        services.AddSingleton<IDashboardRepository, DashboardRepository>();
        services.AddSingleton<IToppingRepository, ToppingRepository>();
        services.AddSingleton<IProductSizeRepository, ProductSizeRepository>();
        services.AddSingleton<IToppingRepository, ToppingRepository>();

        // QUY TẮC A: Services
        services.AddSingleton<IUserService, UserService>();
        services.AddSingleton<IUserQueryService, UserQueryService>();

        services.AddSingleton<IProductService, ProductService>();
        services.AddSingleton<IProductQueryService, ProductQueryService>();

        services.AddSingleton<ICategoryService, CategoryService>();
        services.AddSingleton<ICategoryQueryService, CategoryQueryService>();

        services.AddSingleton<IShiftReportService, ShiftReportService>();
        services.AddSingleton<IShiftReportQueryService, ShiftReportQueryService>();

        services.AddSingleton<IBillService, BillService>();
        services.AddSingleton<IBillQueryService, BillQueryService>();

        services.AddSingleton<IDashboardQueryService, DashboardQueryService>();

        services.AddSingleton<IProductSizeService, ProductSizeService>();
        services.AddSingleton<IProductSizeQueryService, ProductSizeQueryService>();

        services.AddSingleton<IToppingService, ToppingService>();
        services.AddSingleton<IToppingQueryService, ToppingQueryService>();

        // QUY TẮC B: Forms (Transient vì form được dispose sau khi đóng)
        services.AddTransient<LoginForm>();
        services.AddTransient<CashierWorkspaceForm>();
        services.AddTransient<AdminDashboardForm>();
        services.AddTransient(typeof(DynamicModalShell<>));

        // QUY TẮC C: UserControls (Transient để mỗi lần render lấy instance mới)
        services.AddTransient<UC_Sidebar>();
        services.AddTransient<UC_ShiftReportFields>();
        services.AddTransient<UC_Profiles>();

        services.AddTransient<UC_Menu>();
        services.AddTransient<UC_ProductItem>();

        services.AddTransient<UC_Billing>();
        services.AddTransient<UC_BillHistory>();
        services.AddTransient<UC_BillDetail>();
        services.AddTransient<UC_BillItem>();
        services.AddTransient<UC_ProductCustomization>();

        services.AddTransient<UC_Dashboard>();
        services.AddTransient<UC_ManageBills>();
        services.AddTransient<UC_ManageCategories>();
        services.AddTransient<UC_ManageProducts>();
        services.AddTransient<UC_ManageUsers>();

        services.AddTransient<UC_CategoryEditor>();
        services.AddTransient<UC_ProductEditor>();
        services.AddTransient<UC_UserAccountEditor>();

        services.AddTransient<UC_ProductSizeEditor>();
        services.AddTransient<UC_ManageProductSizes>();

        services.AddTransient<UC_ToppingEditor>();
        services.AddTransient<UC_ManageToppings>();

        services.AddTransient<UC_ManageShiftReports>();

        return services;
    }
}
