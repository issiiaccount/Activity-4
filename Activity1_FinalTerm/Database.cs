using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Security.Cryptography;

namespace Activity1_FinalTerm
{
    public static class Database
    {
        #region Configuration

        public static readonly string DB_PATH = ResolveDatabasePath();

        private static readonly string connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + DB_PATH + ";Persist Security Info=False;";

        #endregion

        #region Public database operations
    
        public static OleDbConnection GetConnection()
        {
            return new OleDbConnection(connectionString);
        }

        public static void InitializeSchema()
        {
            using (OleDbConnection connection = GetConnection())
            {
                connection.Open();

                CreateTableIfMissing(connection, "Users", @"
                    CREATE TABLE Users (
                        UserID AUTOINCREMENT CONSTRAINT PK_Users PRIMARY KEY,
                        Username TEXT(255),
                        PasswordHash TEXT(255),
                        FullName TEXT(255),
                        UserRole TEXT(50),
                        IsActive YESNO
                    )");

                CreateTableIfMissing(connection, "Customers", @"
                    CREATE TABLE Customers (
                        CustomerID AUTOINCREMENT CONSTRAINT PK_Customers PRIMARY KEY,
                        CustomerName TEXT(150) NOT NULL,
                        ContactNumber TEXT(50),
                        EmailAddress TEXT(255),
                        TotalOrderPurchased INTEGER DEFAULT 0,
                        OutstandingBalance CURRENCY DEFAULT 0,
                        CreatedAt DATETIME DEFAULT Now(),
                        IsActive YESNO DEFAULT True
                    )");

                CreateTableIfMissing(connection, "Products", @"
                    CREATE TABLE Products (
                        ProductID AUTOINCREMENT CONSTRAINT PK_Products PRIMARY KEY,
                        ProductName TEXT(150) NOT NULL,
                        Category TEXT(100),
                        CostPrice CURRENCY DEFAULT 0,
                        RetailPrice CURRENCY DEFAULT 0,
                        StockQuantity INTEGER DEFAULT 0,
                        Status TEXT(50) DEFAULT 'In Stock',
                        CreatedAt DATETIME DEFAULT Now(),
                        IsActive YESNO DEFAULT True
                    )");

                EnsureProductCodes(connection);
                EnsureUniqueActiveProducts(connection);
                EnsureSampleProducts(connection);

                CreateTableIfMissing(connection, "Invoices", @"
                    CREATE TABLE Invoices (
                        InvoiceID AUTOINCREMENT CONSTRAINT PK_Invoices PRIMARY KEY,
                        CustomerID LONG,
                        CashierUserID LONG,
                        InvoiceDate DATETIME DEFAULT Now(),
                        Total CURRENCY DEFAULT 0,
                        Status TEXT(50) DEFAULT 'Pending',
                        PaymentMethod TEXT(50),
                        AmountPaid CURRENCY DEFAULT 0,
                        ChangeAmount CURRENCY DEFAULT 0
                    )");

                CreateTableIfMissing(connection, "InvoiceItems", @"
                    CREATE TABLE InvoiceItems (
                        InvoiceItemID AUTOINCREMENT CONSTRAINT PK_InvoiceItems PRIMARY KEY,
                        InvoiceID LONG NOT NULL,
                        ProductID LONG NOT NULL,
                        Quantity INTEGER NOT NULL,
                        UnitPrice CURRENCY DEFAULT 0,
                        Total CURRENCY DEFAULT 0
                    )");

                CreateRelationIfMissing(connection, "FK_Invoices_Customers", @"
                    ALTER TABLE Invoices
                    ADD CONSTRAINT FK_Invoices_Customers
                    FOREIGN KEY (CustomerID) REFERENCES Customers (CustomerID)
                ");

                CreateRelationIfMissing(connection, "FK_Invoices_Users", @"
                    ALTER TABLE Invoices
                    ADD CONSTRAINT FK_Invoices_Users
                    FOREIGN KEY (CashierUserID) REFERENCES Users (UserID)
                ");

                CreateRelationIfMissing(connection, "FK_InvoiceItems_Invoices", @"
                    ALTER TABLE InvoiceItems
                    ADD CONSTRAINT FK_InvoiceItems_Invoices
                    FOREIGN KEY (InvoiceID) REFERENCES Invoices (InvoiceID)
                ");

                CreateRelationIfMissing(connection, "FK_InvoiceItems_Products", @"
                    ALTER TABLE InvoiceItems
                    ADD CONSTRAINT FK_InvoiceItems_Products
                    FOREIGN KEY (ProductID) REFERENCES Products (ProductID)
                ");

                EnsureSamplePurchases(connection);
            }
        }

        #endregion

        #region Schema helpers

        private sealed class SampleProduct
        {
            public int Id { get; set; }
            public decimal Price { get; set; }
            public int Stock { get; set; }
        }

        private static void EnsureUniqueActiveProducts(OleDbConnection connection)
        {
            List<Tuple<string, int>> duplicateProducts = new List<Tuple<string, int>>();
            using (OleDbCommand command = new OleDbCommand(@"
                SELECT ProductName, MIN(ProductID) AS KeepProductID
                FROM Products
                WHERE IsActive = True
                GROUP BY ProductName
                HAVING COUNT(*) > 1", connection))
            using (OleDbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    duplicateProducts.Add(Tuple.Create(
                        reader["ProductName"].ToString(),
                        Convert.ToInt32(reader["KeepProductID"])));
                }
            }

            foreach (Tuple<string, int> duplicate in duplicateProducts)
            {
                using (OleDbCommand command = new OleDbCommand(@"
                    UPDATE Products
                    SET IsActive = False
                    WHERE IsActive = True AND ProductName = ? AND ProductID <> ?", connection))
                {
                    command.Parameters.AddWithValue("@ProductName", duplicate.Item1);
                    command.Parameters.AddWithValue("@KeepProductID", duplicate.Item2);
                    command.ExecuteNonQuery();
                }
            }
        }

        private static void EnsureSampleProducts(OleDbConnection connection)
        {
            const int targetProductCount = 250;
            int activeProductCount;
            using (OleDbCommand count = new OleDbCommand("SELECT COUNT(*) FROM Products WHERE IsActive = True", connection))
            {
                activeProductCount = Convert.ToInt32(count.ExecuteScalar());
            }

            if (activeProductCount >= targetProductCount) return;

            string[] categories =
            {
                "Beverages", "Snacks", "Canned Goods", "Dairy", "Bakery",
                "Personal Care", "Household", "Stationery", "Condiments", "Frozen Foods"
            };
            string[] brands =
            {
                "Apex", "BrightDay", "Cedar", "DailyChoice", "EverFresh",
                "GoldenHarvest", "HomePlus", "NorthStar", "PrimeSelect", "ValueMart"
            };
            string[] items =
            {
                "Classic", "Premium", "Family Pack", "Value Pack", "Original"
            };

            int productNumber = 1;
            while (activeProductCount < targetProductCount)
            {
                string category = categories[(productNumber - 1) % categories.Length];
                string productName = string.Format(
                    "{0} {1} {2} {3:000}",
                    brands[(productNumber - 1) % brands.Length],
                    items[(productNumber - 1) % items.Length],
                    category,
                    productNumber);

                using (OleDbCommand exists = new OleDbCommand("SELECT COUNT(*) FROM Products WHERE ProductName = ?", connection))
                {
                    exists.Parameters.AddWithValue("@ProductName", productName);
                    if (Convert.ToInt32(exists.ExecuteScalar()) > 0)
                    {
                        productNumber++;
                        continue;
                    }
                }

                decimal costPrice = 20m + ((productNumber * 17) % 481);
                decimal retailPrice = Math.Round(costPrice * 1.25m, 2);
                int stockQuantity = (productNumber * 13) % 61;
                int sequence = GetNextProductSequence(connection);
                string productCode = CreateProductCode(connection, sequence);

                using (OleDbCommand insert = new OleDbCommand(@"
                    INSERT INTO Products
                        (ProductCode, ProductName, Category, CostPrice, RetailPrice, StockQuantity, Status, IsActive)
                    VALUES (?, ?, ?, ?, ?, ?, ?, True)", connection))
                {
                    insert.Parameters.AddWithValue("@ProductCode", productCode);
                    insert.Parameters.AddWithValue("@ProductName", productName);
                    insert.Parameters.AddWithValue("@Category", category);
                    insert.Parameters.AddWithValue("@CostPrice", costPrice);
                    insert.Parameters.AddWithValue("@RetailPrice", retailPrice);
                    insert.Parameters.AddWithValue("@StockQuantity", stockQuantity);
                    insert.Parameters.AddWithValue("@Status", GetProductStatus(stockQuantity));
                    insert.ExecuteNonQuery();
                }

                activeProductCount++;
                productNumber++;
            }
        }

        private static string GetProductStatus(int stockQuantity)
        {
            if (stockQuantity <= 0) return "Out of Stock";
            return stockQuantity <= 5 ? "Low Stock" : "In Stock";
        }

        private static void EnsureSamplePurchases(OleDbConnection connection)
        {
            List<int> customerIds = new List<int>();
            using (OleDbCommand command = new OleDbCommand(@"
                SELECT TOP 3 c.CustomerID
                FROM Customers AS c
                WHERE c.IsActive = True
                ORDER BY c.CustomerID", connection))
            using (OleDbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read()) customerIds.Add(Convert.ToInt32(reader["CustomerID"]));
            }

            if (customerIds.Count == 0) return;

            List<SampleProduct> products = new List<SampleProduct>();
            using (OleDbCommand command = new OleDbCommand(@"
                SELECT ProductID, RetailPrice, StockQuantity
                FROM Products
                WHERE IsActive = True AND StockQuantity > 0", connection))
            using (OleDbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    products.Add(new SampleProduct
                    {
                        Id = Convert.ToInt32(reader["ProductID"]),
                        Price = Convert.ToDecimal(reader["RetailPrice"]),
                        Stock = Convert.ToInt32(reader["StockQuantity"])
                    });
                }
            }

            if (products.Count == 0) return;

            Random random = new Random();
            using (OleDbTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    for (int monthOffset = 0; monthOffset < 12; monthOffset++)
                    {
                        DateTime monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-monthOffset);
                        DateTime nextMonthStart = monthStart.AddMonths(1);
                        using (OleDbCommand existingMonth = new OleDbCommand(@"
                            SELECT COUNT(*)
                            FROM Invoices
                            WHERE Status = 'Paid' AND InvoiceDate >= ? AND InvoiceDate < ?", connection, transaction))
                        {
                            existingMonth.Parameters.AddWithValue("@MonthStart", monthStart);
                            existingMonth.Parameters.AddWithValue("@NextMonthStart", nextMonthStart);
                            if (Convert.ToInt32(existingMonth.ExecuteScalar()) > 0) continue;
                        }

                        int customerId = customerIds[monthOffset % customerIds.Count];
                        List<SampleProduct> availableProducts = products.FindAll(p => p.Stock > 0);
                        if (availableProducts.Count == 0) throw new InvalidOperationException("There is not enough product stock to create sample purchases.");

                        SampleProduct product = availableProducts[random.Next(availableProducts.Count)];
                        int quantity = random.Next(1, Math.Min(3, product.Stock) + 1);
                        decimal subtotal = product.Price * quantity;
                        decimal total = subtotal * 1.10m;
                        int daysSinceMonthStart = (DateTime.Today - monthStart).Days;
                        DateTime invoiceDate = monthStart.AddDays(Math.Min(random.Next(0, 10), daysSinceMonthStart));
                        int invoiceId;

                        using (OleDbCommand invoice = new OleDbCommand(@"
                            INSERT INTO Invoices
                                (CustomerID, InvoiceDate, Total, Status, PaymentMethod, AmountPaid, ChangeAmount)
                            VALUES (?, ?, ?, 'Paid', 'Cash', ?, 0)", connection, transaction))
                        {
                            invoice.Parameters.AddWithValue("@CustomerID", customerId);
                            invoice.Parameters.AddWithValue("@InvoiceDate", invoiceDate);
                            invoice.Parameters.AddWithValue("@Total", total);
                            invoice.Parameters.AddWithValue("@AmountPaid", total);
                            invoice.ExecuteNonQuery();
                        }

                        using (OleDbCommand identity = new OleDbCommand("SELECT @@IDENTITY", connection, transaction))
                        {
                            invoiceId = Convert.ToInt32(identity.ExecuteScalar());
                        }

                        using (OleDbCommand item = new OleDbCommand(@"
                            INSERT INTO InvoiceItems (InvoiceID, ProductID, Quantity, UnitPrice, Total)
                            VALUES (?, ?, ?, ?, ?)", connection, transaction))
                        {
                            item.Parameters.AddWithValue("@InvoiceID", invoiceId);
                            item.Parameters.AddWithValue("@ProductID", product.Id);
                            item.Parameters.AddWithValue("@Quantity", quantity);
                            item.Parameters.AddWithValue("@UnitPrice", product.Price);
                            item.Parameters.AddWithValue("@Total", subtotal);
                            item.ExecuteNonQuery();
                        }

                        using (OleDbCommand stock = new OleDbCommand(@"
                            UPDATE Products
                            SET StockQuantity = StockQuantity - ?,
                                Status = IIF(StockQuantity - ? <= 0, 'Out of Stock',
                                    IIF(StockQuantity - ? <= 5, 'Low Stock', 'In Stock'))
                            WHERE ProductID = ? AND StockQuantity >= ?", connection, transaction))
                        {
                            stock.Parameters.AddWithValue("@Quantity1", quantity);
                            stock.Parameters.AddWithValue("@Quantity2", quantity);
                            stock.Parameters.AddWithValue("@Quantity3", quantity);
                            stock.Parameters.AddWithValue("@ProductID", product.Id);
                            stock.Parameters.AddWithValue("@Quantity4", quantity);
                            stock.ExecuteNonQuery();
                        }

                        product.Stock -= quantity;
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public static string CreateProductCode(OleDbConnection connection, int sequence)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            using (RandomNumberGenerator generator = RandomNumberGenerator.Create())
            {
                byte[] bytes = new byte[2];
                string code;
                do
                {
                    generator.GetBytes(bytes);
                    code = "PRD" + alphabet[bytes[0] % alphabet.Length] + alphabet[bytes[1] % alphabet.Length] + sequence;
                }
                while (ProductCodeExists(connection, code));

                return code;
            }
        }

        public static int GetNextProductSequence(OleDbConnection connection)
        {
            using (OleDbCommand command = new OleDbCommand("SELECT MAX(ProductID) FROM Products", connection))
            {
                object value = command.ExecuteScalar();
                return value == null || value == DBNull.Value ? 1 : Convert.ToInt32(value) + 1;
            }
        }

        private static void EnsureProductCodes(OleDbConnection connection)
        {
            if (!ColumnExists(connection, "Products", "ProductCode"))
            {
                using (OleDbCommand command = new OleDbCommand("ALTER TABLE Products ADD COLUMN ProductCode TEXT(30)", connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            using (OleDbCommand select = new OleDbCommand("SELECT ProductID FROM Products WHERE ProductCode IS NULL OR ProductCode = '' ORDER BY ProductID", connection))
            {
                List<int> productIds = new List<int>();
                using (OleDbDataReader reader = select.ExecuteReader())
                {
                    while (reader.Read()) productIds.Add(Convert.ToInt32(reader["ProductID"]));
                }

                foreach (int productId in productIds)
                {
                    using (OleDbCommand update = new OleDbCommand("UPDATE Products SET ProductCode = ? WHERE ProductID = ?", connection))
                    {
                        update.Parameters.AddWithValue("@ProductCode", CreateProductCode(connection, productId));
                        update.Parameters.AddWithValue("@ProductID", productId);
                        update.ExecuteNonQuery();
                    }
                }
            }
        }

        private static bool ProductCodeExists(OleDbConnection connection, string productCode)
        {
            using (OleDbCommand command = new OleDbCommand("SELECT COUNT(*) FROM Products WHERE ProductCode = ?", connection))
            {
                command.Parameters.AddWithValue("@ProductCode", productCode);
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        private static bool ColumnExists(OleDbConnection connection, string tableName, string columnName)
        {
            DataTable columns = connection.GetOleDbSchemaTable(
                OleDbSchemaGuid.Columns,
                new object[] { null, null, tableName, columnName });
            return columns != null && columns.Rows.Count > 0;
        }

        private static void CreateTableIfMissing(OleDbConnection connection, string tableName, string createSql)
        {
            if (!TableExists(connection, tableName))
            {
                using (OleDbCommand command = new OleDbCommand(createSql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        private static void CreateRelationIfMissing(OleDbConnection connection, string relationName, string createSql)
        {
            if (RelationExists(connection, relationName))
            {
                return;
            }

            using (OleDbCommand command = new OleDbCommand(createSql, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        private static bool RelationExists(OleDbConnection connection, string relationName)
        {
            DataTable relations = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Foreign_Keys, null);
            return relations != null && relations.Rows.Cast<DataRow>().Any(row =>
                row.Table.Columns.Contains("FK_NAME") &&
                string.Equals(row["FK_NAME"].ToString(), relationName, StringComparison.OrdinalIgnoreCase));
        }

        private static string ResolveDatabasePath()
        {
            string applicationDirectory = Path.GetDirectoryName(typeof(Database).Assembly.Location);
            if (string.IsNullOrEmpty(applicationDirectory))
            {
                applicationDirectory = AppDomain.CurrentDomain.BaseDirectory;
            }

            string deployedPath = Path.Combine(applicationDirectory, "InventorySys.accdb");
            if (File.Exists(deployedPath))
            {
                return deployedPath;
            }

            return Path.GetFullPath(Path.Combine(applicationDirectory, @"..\..\InventorySys.accdb"));
        }

        private static bool TableExists(OleDbConnection connection, string tableName)
        {
            DataTable tables = connection.GetSchema("Tables");
            return tables.Rows.Cast<DataRow>().Any(row =>
                string.Equals(row["TABLE_NAME"].ToString(), tableName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(row["TABLE_TYPE"].ToString(), "TABLE", StringComparison.OrdinalIgnoreCase));
        }

        #endregion
    }
}
