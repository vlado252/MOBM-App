using System;

namespace MOBM_App.Models
{
    public class StockOperation
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string OperationType { get; set; }
        public int Quantity { get; set; }
        public DateTime Date { get; set; }
        public string Comment { get; set; }
        public string UserName { get; set; }
    }

    public class WarehouseStats
    {
        public int TotalItems { get; set; }
        public int TotalStock { get; set; }
        public decimal TotalValue { get; set; }
        public int LowStockItems { get; set; }
    }
}