using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Activity1_FinalTerm
{
    public partial class ucProducts : UserControl
    {
        private readonly List<object[]> filteredProducts = new List<object[]>();
        private readonly Dictionary<string, int> productIdsByCode = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        #region Initialization and layout

        public ucProducts()
        {
            InitializeComponent();
            MinimumSize = Size.Empty;
            roundedTextBox1.BackColor = Color.White;
            roundedTextBox1.BorderActiveColor = Color.FromArgb(32, 150, 110);
            roundedTextBox1.BorderColor = Color.FromArgb(210, 220, 220);
            roundedTextBox1.BorderInactiveColor = Color.FromArgb(210, 220, 220);
            roundedTextBox1.BorderRadius = 10;
            roundedTextBox1.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point, 0);
            roundedTextBox1.Height = 24;
            roundedTextBox1.Padding = new Padding(10, 4, 10, 4);
            roundedTextBox1.PlaceholderColor = Color.Gray;
            button1.AutoSize = false;
            button1.Height = 24;
            button2.Height = 30;
            button3.Height = 30;
            ApplyProductsButtonStyle(button1);
            ApplyProductsButtonStyle(button2);
            ApplyProductsButtonStyle(button3);
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.CellDoubleClick += dataGridView1_CellDoubleClick;
            dataGridView1.ColumnHeaderMouseClick += dataGridView1_ColumnHeaderMouseClick;
            dataGridView1.SortCompare += dataGridView1_SortCompare;
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.Programmatic;
            }

            ProductID.MinimumWidth = 70;
            ProductName.MinimumWidth = 120;
            Category.MinimumWidth = 90;
            CostPrice.MinimumWidth = 85;
            RetailPrice.MinimumWidth = 85;
            StockQuantity.MinimumWidth = 90;
            Status.MinimumWidth = 90;
            ProductID.FillWeight = 55;
            ProductName.FillWeight = 170;
            Category.FillWeight = 110;
            CostPrice.FillWeight = 100;
            RetailPrice.FillWeight = 100;
            StockQuantity.FillWeight = 110;
            Status.FillWeight = 110;
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
            dataGridView1.RowTemplate.Height = 30;
            dataGridView1.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
            dataGridView1.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            button1.Click += button1_Click;
            button2.Click += button2_Click;
            button3.Click += button3_Click;
            flatComboBox1.Items.Clear();
            flatComboBox1.Items.Add("All Categories");
            flatComboBox2.Items.Clear();
            flatComboBox2.Items.Add("All Status");
            flatComboBox2.Items.Add("In Stock");
            flatComboBox2.Items.Add("Low Stock");
            flatComboBox2.Items.Add("Out of Stock");
            flatComboBox1.SelectedIndex = 0;
            flatComboBox2.SelectedIndex = 0;
            roundedTextBox1.TextChanged += (sender, args) => LoadProducts();
            flatComboBox1.SelectedIndexChanged += (sender, args) => LoadProducts();
            flatComboBox2.SelectedIndexChanged += (sender, args) => LoadProducts();
            Resize += (sender, args) => LayoutProductsPage();
            LoadCategories();
            LoadProducts();
            LayoutProductsPage();
        }

        private void ApplyProductsButtonStyle(Button button)
        {
            button.BackColor = Color.FromArgb(40, 161, 116);
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(49, 177, 130);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(31, 130, 94);
            button.FlatStyle = FlatStyle.Flat;
            button.Font = new Font("Segoe UI", 8.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            button.ForeColor = Color.White;
            button.Cursor = Cursors.Hand;
            button.Padding = new Padding(0);
            button.TextAlign = ContentAlignment.MiddleCenter;
            button.UseVisualStyleBackColor = false;
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) button2_Click(sender, EventArgs.Empty);
        }

        private void dataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex < 0) return;

            SortOrder nextOrder = dataGridView1.Columns[e.ColumnIndex].HeaderCell.SortGlyphDirection == SortOrder.Ascending
                ? SortOrder.Descending
                : SortOrder.Ascending;

            dataGridView1.Sort(dataGridView1.Columns[e.ColumnIndex], nextOrder == SortOrder.Ascending
                ? ListSortDirection.Ascending
                : ListSortDirection.Descending);
            dataGridView1.Columns[e.ColumnIndex].HeaderCell.SortGlyphDirection = nextOrder;
        }

        private void dataGridView1_SortCompare(object sender, DataGridViewSortCompareEventArgs e)
        {
            decimal leftNumber;
            decimal rightNumber;
            if (TryParseMoney(Convert.ToString(e.CellValue1), out leftNumber)
                && TryParseMoney(Convert.ToString(e.CellValue2), out rightNumber))
            {
                e.SortResult = leftNumber.CompareTo(rightNumber);
            }
            else
            {
                e.SortResult = string.Compare(Convert.ToString(e.CellValue1), Convert.ToString(e.CellValue2), StringComparison.CurrentCultureIgnoreCase);
            }
            e.Handled = true;
        }

        private void LayoutProductsPage()
        {
            const int margin = 21;
            const int gap = 12;
            const int desktopTableTop = 132;
            int width = Math.Max(1, ClientSize.Width);
            int filterTop = 81;
            int tableTop = desktopTableTop;

            roundedTextBox1.Left = margin;
            roundedTextBox1.Top = filterTop;
            button1.AutoSize = false;
            button1.Height = roundedTextBox1.Height;
            roundedTextBox1.Width = Math.Max(180, width - (margin * 2));

            if (width >= 760)
            {
                button1.Left = width - margin - button1.Width;
                flatComboBox2.Left = button1.Left - gap - flatComboBox2.Width;
                flatComboBox1.Left = flatComboBox2.Left - gap - flatComboBox1.Width;
                roundedTextBox1.Width = Math.Min(535, Math.Max(180, flatComboBox1.Left - gap - margin));
                tableTop = desktopTableTop;
            }
            else
            {
                filterTop = roundedTextBox1.Bottom + gap;
                flatComboBox1.Left = margin;
                flatComboBox2.Left = flatComboBox1.Right + gap;
                button1.Left = width - margin - button1.Width;
                flatComboBox1.Top = filterTop;
                flatComboBox2.Top = filterTop;
                button1.Top = filterTop;
                tableTop = flatComboBox1.Bottom + gap;
            }

            if (width >= 760)
            {
                flatComboBox1.Top = filterTop;
                flatComboBox2.Top = filterTop;
                button1.Top = filterTop;
            }

            int actionTop = Math.Max(tableTop + 120, ClientSize.Height - margin - button2.Height);
            dataGridView1.Left = margin;
            dataGridView1.Top = tableTop;
            dataGridView1.Width = Math.Max(1, width - (margin * 2));
            dataGridView1.Height = Math.Max(120, actionTop - tableTop - gap);

            button2.Left = margin;
            button2.Top = actionTop;
            button2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            button2.Visible = true;
            button3.Left = button2.Right + gap;
            button3.Top = button2.Top;
            button3.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            button3.Visible = true;
            label1.Left = Math.Max(margin, width - margin - label1.Width);
            label1.Top = button2.Top + 2;
            label1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            label1.Visible = true;
            button2.BringToFront();
            button3.BringToFront();
            label1.BringToFront();
        }

        #endregion

        #region Product commands and data operations

        private void button1_Click(object sender, EventArgs e)
        {
            Dictionary<string, string> values;
            if (!ucCustomers.ShowEditor("Add New Product", new[] { "Product Name", "Category", "Cost Price", "Retail Price", "Stock Quantity" }, null, out values)) return;
            decimal costPrice;
            decimal retailPrice;
            int stockQuantity;
            if (string.IsNullOrWhiteSpace(values["Product Name"]) || !TryParseMoney(values["Cost Price"], out costPrice) || !TryParseMoney(values["Retail Price"], out retailPrice) || !int.TryParse(values["Stock Quantity"], out stockQuantity) || costPrice < 0 || retailPrice < 0 || stockQuantity < 0)
            {
                MessageBox.Show("Enter a product name and valid non-negative numeric values.", "Products", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (OleDbConnection connection = Database.GetConnection())
                {
                    connection.Open();
                    string productName = values["Product Name"].Trim();
                    string category = values["Category"].Trim();
                    if (ProductExists(connection, productName, category, null))
                    {
                        MessageBox.Show("A product with the same name already exists in this category.", "Products", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    int sequence = Database.GetNextProductSequence(connection);
                    string productCode = Database.CreateProductCode(connection, sequence);
                    using (OleDbCommand command = new OleDbCommand(@"INSERT INTO Products (ProductCode, ProductName, Category, CostPrice, RetailPrice, StockQuantity, Status, IsActive) VALUES (?, ?, ?, ?, ?, ?, ?, True)", connection))
                    {
                        command.Parameters.AddWithValue("@ProductCode", productCode);
                        command.Parameters.AddWithValue("@Name", productName);
                        command.Parameters.AddWithValue("@Category", category);
                        command.Parameters.AddWithValue("@Cost", costPrice);
                        command.Parameters.AddWithValue("@Retail", retailPrice);
                        command.Parameters.AddWithValue("@Stock", stockQuantity);
                        command.Parameters.AddWithValue("@Status", GetStatus(stockQuantity));
                        command.ExecuteNonQuery();
                    }
                }
                ReloadCategories();
                LoadProducts();
            }
            catch (Exception ex) { MessageBox.Show("Unable to add product: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = GetSelectedRow();
            if (row == null) return;
            Dictionary<string, string> values = new Dictionary<string, string>
            {
                ["Product Name"] = CellText(row, 1),
                ["Category"] = CellText(row, 2),
                ["Cost Price"] = CellText(row, 3),
                ["Retail Price"] = CellText(row, 4),
                ["Stock Quantity"] = CellText(row, 5)
            };
            Dictionary<string, string> updated;
            if (!ucCustomers.ShowEditor("Edit Product", new[] { "Product Name", "Category", "Cost Price", "Retail Price", "Stock Quantity" }, values, out updated)) return;
            decimal costPrice;
            decimal retailPrice;
            int stockQuantity;
            if (!TryParseMoney(updated["Cost Price"], out costPrice) || !TryParseMoney(updated["Retail Price"], out retailPrice) || !int.TryParse(updated["Stock Quantity"], out stockQuantity) || string.IsNullOrWhiteSpace(updated["Product Name"]) || costPrice < 0 || retailPrice < 0 || stockQuantity < 0)
            {
                MessageBox.Show("Enter a product name and valid non-negative numeric values.", "Products", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (OleDbConnection connection = Database.GetConnection())
                {
                    connection.Open();
                    int productId = Convert.ToInt32(row.Tag);
                    if (ProductExists(connection, updated["Product Name"], updated["Category"], productId))
                    {
                        MessageBox.Show("A product with the same name already exists in this category.", "Products", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    using (OleDbCommand command = new OleDbCommand(@"UPDATE Products SET ProductName = ?, Category = ?, CostPrice = ?, RetailPrice = ?, StockQuantity = ?, Status = ? WHERE ProductID = ?", connection))
                    {
                        command.Parameters.AddWithValue("@Name", updated["Product Name"]);
                        command.Parameters.AddWithValue("@Category", updated["Category"]);
                        command.Parameters.AddWithValue("@Cost", costPrice);
                        command.Parameters.AddWithValue("@Retail", retailPrice);
                        command.Parameters.AddWithValue("@Stock", stockQuantity);
                        command.Parameters.AddWithValue("@Status", GetStatus(stockQuantity));
                        command.Parameters.AddWithValue("@ID", productId);
                        command.ExecuteNonQuery();
                    }
                }
                ReloadCategories();
                LoadProducts();
            }
            catch (Exception ex) { MessageBox.Show("Unable to update product: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = GetSelectedRow();
            if (row == null) return;
            if (MessageBox.Show("Delete the selected product?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                using (OleDbConnection connection = Database.GetConnection())
                using (OleDbCommand command = new OleDbCommand("UPDATE Products SET IsActive = False, Status = 'Out of Stock' WHERE ProductID = ?", connection))
                {
                    connection.Open();
                    command.Parameters.AddWithValue("@ID", Convert.ToInt32(row.Tag));
                    command.ExecuteNonQuery();
                }
                LoadProducts();
            }
            catch (Exception ex) { MessageBox.Show("Unable to delete product: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private string GetStatus(int stockQuantity)
        {
            return stockQuantity <= 0 ? "Out of Stock" : stockQuantity <= 5 ? "Low Stock" : "In Stock";
        }

        private DataGridViewRow GetSelectedRow()
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select a product first.", "Products", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }
            return dataGridView1.SelectedRows[0];
        }

        private static string CellText(DataGridViewRow row, int index)
        {
            return row.Cells[index].Value == null || row.Cells[index].Value == DBNull.Value
                ? string.Empty
                : row.Cells[index].Value.ToString().Replace("$", string.Empty).Replace("₱", string.Empty).Trim();
        }

        private static bool TryParseMoney(string value, out decimal amount)
        {
            return decimal.TryParse(value, NumberStyles.Currency, CultureInfo.CurrentCulture, out amount)
                || decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out amount);
        }

        private static bool ProductExists(OleDbConnection connection, string productName, string category, int? excludedProductId)
        {
            string sql = "SELECT COUNT(*) FROM Products WHERE IsActive = True AND ProductName = ?";
            if (excludedProductId.HasValue) sql += " AND ProductID <> ?";

            using (OleDbCommand command = new OleDbCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@Name", productName.Trim());
                if (excludedProductId.HasValue) command.Parameters.AddWithValue("@ID", excludedProductId.Value);
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        private void ReloadCategories()
        {
            flatComboBox1.Items.Clear();
            flatComboBox1.Items.Add("All Categories");
            LoadCategories();
            flatComboBox1.SelectedIndex = 0;
        }

        private void LoadCategories()
        {
            try
            {
                using (OleDbConnection connection = Database.GetConnection())
                using (OleDbCommand command = new OleDbCommand("SELECT DISTINCT Category FROM Products WHERE Category IS NOT NULL AND Category <> '' ORDER BY Category", connection))
                {
                    connection.Open();
                    using (OleDbDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            flatComboBox1.Items.Add(reader["Category"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load product categories: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadProducts()
        {
            try
            {
                filteredProducts.Clear();
                productIdsByCode.Clear();
                string search = roundedTextBox1.Text.Trim();
                string category = flatComboBox1.SelectedItem == null ? "All Categories" : flatComboBox1.SelectedItem.ToString();
                string status = flatComboBox2.SelectedItem == null ? "All Status" : flatComboBox2.SelectedItem.ToString();

                using (OleDbConnection connection = Database.GetConnection())
                using (OleDbCommand command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = @"
                        SELECT ProductID, ProductCode, ProductName, Category, CostPrice, RetailPrice,
                               StockQuantity, Status
                        FROM Products
                        WHERE IsActive = True
                          AND (ProductName LIKE ? OR Category LIKE ? OR ProductCode LIKE ? OR ProductID = ?)
                          AND (? = 'All Categories' OR Category = ?)
                          AND (? = 'All Status' OR Status = ?)
                        ORDER BY ProductName";

                    string pattern = search + "%";
                    string broadPattern = "%" + search + "%";
                    int productIdSearch;
                    int.TryParse(search, out productIdSearch);
                    command.Parameters.AddWithValue("@Name", pattern);
                    command.Parameters.AddWithValue("@CategorySearch", broadPattern);
                    command.Parameters.AddWithValue("@ProductCode", broadPattern);
                    command.Parameters.AddWithValue("@ProductID", productIdSearch);
                    command.Parameters.AddWithValue("@CategoryAll", category);
                    command.Parameters.AddWithValue("@Category", category);
                    command.Parameters.AddWithValue("@StatusAll", status);
                    command.Parameters.AddWithValue("@Status", status);

                    using (OleDbDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string productCode = reader["ProductCode"].ToString();
                            productIdsByCode[productCode] = Convert.ToInt32(reader["ProductID"]);
                            filteredProducts.Add(new object[]
                            {
                                productCode,
                                reader["ProductName"],
                                reader["Category"],
                                Convert.ToDecimal(reader["CostPrice"]).ToString("C2"),
                                Convert.ToDecimal(reader["RetailPrice"]).ToString("C2"),
                                reader["StockQuantity"],
                                reader["Status"]
                            });
                        }
                    }
                }

                PopulateProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load products: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PopulateProducts()
        {
            dataGridView1.Rows.Clear();

            for (int i = 0; i < filteredProducts.Count; i++)
            {
                int rowIndex = dataGridView1.Rows.Add(filteredProducts[i]);
                dataGridView1.Rows[rowIndex].Tag = productIdsByCode[filteredProducts[i][0].ToString()];
            }

            label1.Text = filteredProducts.Count == 0
                ? "Showing 0 Products"
                : "Showing " + filteredProducts.Count + " Products";
        }

        #endregion
    }
}
