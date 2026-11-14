using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Activity1_FinalTerm
{
    public partial class ucPOS : UserControl
    {
        private readonly int cashierUserId;
        private readonly Dictionary<int, int> productStocks = new Dictionary<int, int>();
        private readonly ListBox productSuggestions = new ListBox();
        private bool selectingProductSuggestion;

        #region Initialization and layout

        public ucPOS() : this(0)
        {
        }

        public ucPOS(int userId)
        {
            InitializeComponent();
            MinimumSize = Size.Empty;
            AutoScroll = false;
            roundedTextBox1.BackColor = Color.White;
            roundedTextBox1.BorderActiveColor = Color.FromArgb(32, 150, 110);
            roundedTextBox1.BorderColor = Color.FromArgb(210, 220, 220);
            roundedTextBox1.BorderInactiveColor = Color.FromArgb(210, 220, 220);
            roundedTextBox1.BorderRadius = 10;
            roundedTextBox1.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point, 0);
            roundedTextBox1.Height = 24;
            roundedTextBox1.Padding = new Padding(10, 4, 10, 4);
            roundedTextBox1.PlaceholderColor = Color.Gray;
            flatComboBox1.Height = 24;
            button1.Width = 120;
            button1.AutoSize = false;
            button1.Height = 24;
            button2.Height = 30;
            ApplyPosButtonStyle(button1);
            ApplyPosButtonStyle(button2);
            panel1.AutoScroll = true;
            panel1.AutoScrollMinSize = new Size(0, 450);
            cashierUserId = userId;
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.RowTemplate.Height = 30;
            dataGridView1.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
            dataGridView1.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            Number.MinimumWidth = 45;
            ProductName.MinimumWidth = 120;
            UnitPrice.MinimumWidth = 85;
            Quantity.MinimumWidth = 75;
            Total.MinimumWidth = 85;
            Action.MinimumWidth = 70;
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                column.ReadOnly = true;
            }

            Quantity.ReadOnly = false;
            button1.Click += button1_Click;
            button2.Click += button2_Click;
            dataGridView1.CellValidating += dataGridView1_CellValidating;
            dataGridView1.CellEndEdit += dataGridView1_CellEndEdit;
            roundedTextBox1.TextChanged += roundedTextBox1_TextChanged;
            roundedTextBox1.KeyDown += roundedTextBox1_KeyDown;
            productSuggestions.Visible = false;
            productSuggestions.IntegralHeight = false;
            productSuggestions.Font = new Font("Segoe UI", 9F);
            productSuggestions.Height = 120;
            productSuggestions.BringToFront();
            productSuggestions.Click += productSuggestions_Click;
            Controls.Add(productSuggestions);
            checkBox1.CheckedChanged += PaymentMethodChanged;
            checkBox2.CheckedChanged += PaymentMethodChanged;
            checkBox3.CheckedChanged += PaymentMethodChanged;
            LoadCustomers();
            UpdateSummary();
            Resize += (sender, args) => LayoutPosPage();
            panel1.Resize += (sender, args) => LayoutPosSummary();
            LayoutPosPage();
        }

        private void ApplyPosButtonStyle(Button button)
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

        private void roundedTextBox1_TextChanged(object sender, EventArgs e)
        {
            button1.Enabled = !string.IsNullOrWhiteSpace(roundedTextBox1.Text);
            if (!selectingProductSuggestion) LoadProductSuggestions();
        }

        private void roundedTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter || !button1.Enabled) return;

            e.SuppressKeyPress = true;
            button1_Click(sender, EventArgs.Empty);
        }

        private void LoadProductSuggestions()
        {
            productSuggestions.Items.Clear();
            string search = roundedTextBox1.Text.Trim();
            if (string.IsNullOrWhiteSpace(search))
            {
                productSuggestions.Visible = false;
                return;
            }

            try
            {
                using (OleDbConnection connection = Database.GetConnection())
                using (OleDbCommand command = new OleDbCommand(@"
                    SELECT TOP 8 ProductID, ProductName, RetailPrice, StockQuantity
                    FROM Products
                    WHERE IsActive = True AND StockQuantity > 0
                      AND (ProductName LIKE ? OR Category LIKE ? OR ProductCode LIKE ?)
                    ORDER BY ProductName", connection))
                {
                    string prefix = search + "%";
                    string contains = "%" + search + "%";
                    command.Parameters.AddWithValue("@Name", prefix);
                    command.Parameters.AddWithValue("@Category", contains);
                    command.Parameters.AddWithValue("@ProductCode", contains);
                    connection.Open();
                    using (OleDbDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            productSuggestions.Items.Add(new ProductSuggestion
                            {
                                Id = Convert.ToInt32(reader["ProductID"]),
                                Name = reader["ProductName"].ToString(),
                                Price = Convert.ToDecimal(reader["RetailPrice"]),
                                Stock = Convert.ToInt32(reader["StockQuantity"])
                            });
                        }
                    }
                }
                productSuggestions.Visible = productSuggestions.Items.Count > 0;
                if (productSuggestions.Visible) productSuggestions.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                productSuggestions.Visible = false;
                System.Diagnostics.Debug.WriteLine("Unable to load POS product suggestions: " + ex.Message);
            }
        }

        private void productSuggestions_Click(object sender, EventArgs e)
        {
            ProductSuggestion suggestion = productSuggestions.SelectedItem as ProductSuggestion;
            if (suggestion == null) return;
            selectingProductSuggestion = true;
            roundedTextBox1.Text = suggestion.Name;
            selectingProductSuggestion = false;
            productSuggestions.Visible = false;
            button1.Focus();
        }

        private sealed class ProductSuggestion
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
            public int Stock { get; set; }

            public override string ToString()
            {
                return Name + "  -  " + Price.ToString("C2") + " (" + Stock + " in stock)";
            }
        }

        private void LayoutPosPage()
        {
            const int margin = 21;
            const int gap = 14;
            const int desktopTableTop = 132;
            int width = Math.Max(1, ClientSize.Width);
            int height = Math.Max(1, ClientSize.Height);
            int tableTop = desktopTableTop;

            roundedTextBox1.Left = margin;
            roundedTextBox1.Top = 81;
            button1.AutoSize = false;
            button1.Height = roundedTextBox1.Height;

            if (width >= 760)
            {
                int summaryWidth = Math.Min(280, Math.Max(240, width / 3));
                panel1.Left = width - margin - summaryWidth;
                panel1.Top = tableTop;
                panel1.Width = summaryWidth;
                panel1.Height = Math.Max(240, height - tableTop - margin);

                button1.Left = width - margin - button1.Width;
                flatComboBox1.Left = button1.Left - gap - flatComboBox1.Width;
                roundedTextBox1.Width = Math.Min(650, Math.Max(180, flatComboBox1.Left - gap - margin));
                flatComboBox1.Top = 81;
                button1.Top = 81;

                dataGridView1.Left = margin;
                dataGridView1.Top = tableTop;
                dataGridView1.Width = Math.Max(180, panel1.Left - gap - margin);
                dataGridView1.Height = Math.Max(180, height - tableTop - margin);
            }
            else
            {
                roundedTextBox1.Width = Math.Max(180, width - (margin * 2));
                flatComboBox1.Left = margin;
                flatComboBox1.Top = roundedTextBox1.Bottom + gap;
                button1.Left = flatComboBox1.Right + gap;
                button1.Top = flatComboBox1.Top;
                tableTop = flatComboBox1.Bottom + gap;

                dataGridView1.Left = margin;
                dataGridView1.Top = tableTop;
                dataGridView1.Width = Math.Max(180, width - (margin * 2));
                dataGridView1.Height = 260;
                panel1.Left = margin;
                panel1.Top = dataGridView1.Bottom + gap;
                panel1.Width = Math.Max(240, width - (margin * 2));
                panel1.Height = 432;
            }

            productSuggestions.Left = roundedTextBox1.Left;
            productSuggestions.Top = roundedTextBox1.Bottom;
            productSuggestions.Width = roundedTextBox1.Width;
            LayoutPosSummary();
        }

        private void dataGridView1_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != Quantity.Index) return;

            int quantity;
            DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
            string value = Convert.ToString(e.FormattedValue).Trim();
            if (!int.TryParse(value, out quantity) || quantity < 1)
            {
                e.Cancel = true;
                row.ErrorText = "Enter a whole number greater than zero.";
                return;
            }

            int productId = row.Tag is int ? (int)row.Tag : 0;
            int availableStock;
            if (productStocks.TryGetValue(productId, out availableStock) && quantity > availableStock)
            {
                e.Cancel = true;
                row.ErrorText = "Quantity cannot exceed available stock (" + availableStock + ").";
                return;
            }

            row.ErrorText = string.Empty;
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != Quantity.Index) return;

            DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
            row.ErrorText = string.Empty;
            decimal unitPrice = Convert.ToDecimal(row.Cells[2].Value);
            int quantity = Convert.ToInt32(row.Cells[3].Value);
            row.Cells[4].Value = unitPrice * quantity;
            UpdateSummary();
        }

        private void LayoutPosSummary()
        {
            int contentWidth = Math.Max(180, panel1.ClientSize.Width - 18);
            label9.Top = 190;
            label8.Top = 220;
            label7.Top = 250;
            label6.Top = 282;
            int paymentTop = 310;
            label10.Top = paymentTop;
            checkBox1.Top = paymentTop + 28;
            checkBox2.Top = paymentTop + 28;
            checkBox3.Top = paymentTop + 28;
            label11.Top = paymentTop + 56;
            textBox1.Left = 9;
            textBox1.Width = contentWidth;
            textBox1.Top = label11.Bottom + 8;
            label12.Top = textBox1.Bottom + 8;
            button2.Left = 9;
            button2.Width = contentWidth;
            button2.AutoSize = false;
            button2.Height = 30;
            button2.Top = label12.Bottom + 12;
            label6.Left = 10;
            label6.Width = contentWidth;
            panel1.AutoScrollMinSize = new Size(0, button2.Bottom + 11);
        }

        #endregion

        #region Customer and sales operations

        private sealed class CustomerOption
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public override string ToString()
            {
                return Name;
            }
        }

        private void LoadCustomers()
        {
            try
            {
                flatComboBox1.Items.Clear();
                flatComboBox1.Items.Add(new CustomerOption { Id = 0, Name = "Walk-in Customer" });

                using (OleDbConnection connection = Database.GetConnection())
                using (OleDbCommand command = new OleDbCommand("SELECT CustomerID, CustomerName FROM Customers WHERE IsActive = True ORDER BY CustomerName", connection))
                {
                    connection.Open();
                    using (OleDbDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            flatComboBox1.Items.Add(new CustomerOption
                            {
                                Id = Convert.ToInt32(reader["CustomerID"]),
                                Name = reader["CustomerName"].ToString()
                            });
                        }
                    }
                }

                flatComboBox1.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load customers: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string search = roundedTextBox1.Text.Trim();
            if (string.IsNullOrWhiteSpace(search))
            {
                MessageBox.Show("Search for a product before adding it.", "POS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                using (OleDbConnection connection = Database.GetConnection())
                using (OleDbCommand command = new OleDbCommand(@"
                    SELECT TOP 1 ProductID, ProductName, RetailPrice, StockQuantity
                    FROM Products
                    WHERE IsActive = True
                      AND StockQuantity > 0
                      AND (ProductName LIKE ? OR Category LIKE ? OR ProductCode LIKE ? OR ProductID = ?)
                    ORDER BY ProductName", connection))
                {
                    connection.Open();
                    int productIdSearch;
                    int.TryParse(search, out productIdSearch);
                    command.Parameters.AddWithValue("@Name", "%" + search + "%");
                    command.Parameters.AddWithValue("@Category", "%" + search + "%");
                    command.Parameters.AddWithValue("@ProductCode", "%" + search + "%");
                    command.Parameters.AddWithValue("@ProductID", productIdSearch);

                    using (OleDbDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            MessageBox.Show("No available product matched your search.", "POS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }

                        int productId = Convert.ToInt32(reader["ProductID"]);
                        string productName = reader["ProductName"].ToString();
                        decimal unitPrice = Convert.ToDecimal(reader["RetailPrice"]);
                        int stockQuantity = Convert.ToInt32(reader["StockQuantity"]);
                        productStocks[productId] = stockQuantity;
                        DataGridViewRow existingRow = dataGridView1.Rows
                            .Cast<DataGridViewRow>()
                            .FirstOrDefault(row => row.Tag is int id && id == productId);

                        if (existingRow != null)
                        {
                            int quantity = Convert.ToInt32(existingRow.Cells[3].Value);
                            if (quantity >= stockQuantity)
                            {
                                MessageBox.Show("The requested quantity is not available in stock.", "POS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }

                            existingRow.Cells[3].Value = quantity + 1;
                            existingRow.Cells[4].Value = unitPrice * (quantity + 1);
                        }
                        else
                        {
                            int rowIndex = dataGridView1.Rows.Add(
                                dataGridView1.Rows.Count + 1,
                                productName,
                                unitPrice,
                                1,
                                unitPrice,
                                "Remove");
                            dataGridView1.Rows[rowIndex].Tag = productId;
                            RenumberCartRows();
                        }
                    }
                }

                UpdateSummary();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to add product: " + ex.Message, "POS Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count == 0)
            {
                MessageBox.Show("Add at least one product before completing the sale.", "POS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            decimal subtotal = GetSubtotal();
            decimal vat = subtotal * LocalSettings.TaxRate / 100m;
            decimal total = subtotal + vat;
            string paymentMethod = GetPaymentMethod();
            decimal amountPaid = total;

            if (paymentMethod == "Cash" && (!decimal.TryParse(textBox1.Text.Trim(), out amountPaid) || amountPaid < total))
            {
                MessageBox.Show("Enter a cash amount that is at least the total.", "POS", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            CustomerOption customer = flatComboBox1.SelectedItem as CustomerOption;

            try
            {
                using (OleDbConnection connection = Database.GetConnection())
                {
                    connection.Open();
                    using (OleDbTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            int customerId = customer == null || customer.Id == 0 ? 0 : customer.Id;
                            using (OleDbCommand command = new OleDbCommand(@"
                                INSERT INTO Invoices (CustomerID, CashierUserID, InvoiceDate, Total, Status, PaymentMethod, AmountPaid, ChangeAmount)
                                VALUES (?, ?, Now(), ?, 'Paid', ?, ?, ?)", connection, transaction))
                            {
                                command.Parameters.AddWithValue("@CustomerID", customerId == 0 ? (object)DBNull.Value : customerId);
                                command.Parameters.AddWithValue("@CashierUserID", cashierUserId == 0 ? (object)DBNull.Value : cashierUserId);
                                command.Parameters.AddWithValue("@Total", total);
                                command.Parameters.AddWithValue("@PaymentMethod", paymentMethod);
                                command.Parameters.AddWithValue("@AmountPaid", amountPaid);
                                command.Parameters.AddWithValue("@ChangeAmount", amountPaid - total);
                                command.ExecuteNonQuery();
                            }

                            int invoiceId;
                            using (OleDbCommand identityCommand = new OleDbCommand("SELECT @@IDENTITY", connection, transaction))
                            {
                                invoiceId = Convert.ToInt32(identityCommand.ExecuteScalar());
                            }

                            foreach (DataGridViewRow row in dataGridView1.Rows)
                            {
                                int productId = (int)row.Tag;
                                int quantity = Convert.ToInt32(row.Cells[3].Value);
                                decimal unitPrice = Convert.ToDecimal(row.Cells[2].Value);
                                decimal lineTotal = Convert.ToDecimal(row.Cells[4].Value);

                                using (OleDbCommand itemCommand = new OleDbCommand(@"
                                    INSERT INTO InvoiceItems (InvoiceID, ProductID, Quantity, UnitPrice, Total)
                                    VALUES (?, ?, ?, ?, ?)", connection, transaction))
                                {
                                    itemCommand.Parameters.AddWithValue("@InvoiceID", invoiceId);
                                    itemCommand.Parameters.AddWithValue("@ProductID", productId);
                                    itemCommand.Parameters.AddWithValue("@Quantity", quantity);
                                    itemCommand.Parameters.AddWithValue("@UnitPrice", unitPrice);
                                    itemCommand.Parameters.AddWithValue("@Total", lineTotal);
                                    itemCommand.ExecuteNonQuery();
                                }

                                using (OleDbCommand stockCommand = new OleDbCommand(@"
                                    UPDATE Products
                                    SET StockQuantity = StockQuantity - ?,
                                        Status = IIF(StockQuantity - ? <= 0, 'Out of Stock', IIF(StockQuantity - ? <= 5, 'Low Stock', 'In Stock'))
                                    WHERE ProductID = ? AND StockQuantity >= ?", connection, transaction))
                                {
                                    stockCommand.Parameters.AddWithValue("@Quantity1", quantity);
                                    stockCommand.Parameters.AddWithValue("@Quantity2", quantity);
                                    stockCommand.Parameters.AddWithValue("@Quantity3", quantity);
                                    stockCommand.Parameters.AddWithValue("@ProductID", productId);
                                    stockCommand.Parameters.AddWithValue("@Quantity4", quantity);
                                    if (stockCommand.ExecuteNonQuery() != 1)
                                    {
                                        throw new InvalidOperationException("One of the selected products no longer has enough stock.");
                                    }
                                }
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

                MessageBox.Show("Sale completed. Change: " + (amountPaid - total).ToString("C2"), "POS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                dataGridView1.Rows.Clear();
                productStocks.Clear();
                textBox1.Clear();
                UpdateSummary();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to complete sale: " + ex.Message, "POS Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetPaymentMethod()
        {
            if (checkBox1.Checked) return "Cash";
            if (checkBox2.Checked) return "Card";
            if (checkBox3.Checked) return "e-Wallet";
            return "Cash";
        }

        private decimal GetSubtotal()
        {
            return dataGridView1.Rows.Cast<DataGridViewRow>().Sum(row => Convert.ToDecimal(row.Cells[4].Value));
        }

        private void UpdateSummary()
        {
            decimal subtotal = dataGridView1.Rows.Count == 0 ? 0 : GetSubtotal();
            decimal vat = subtotal * LocalSettings.TaxRate / 100m;
            decimal total = subtotal + vat;
            label3.Text = "Customer: " + (flatComboBox1.SelectedItem == null ? "Walk-in Customer" : flatComboBox1.SelectedItem.ToString());
            label4.Text = "Date: " + DateTime.Now.ToShortDateString();
            label5.Text = "Items: " + dataGridView1.Rows.Count;
            label9.Text = "Subtotal: " + subtotal.ToString("C2");
            label8.Text = "VAT (" + LocalSettings.TaxRate.ToString("0.##") + "%): " + vat.ToString("C2");
            label7.Text = "TOTAL: " + total.ToString("C2");
            label12.Text = "Change: " + CalculateChange(total).ToString("C2");
        }

        private decimal CalculateChange(decimal total)
        {
            decimal amountPaid;
            return decimal.TryParse(textBox1.Text.Trim(), out amountPaid) ? amountPaid - total : 0;
        }

        private void PaymentMethodChanged(object sender, EventArgs e)
        {
            CheckBox selected = sender as CheckBox;
            if (selected != null && selected.Checked)
            {
                foreach (CheckBox checkBox in new[] { checkBox1, checkBox2, checkBox3 })
                {
                    if (checkBox != selected) checkBox.Checked = false;
                }
            }
            UpdateSummary();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == Action.Index)
            {
                dataGridView1.Rows.RemoveAt(e.RowIndex);
                RenumberCartRows();
                UpdateSummary();
            }
        }

        private void RenumberCartRows()
        {
            for (int index = 0; index < dataGridView1.Rows.Count; index++)
            {
                dataGridView1.Rows[index].Cells[Number.Index].Value = index + 1;
            }
        }

        #endregion
    }
}
