namespace CoffeePOS.Shared.Helpers;

public static class SqlKeys
{
    public static class Dashboard
    {
        public const string GetTodaySummary = "Dashboard.get_today_summary.sql";
        public const string GetRevenueChart = "Dashboard.get_revenue_chart.sql";
        public const string GetTopProducts = "Dashboard.get_top_products.sql";
    }

    public static class Bill
    {
        public const string InsertBill = "Bill.insert_bill.sql";
        public const string InsertBillDetail = "Bill.insert_bill_detail.sql";
        public const string GetBillDetails = "Bill.get_bill_details.sql";
        public const string CancelBill = "Bill.cancel_bill.sql";
        public const string RestoreBill = "Bill.restore_bill.sql";
        public const string GetTodayBillsByUser = "Bill.get_today_bills_by_user.sql";
        public const string GetBillsByDateRange = "Bill.get_bills_by_date_range.sql";
    }

    public static class ShiftReport
    {
        public const string GetShiftSummary = "ShiftReport.get_shift_summary.sql";
        public const string InsertShiftReport = "ShiftReport.insert_shift_report.sql";
    }

    public static class User
    {
        public const string Authenticate = "User.authenticate.sql";
        public const string GetAll = "User.get_all.sql";
        public const string Insert = "User.insert.sql";
        public const string UpdateProfile = "User.update_profile.sql";
        public const string SetActiveStatus = "User.set_active_status.sql";
        public const string DeactivateUser = "User.deactivate_user.sql";
        public const string UpdatePassword = "User.update_password.sql";
    }

    public static class Product
    {
        public const string GetAll = "Product.get_all.sql";
        public const string GetById = "Product.get_by_id.sql";
        public const string Insert = "Product.insert.sql";
        public const string Update = "Product.update.sql";
        public const string SoftDelete = "Product.soft_delete.sql";
        public const string GetDeleted = "Product.get_deleted.sql";
        public const string GetDeletedById = "Product.get_deleted_by_id.sql";
        public const string Restore = "Product.restore.sql";
    }

    public static class Category
    {
        public const string GetAll = "Category.get_all.sql";
        public const string GetById = "Category.get_by_id.sql";
        public const string Insert = "Category.insert.sql";
        public const string Update = "Category.update.sql";
        public const string SoftDelete = "Category.soft_delete.sql";
        public const string SoftDeleteProductsByCategory = "Category.soft_delete_products_by_category.sql";
        public const string GetDeleted = "Category.get_deleted.sql";
        public const string GetDeletedById = "Category.get_deleted_by_id.sql";
        public const string Restore = "Category.restore.sql";
    }

    public static class Topping
    {
        public const string Get = "Topping.get.sql";
        public const string GetAll = "Topping.get_all.sql";
        public const string GetDeleted = "Topping.get_deleted.sql";
        public const string Insert = "Topping.insert.sql";
        public const string Restore = "Topping.restore.sql";
        public const string Update = "Topping.update.sql";
        public const string SoftDelete = "Topping.soft_delete.sql";
    }

    public static class ProductSize
    {
        public const string GetByProductId = "ProductSize.get_by_product_id.sql";
        public const string Insert = "ProductSize.insert.sql";
        public const string Update = "ProductSize.update.sql";
        public const string Delete = "ProductSize.delete.sql";
    }
}
