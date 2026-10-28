using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Windows;
using MOBM_App.Helpers;
using MOBM_App.Models;

namespace MOBM_App.Services
{
    public static class DatabaseService
    {
        private static string _dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mobm.db");
        private static string _connStr => $"Data Source={_dbPath};Version=3;BusyTimeout=5000;Journal Mode=WAL;";

        public static void Initialize()
        {
            try
            {
                if (File.Exists(_dbPath) && new FileInfo(_dbPath).Length < 500)
                    File.Delete(_dbPath);

                using (var c = new SQLiteConnection(_connStr))
                {
                    c.Open();
                    new SQLiteCommand(@"CREATE TABLE IF NOT EXISTS Users (Id INTEGER PRIMARY KEY AUTOINCREMENT, Login TEXT UNIQUE, Password TEXT, Name TEXT, Email TEXT, Role INTEGER)", c).ExecuteNonQuery();
                    new SQLiteCommand(@"CREATE TABLE IF NOT EXISTS Products (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT, Category INTEGER, Price REAL, Stock INTEGER, Description TEXT, Material TEXT, Dimensions TEXT)", c).ExecuteNonQuery();

                    // ✅ ИСПРАВЛЕНО: Убрали DROP TABLE - теперь заказы сохраняются
                    new SQLiteCommand(@"CREATE TABLE IF NOT EXISTS Orders (Id INTEGER PRIMARY KEY AUTOINCREMENT, UserId INTEGER, Date TEXT, TotalPrice REAL, Status TEXT DEFAULT 'Выполнен')", c).ExecuteNonQuery();

                    // ✅ ИСПРАВЛЕНО: Убрали DROP TABLE - теперь товары заказов сохраняются
                    new SQLiteCommand(@"CREATE TABLE IF NOT EXISTS OrderItems (Id INTEGER PRIMARY KEY AUTOINCREMENT, OrderId INTEGER, ProductId INTEGER, Quantity INTEGER, Price REAL)", c).ExecuteNonQuery();

                    // Создаём таблицу операций склада
                    new SQLiteCommand(@"CREATE TABLE IF NOT EXISTS StockOperations (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT, 
                        ProductId INTEGER, 
                        OperationType TEXT, 
                        Quantity INTEGER, 
                        Date TEXT, 
                        Comment TEXT, 
                        UserId INTEGER)", c).ExecuteNonQuery();
                    // В методе Initialize() после создания таблицы Users добавь:
                    new SQLiteCommand(@"CREATE TABLE IF NOT EXISTS EmployeeRequests (
    Id INTEGER PRIMARY KEY AUTOINCREMENT, 
    FullName TEXT, 
    Login TEXT UNIQUE, 
    Password TEXT, 
    Email TEXT, 
    Phone TEXT, 
    Position TEXT, 
    DesiredRole INTEGER, 
    Status TEXT DEFAULT 'На рассмотрении',
    SubmittedDate TEXT,
    AdminComment TEXT)", c).ExecuteNonQuery();
                    // Таблица промокодов
                    new SQLiteCommand(@"CREATE TABLE IF NOT EXISTS PromoCodes (
    Id INTEGER PRIMARY KEY AUTOINCREMENT, 
    Code TEXT UNIQUE, 
    DiscountPercent REAL, 
    ValidUntil TEXT, 
    IsActive INTEGER DEFAULT 1,
    Description TEXT)", c).ExecuteNonQuery();

                    // Добавляем тестовые промокоды
                    long promoCount = (long)new SQLiteCommand("SELECT COUNT(*) FROM PromoCodes", c).ExecuteScalar();
                    if (promoCount == 0)
                    {
                        // Летняя скидка
                        new SQLiteCommand(
                            "INSERT INTO PromoCodes (Code, DiscountPercent, ValidUntil, IsActive, Description) VALUES ('SUMMER2026', 10, '2026-08-31', 1, 'Летняя скидка 10%')", c).ExecuteNonQuery();

                        // Новогодняя скидка
                        new SQLiteCommand(
                            "INSERT INTO PromoCodes (Code, DiscountPercent, ValidUntil, IsActive, Description) VALUES ('NEWYEAR2026', 15, '2026-12-31', 1, 'Новогодняя скидка 15%')", c).ExecuteNonQuery();

                        // Скидка для новичков
                        new SQLiteCommand(
                            "INSERT INTO PromoCodes (Code, DiscountPercent, ValidUntil, IsActive, Description) VALUES ('FIRST5', 5, '2027-12-31', 1, 'Скидка 5% на первый заказ')", c).ExecuteNonQuery();
                    }
                    long count = (long)new SQLiteCommand("SELECT COUNT(*) FROM Users", c).ExecuteScalar();
                    if (count == 0) SeedData(c);
                }
            }
            catch (Exception ex)
            {
                if (File.Exists(_dbPath)) File.Delete(_dbPath);
                throw new Exception($"Ошибка инициализации БД: {ex.Message}");
            }
        }

        private static void SeedData(SQLiteConnection c)
        {
            new SQLiteCommand("INSERT INTO Users (Login, Password, Name, Email, Role) VALUES ('admin', '123', 'Иванов А.А.', 'admin@mobm.by', 0)", c).ExecuteNonQuery();
            new SQLiteCommand("INSERT INTO Users (Login, Password, Name, Email, Role) VALUES ('manager', '123', 'Петров Б.Б.', 'man@mobm.by', 1)", c).ExecuteNonQuery();
            new SQLiteCommand("INSERT INTO Users (Login, Password, Name, Email, Role) VALUES ('warehouse', '123', 'Сидоров В.В.', 'wh@mobm.by', 2)", c).ExecuteNonQuery();

            new SQLiteCommand("INSERT INTO Products (Name, Category, Price, Stock, Description, Material, Dimensions) VALUES ('Стул Office Pro', 0, 150, 50, 'Эргономичный', 'Пластик/Металл', '50x50x100')", c).ExecuteNonQuery();
            new SQLiteCommand("INSERT INTO Products (Name, Category, Price, Stock, Description, Material, Dimensions) VALUES ('Стол Director', 0, 400, 20, 'Массив дуба', 'Дуб', '160x80x75')", c).ExecuteNonQuery();
            new SQLiteCommand("INSERT INTO Products (Name, Category, Price, Stock, Description, Material, Dimensions) VALUES ('Шкаф Metal', 0, 280, 15, 'Металлический', 'Сталь', '90x40x180')", c).ExecuteNonQuery();
            new SQLiteCommand("INSERT INTO Products (Name, Category, Price, Stock, Description, Material, Dimensions) VALUES ('Древесина сосна', 1, 50, 1000, 'Пиломатериал', 'Сосна', '6000x150x25')", c).ExecuteNonQuery();
            new SQLiteCommand("INSERT INTO Products (Name, Category, Price, Stock, Description, Material, Dimensions) VALUES ('ЛДСП Белый', 1, 35, 2000, 'Ламинированная плита', 'ДСП', '2800x2070x16')", c).ExecuteNonQuery();
        }

        public static List<User> GetUsers()
        {
            var list = new List<User>();
            using (var c = new SQLiteConnection(_connStr))
            {
                c.Open();
                using (var r = new SQLiteCommand("SELECT * FROM Users", c).ExecuteReader())
                    while (r.Read())
                        list.Add(new User { Id = Convert.ToInt32(r["Id"]), Login = r["Login"].ToString(), Password = r["Password"].ToString(), Name = r["Name"].ToString(), Email = r["Email"].ToString(), UserRole = (Role)Convert.ToInt32(r["Role"]) });
            }
            return list;
        }

        public static List<Product> GetProducts()
        {
            var list = new List<Product>();
            using (var c = new SQLiteConnection(_connStr))
            {
                c.Open();
                using (var r = new SQLiteCommand("SELECT * FROM Products", c).ExecuteReader())
                    while (r.Read())
                        list.Add(new Product
                        {
                            Id = Convert.ToInt32(r["Id"]),
                            Name = r["Name"]?.ToString() ?? "Без названия",
                            Category = (Category)Convert.ToInt32(r["Category"]),
                            Price = Convert.ToDecimal(r["Price"]),
                            Stock = Convert.ToInt32(r["Stock"]),
                            Description = r["Description"]?.ToString(),
                            Material = r["Material"]?.ToString(),
                            Dimensions = r["Dimensions"]?.ToString()
                        });
            }
            return list;
        }
        // Получить всех пользователей
        public static List<User> GetAllUsers()
        {
            var list = new List<User>();
            using (var c = new SQLiteConnection(_connStr))
            {
                c.Open();
                using (var r = new SQLiteCommand("SELECT * FROM Users ORDER BY Id", c).ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new User
                        {
                            Id = Convert.ToInt32(r["Id"]),
                            Login = r["Login"]?.ToString() ?? "",
                            Password = r["Password"]?.ToString() ?? "",
                            Name = r["Name"]?.ToString() ?? "",
                            Email = r["Email"]?.ToString() ?? "",
                            UserRole = (Role)Convert.ToInt32(r["Role"])
                        });
                    }
                }
            }
            return list;
        }

        // Обновить пользователя
        public static void UpdateUser(User user)
        {
            using (var c = new SQLiteConnection(_connStr))
            {
                c.Open();
                var cmd = new SQLiteCommand(
                    "UPDATE Users SET Login=@login, Password=@password, Name=@name, Email=@email, Role=@role WHERE Id=@id", c);
                cmd.Parameters.AddWithValue("@login", user.Login);
                cmd.Parameters.AddWithValue("@password", user.Password);
                cmd.Parameters.AddWithValue("@name", user.Name);
                cmd.Parameters.AddWithValue("@email", user.Email ?? "");
                cmd.Parameters.AddWithValue("@role", (int)user.UserRole);
                cmd.Parameters.AddWithValue("@id", user.Id);
                cmd.ExecuteNonQuery();
            }
        }

        // Удалить пользователя
        public static void DeleteUser(int userId)
        {
            using (var c = new SQLiteConnection(_connStr))
            {
                c.Open();

                // Сначала удаляем связанные заказы
                var cmd1 = new SQLiteCommand("DELETE FROM Orders WHERE UserId=@id", c);
                cmd1.Parameters.AddWithValue("@id", userId);
                cmd1.ExecuteNonQuery();

                // Затем удаляем пользователя
                var cmd2 = new SQLiteCommand("DELETE FROM Users WHERE Id=@id", c);
                cmd2.Parameters.AddWithValue("@id", userId);
                cmd2.ExecuteNonQuery();
            }
        }
        public static void UpdateProductStock(int productId, int newStock)
        {
            System.Diagnostics.Debug.WriteLine($"\n[DB] UpdateProductStock START");
            System.Diagnostics.Debug.WriteLine($"  Product ID: {productId}");
            System.Diagnostics.Debug.WriteLine($"  New Stock: {newStock}");

            try
            {
                using (var c = new SQLiteConnection(_connStr))
                {
                    c.Open();
                    var cmd = new SQLiteCommand(
                        "UPDATE Products SET Stock = @stock WHERE Id = @id", c);
                    cmd.Parameters.AddWithValue("@stock", newStock);
                    cmd.Parameters.AddWithValue("@id", productId);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    System.Diagnostics.Debug.WriteLine($"  Rows affected: {rowsAffected}");
                    System.Diagnostics.Debug.WriteLine($"  [DB] UpdateProductStock SUCCESS");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"  [DB] UpdateProductStock ERROR: {ex.Message}");
                throw;
            }
        }

        public static int SaveOrderHistory(Order order)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));
            if (order.Items == null || order.Items.Count == 0) throw new ArgumentException("Заказ пуст");

            using (var c = new SQLiteConnection(_connStr))
            {
                c.Open();
                using (var t = c.BeginTransaction())
                {
                    try
                    {
                        var cmdDelDraftItems = new SQLiteCommand(
                            "DELETE FROM OrderItems WHERE OrderId IN (SELECT Id FROM Orders WHERE UserId=@uid AND Status='Черновик')", c, t);
                        cmdDelDraftItems.Parameters.AddWithValue("@uid", order.UserId);
                        cmdDelDraftItems.ExecuteNonQuery();

                        var cmdDelDraftOrder = new SQLiteCommand(
                            "DELETE FROM Orders WHERE UserId=@uid AND Status='Черновик'", c, t);
                        cmdDelDraftOrder.Parameters.AddWithValue("@uid", order.UserId);
                        cmdDelDraftOrder.ExecuteNonQuery();

                        var cmdOrder = new SQLiteCommand(
                            "INSERT INTO Orders (UserId, Date, TotalPrice, Status) VALUES (@uid, @date, @total, @status)", c, t);

                        cmdOrder.Parameters.AddWithValue("@uid", Convert.ToInt32(order.UserId));
                        cmdOrder.Parameters.AddWithValue("@date", order.Date.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmdOrder.Parameters.AddWithValue("@total", Convert.ToDouble(order.TotalPrice));
                        cmdOrder.Parameters.AddWithValue("@status", order.Status ?? "Выполнен");

                        cmdOrder.ExecuteNonQuery();

                        long orderId = (long)new SQLiteCommand("SELECT last_insert_rowid()", c, t).ExecuteScalar();

                        foreach (var item in order.Items)
                        {
                            if (item.Product == null) continue;

                            var cmdItem = new SQLiteCommand(
                                "INSERT INTO OrderItems (OrderId, ProductId, Quantity, Price) VALUES (@oid, @pid, @qty, @price)", c, t);

                            cmdItem.Parameters.AddWithValue("@oid", Convert.ToInt64(orderId));
                            cmdItem.Parameters.AddWithValue("@pid", Convert.ToInt32(item.Product.Id));
                            cmdItem.Parameters.AddWithValue("@qty", Convert.ToInt32(item.Quantity));
                            cmdItem.Parameters.AddWithValue("@price", Convert.ToDouble(item.Product.Price));

                            cmdItem.ExecuteNonQuery();
                        }

                        t.Commit();
                        return Convert.ToInt32(orderId);
                    }
                    catch (Exception ex)
                    {
                        t.Rollback();
                        throw new Exception($"Ошибка БД при сохранении: {ex.Message}");
                    }
                }
            }
        }

        public static void SaveCartDraft(int userId, List<OrderItem> items, decimal total)
        {
            using (var c = new SQLiteConnection(_connStr))
            {
                c.Open();
                using (var t = c.BeginTransaction())
                {
                    try
                    {
                        new SQLiteCommand("DELETE FROM OrderItems WHERE OrderId IN (SELECT Id FROM Orders WHERE UserId=@uid AND Status='Черновик')", c, t).ExecuteNonQuery();
                        new SQLiteCommand("DELETE FROM Orders WHERE UserId=@uid AND Status='Черновик'", c, t).ExecuteNonQuery();

                        if (items.Count == 0) { t.Commit(); return; }

                        var cmdOrder = new SQLiteCommand(
                            "INSERT INTO Orders (UserId, Date, TotalPrice, Status) VALUES (@uid, @date, @total, 'Черновик')", c, t);
                        cmdOrder.Parameters.AddWithValue("@uid", userId);
                        cmdOrder.Parameters.AddWithValue("@date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmdOrder.Parameters.AddWithValue("@total", total);
                        cmdOrder.ExecuteNonQuery();

                        long orderId = (long)new SQLiteCommand("SELECT last_insert_rowid()", c, t).ExecuteScalar();

                        foreach (var item in items)
                        {
                            var cmdItem = new SQLiteCommand(
                                "INSERT INTO OrderItems (OrderId, ProductId, Quantity, Price) VALUES (@oid, @pid, @qty, @price)", c, t);
                            cmdItem.Parameters.AddWithValue("@oid", orderId);
                            cmdItem.Parameters.AddWithValue("@pid", item.Product.Id);
                            cmdItem.Parameters.AddWithValue("@qty", item.Quantity);
                            cmdItem.Parameters.AddWithValue("@price", item.Product.Price);
                            cmdItem.ExecuteNonQuery();
                        }
                        t.Commit();
                    }
                    catch { t.Rollback(); throw; }
                }
            }
        }

        public static List<OrderItem> LoadCartDraft(int userId)
        {
            var items = new List<OrderItem>();
            using (var c = new SQLiteConnection(_connStr))
            {
                c.Open();
                var cmd = new SQLiteCommand(
                    @"SELECT oi.ProductId, oi.Quantity 
                      FROM OrderItems oi 
                      JOIN Orders o ON oi.OrderId = o.Id 
                      WHERE o.UserId = @uid AND o.Status = 'Черновик' 
                      ORDER BY o.Id DESC LIMIT 1", c);
                cmd.Parameters.AddWithValue("@uid", userId);

                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        int prodId = Convert.ToInt32(r["ProductId"]);
                        int qty = Convert.ToInt32(r["Quantity"]);

                        var product = GetProducts().FirstOrDefault(p => p.Id == prodId);
                        if (product != null)
                        {
                            items.Add(new OrderItem { Product = product, Quantity = qty });
                        }
                    }
                }
            }
            return items;
        }

        public static void ClearCartDraft(int userId)
        {
            using (var c = new SQLiteConnection(_connStr))
            {
                c.Open();
                new SQLiteCommand("DELETE FROM OrderItems WHERE OrderId IN (SELECT Id FROM Orders WHERE UserId=@uid AND Status='Черновик')", c).ExecuteNonQuery();
                new SQLiteCommand("DELETE FROM Orders WHERE UserId=@uid AND Status='Черновик'", c).ExecuteNonQuery();
            }
        }

        public static List<Order> GetOrdersHistory()
        {
            var list = new List<Order>();
            using (var c = new SQLiteConnection(_connStr))
            {
                c.Open();
                string query = @"
                    SELECT o.Id, o.UserId, o.Date, o.TotalPrice, o.Status, u.Name as UserName
                    FROM Orders o
                    LEFT JOIN Users u ON o.UserId = u.Id
                    WHERE o.Status != 'Черновик'
                    ORDER BY o.Date DESC";

                using (var r = new SQLiteCommand(query, c).ExecuteReader())
                {
                    while (r.Read())
                    {
                        var order = new Order
                        {
                            Id = Convert.ToInt32(r["Id"]),
                            UserId = Convert.ToInt32(r["UserId"]),
                            Date = DateTime.Parse(r["Date"].ToString()),
                            TotalPrice = Convert.ToDecimal(r["TotalPrice"]),
                            Status = r["Status"]?.ToString() ?? "Выполнен",
                            UserName = r["UserName"]?.ToString() ?? "Неизвестно"
                        };
                        list.Add(order);
                    }
                }
            }
            return list;
        }

        public static Order GetOrderById(int orderId)
        {
            try
            {
                using (var c = new SQLiteConnection(_connStr))
                {
                    c.Open();
                    var query = @"
                        SELECT o.Id, o.UserId, o.Date, o.TotalPrice, o.Status, u.Name as UserName
                        FROM Orders o
                        LEFT JOIN Users u ON o.UserId = u.Id
                        WHERE o.Id = @oid";

                    var cmd = new SQLiteCommand(query, c);
                    cmd.Parameters.AddWithValue("@oid", orderId);

                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            return new Order
                            {
                                Id = Convert.ToInt32(r["Id"]),
                                UserId = Convert.ToInt32(r["UserId"]),
                                Date = DateTime.Parse(r["Date"].ToString()),
                                TotalPrice = Convert.ToDecimal(r["TotalPrice"]),
                                Status = r["Status"]?.ToString() ?? "Выполнен",
                                UserName = r["UserName"]?.ToString() ?? "Неизвестно"
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заказа: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return null;
        }

        public static List<OrderItem> GetOrderDetails(int orderId)
        {
            var items = new List<OrderItem>();
            try
            {
                using (var c = new SQLiteConnection(_connStr))
                {
                    c.Open();

                    var query = @"
                        SELECT oi.ProductId, oi.Quantity, oi.Price, p.Name
                        FROM OrderItems oi
                        LEFT JOIN Products p ON oi.ProductId = p.Id
                        WHERE oi.OrderId = @oid";

                    var cmd = new SQLiteCommand(query, c);
                    cmd.Parameters.AddWithValue("@oid", orderId);

                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            items.Add(new OrderItem
                            {
                                Quantity = Convert.ToInt32(r["Quantity"]),
                                Product = new Product
                                {
                                    Id = Convert.ToInt32(r["ProductId"]),
                                    Name = r["Name"]?.ToString() ?? "Товар удален",
                                    Price = Convert.ToDecimal(r["Price"])
                                }
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки деталей заказа: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return items;
        }

        // Получить только сырьё (Category = 1)
        public static List<Product> GetRawMaterials()
        {
            var list = new List<Product>();
            using (var c = new SQLiteConnection(_connStr))
            {
                c.Open();
                using (var r = new SQLiteCommand("SELECT * FROM Products WHERE Category = 1", c).ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new Product
                        {
                            Id = Convert.ToInt32(r["Id"]),
                            Name = r["Name"]?.ToString() ?? "Без названия",
                            Category = Category.RawMaterial,
                            Price = Convert.ToDecimal(r["Price"]),
                            Stock = Convert.ToInt32(r["Stock"]),
                            Description = r["Description"]?.ToString(),
                            Material = r["Material"]?.ToString(),
                            Dimensions = r["Dimensions"]?.ToString()
                        });
                    }
                }
            }
            return list;
        }

        // Обновить остаток с записью операции
        public static void UpdateStockWithLog(int productId, int change, string operationType, string comment = "")
        {
            using (var c = new SQLiteConnection(_connStr))
            {
                c.Open();
                using (var t = c.BeginTransaction())
                {
                    try
                    {
                        var cmd = new SQLiteCommand("UPDATE Products SET Stock = Stock + @change WHERE Id = @id", c, t);
                        cmd.Parameters.AddWithValue("@change", change);
                        cmd.Parameters.AddWithValue("@id", productId);
                        cmd.ExecuteNonQuery();

                        var cmdLog = new SQLiteCommand(
                            @"INSERT INTO StockOperations (ProductId, OperationType, Quantity, Date, Comment, UserId) 
                              VALUES (@pid, @type, @qty, @date, @comment, @uid)", c, t);
                        cmdLog.Parameters.AddWithValue("@pid", productId);
                        cmdLog.Parameters.AddWithValue("@type", operationType);
                        cmdLog.Parameters.AddWithValue("@qty", Math.Abs(change));
                        cmdLog.Parameters.AddWithValue("@date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmdLog.Parameters.AddWithValue("@comment", comment ?? "");
                        cmdLog.Parameters.AddWithValue("@uid", DataStorage.CurrentUser?.Id ?? 0);
                        cmdLog.ExecuteNonQuery();

                        t.Commit();
                    }
                    catch
                    {
                        t.Rollback();
                        throw;
                    }
                }
            }
        }

        // Получить статистику по складу сырья
        public static WarehouseStats GetWarehouseStats()
        {
            var stats = new WarehouseStats();
            using (var c = new SQLiteConnection(_connStr))
            {
                c.Open();

                stats.TotalItems = Convert.ToInt32(new SQLiteCommand("SELECT COUNT(*) FROM Products WHERE Category = 1", c).ExecuteScalar());
                stats.TotalStock = Convert.ToInt32(new SQLiteCommand("SELECT COALESCE(SUM(Stock), 0) FROM Products WHERE Category = 1", c).ExecuteScalar());
                stats.TotalValue = Convert.ToDecimal(new SQLiteCommand("SELECT COALESCE(SUM(Stock * Price), 0) FROM Products WHERE Category = 1", c).ExecuteScalar());
                stats.LowStockItems = Convert.ToInt32(new SQLiteCommand("SELECT COUNT(*) FROM Products WHERE Category = 1 AND Stock < 100", c).ExecuteScalar());
            }
            return stats;
        }

        public static void AddProduct(Product product)
        {
            using (var c = new SQLiteConnection(_connStr))
            {
                c.Open();
                var cmd = new SQLiteCommand(
                    @"INSERT INTO Products (Name, Category, Price, Stock, Description, Material, Dimensions) 
                      VALUES (@name, @category, @price, @stock, @description, @material, @dimensions)", c);

                cmd.Parameters.AddWithValue("@name", product.Name ?? "");
                cmd.Parameters.AddWithValue("@category", (int)product.Category);
                cmd.Parameters.AddWithValue("@price", (double)product.Price);
                cmd.Parameters.AddWithValue("@stock", product.Stock);
                cmd.Parameters.AddWithValue("@description", product.Description ?? "");
                cmd.Parameters.AddWithValue("@material", product.Material ?? "");
                cmd.Parameters.AddWithValue("@dimensions", product.Dimensions ?? "");

                cmd.ExecuteNonQuery();
            }
        }
        // === МЕТОДЫ ДЛЯ РАБОТЫ С ЗАЯВКАМИ СОТРУДНИКОВ ===

        // Подать заявку на работу
        public static void SubmitEmployeeRequest(string fullName, string login, string password,
            string email, string phone, string position, int desiredRole)
        {
            using (var c = new SQLiteConnection(_connStr))
            {
                c.Open();
                var cmd = new SQLiteCommand(
                    @"INSERT INTO EmployeeRequests (FullName, Login, Password, Email, Phone, Position, DesiredRole, SubmittedDate) 
              VALUES (@fullName, @login, @password, @email, @phone, @position, @desiredRole, @date)", c);

                cmd.Parameters.AddWithValue("@fullName", fullName);
                cmd.Parameters.AddWithValue("@login", login);
                cmd.Parameters.AddWithValue("@password", password);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@phone", phone);
                cmd.Parameters.AddWithValue("@position", position);
                cmd.Parameters.AddWithValue("@desiredRole", desiredRole);
                cmd.Parameters.AddWithValue("@date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                cmd.ExecuteNonQuery();
            }
        }

        // Получить все заявки
        public static List<EmployeeRequest> GetEmployeeRequests()
        {
            var list = new List<EmployeeRequest>();
            using (var c = new SQLiteConnection(_connStr))
            {
                c.Open();
                using (var r = new SQLiteCommand("SELECT * FROM EmployeeRequests ORDER BY SubmittedDate DESC", c).ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new EmployeeRequest
                        {
                            Id = Convert.ToInt32(r["Id"]),
                            FullName = r["FullName"]?.ToString() ?? "",
                            Login = r["Login"]?.ToString() ?? "",
                            Email = r["Email"]?.ToString() ?? "",
                            Phone = r["Phone"]?.ToString() ?? "",
                            Position = r["Position"]?.ToString() ?? "",
                            DesiredRole = (Role)Convert.ToInt32(r["DesiredRole"]),
                            Status = r["Status"]?.ToString() ?? "",
                            SubmittedDate = DateTime.Parse(r["SubmittedDate"].ToString()),
                            AdminComment = r["AdminComment"]?.ToString() ?? ""
                        });
                    }
                }
            }
            return list;
        }

        // Обновить статус заявки
        public static void UpdateRequestStatus(int requestId, string status, string adminComment = "")
        {
            using (var c = new SQLiteConnection(_connStr))
            {
                c.Open();
                var cmd = new SQLiteCommand(
                    "UPDATE EmployeeRequests SET Status = @status, AdminComment = @comment WHERE Id = @id", c);

                cmd.Parameters.AddWithValue("@status", status);
                cmd.Parameters.AddWithValue("@comment", adminComment);
                cmd.Parameters.AddWithValue("@id", requestId);

                cmd.ExecuteNonQuery();
            }
        }

        // Создать пользователя из заявки
        public static void CreateUserFromRequest(int requestId, Role finalRole)
        {
            using (var c = new SQLiteConnection(_connStr))
            {
                c.Open();
                using (var t = c.BeginTransaction())
                {
                    try
                    {
                        var cmd = new SQLiteCommand("SELECT * FROM EmployeeRequests WHERE Id = @id", c, t);
                        cmd.Parameters.AddWithValue("@id", requestId);

                        using (var r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                var createUserCmd = new SQLiteCommand(
                                    @"INSERT INTO Users (Login, Password, Name, Email, Role) 
                              VALUES (@login, @password, @name, @email, @role)", c, t);

                                createUserCmd.Parameters.AddWithValue("@login", r["Login"]);
                                createUserCmd.Parameters.AddWithValue("@password", r["Password"]);
                                createUserCmd.Parameters.AddWithValue("@name", r["FullName"]);
                                createUserCmd.Parameters.AddWithValue("@email", r["Email"]);
                                createUserCmd.Parameters.AddWithValue("@role", (int)finalRole);

                                createUserCmd.ExecuteNonQuery();
                            }
                        }

                        var updateCmd = new SQLiteCommand(
                            "UPDATE EmployeeRequests SET Status = 'Одобрено' WHERE Id = @id", c, t);
                        updateCmd.Parameters.AddWithValue("@id", requestId);
                        updateCmd.ExecuteNonQuery();

                        t.Commit();
                    }
                    catch
                    {
                        t.Rollback();
                        throw;
                    }
                }
            }
        }
        // === МЕТОДЫ ДЛЯ РАБОТЫ С ПРОМОКОДАМИ ===

        // Проверить и получить промокод
        // === МЕТОДЫ ДЛЯ РАБОТЫ С ПРОМОКОДАМИ ===

        public static PromoCode ValidatePromoCode(string code)
        {
            using (var c = new SQLiteConnection(_connStr))
            {
                c.Open();
                var cmd = new SQLiteCommand(
                    "SELECT * FROM PromoCodes WHERE Code = @code AND IsActive = 1 AND ValidUntil >= @date", c);
                cmd.Parameters.AddWithValue("@code", code.ToUpper());
                cmd.Parameters.AddWithValue("@date", DateTime.Now.ToString("yyyy-MM-dd"));

                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        return new PromoCode
                        {
                            Id = Convert.ToInt32(r["Id"]),
                            Code = r["Code"]?.ToString() ?? "",
                            DiscountPercent = Convert.ToDecimal(r["DiscountPercent"]),
                            ValidUntil = DateTime.Parse(r["ValidUntil"].ToString()),
                            IsActive = Convert.ToBoolean(r["IsActive"]),
                            Description = r["Description"]?.ToString() ?? ""
                        };
                    }
                }
            }
            return null;
        }

        public static List<PromoCode> GetActivePromoCodes()
        {
            var list = new List<PromoCode>();
            using (var c = new SQLiteConnection(_connStr))
            {
                c.Open();
                using (var r = new SQLiteCommand(
                    "SELECT * FROM PromoCodes WHERE IsActive = 1 AND ValidUntil >= @date ORDER BY DiscountPercent DESC", c))
                {
                    r.Parameters.AddWithValue("@date", DateTime.Now.ToString("yyyy-MM-dd"));
                    using (var reader = r.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new PromoCode
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Code = reader["Code"]?.ToString() ?? "",
                                DiscountPercent = Convert.ToDecimal(reader["DiscountPercent"]),
                                ValidUntil = DateTime.Parse(reader["ValidUntil"].ToString()),
                                IsActive = Convert.ToBoolean(reader["IsActive"]),
                                Description = reader["Description"]?.ToString() ?? ""
                            });
                        }
                    }
                }
            }
            return list;
        }

        public static void AddPromoCode(string code, decimal discountPercent, DateTime validUntil, string description)
        {
            using (var c = new SQLiteConnection(_connStr))
            {
                c.Open();
                var cmd = new SQLiteCommand(
                    "INSERT INTO PromoCodes (Code, DiscountPercent, ValidUntil, IsActive, Description) VALUES (@code, @percent, @valid, 1, @desc)", c);
                cmd.Parameters.AddWithValue("@code", code.ToUpper());
                cmd.Parameters.AddWithValue("@percent", (double)discountPercent);
                cmd.Parameters.AddWithValue("@valid", validUntil.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@desc", description ?? "");
                cmd.ExecuteNonQuery();
            }
        }

        public static void DeletePromoCode(int promoId)
        {
            using (var c = new SQLiteConnection(_connStr))
            {
                c.Open();
                var cmd = new SQLiteCommand("DELETE FROM PromoCodes WHERE Id = @id", c);
                cmd.Parameters.AddWithValue("@id", promoId);
                cmd.ExecuteNonQuery();
            }
        }
        // Получить ВСЕ промокоды (включая неактивные)
        public static List<PromoCode> GetAllPromoCodes()
        {
            var list = new List<PromoCode>();
            using (var c = new SQLiteConnection(_connStr))
            {
                c.Open();
                using (var r = new SQLiteCommand("SELECT * FROM PromoCodes ORDER BY ValidUntil DESC", c).ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new PromoCode
                        {
                            Id = Convert.ToInt32(r["Id"]),
                            Code = r["Code"]?.ToString() ?? "",
                            DiscountPercent = Convert.ToDecimal(r["DiscountPercent"]),
                            ValidUntil = DateTime.Parse(r["ValidUntil"].ToString()),
                            IsActive = Convert.ToBoolean(r["IsActive"]),
                            Description = r["Description"]?.ToString() ?? ""
                        });
                    }
                }
            }
            return list;
        }

        // Обновить промокод
        public static void UpdatePromoCode(PromoCode promo)
        {
            using (var c = new SQLiteConnection(_connStr))
            {
                c.Open();
                var cmd = new SQLiteCommand(
                    "UPDATE PromoCodes SET Code=@code, DiscountPercent=@percent, ValidUntil=@valid, IsActive=@active, Description=@desc WHERE Id=@id", c);
                cmd.Parameters.AddWithValue("@code", promo.Code.ToUpper());
                cmd.Parameters.AddWithValue("@percent", (double)promo.DiscountPercent);
                cmd.Parameters.AddWithValue("@valid", promo.ValidUntil.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@active", promo.IsActive ? 1 : 0);
                cmd.Parameters.AddWithValue("@desc", promo.Description ?? "");
                cmd.Parameters.AddWithValue("@id", promo.Id);
                cmd.ExecuteNonQuery();
            }
        }
    }
}