using MOBM_App.Models;

namespace MOBM_App.Helpers
{
    public static class Permissions
    {
        // === КАТАЛОГ ТОВАРОВ ===
        public static bool CanViewCatalog(Role role) => true;

        public static bool CanAddProduct(Role role)
            => role == Role.Admin || role == Role.Manager;

        public static bool CanEditProduct(Role role)
            => role == Role.Admin;

        public static bool CanDeleteProduct(Role role)
            => role == Role.Admin;

        // === ЗАКАЗЫ ===
        public static bool CanCreateOrder(Role role)
            => role == Role.Admin || role == Role.Manager;

        public static bool CanViewOrderHistory(Role role) => true;

        public static bool CanEditOrder(Role role)
            => role == Role.Admin || role == Role.Manager;

        public static bool CanExportData(Role role)
            => role == Role.Admin || role == Role.Manager;

        // === СКЛАД ===
        public static bool CanViewWarehouse(Role role) => true;

        public static bool CanManageStock(Role role)
            => role == Role.Admin || role == Role.Warehouse;

        public static bool CanViewStockOperations(Role role)
            => role == Role.Admin || role == Role.Warehouse;
    }
}