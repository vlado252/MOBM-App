using System.Collections.Generic;
using System.IO;
using System.Text;
using MOBM_App.Models;

namespace MOBM_App.Services
{
    public static class ExportService
    {
        public static void ExportToCsv(List<Product> products, string path)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ID,Name,Category,Price,Stock");
            foreach (var p in products) sb.AppendLine($"{p.Id},{p.Name},{p.Category},{p.Price},{p.Stock}");
            File.WriteAllText(path, sb.ToString());
        }
    }
}