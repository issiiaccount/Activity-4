using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Activity1_FinalTerm
{
    public partial class ucInvoicesHistory : UserControl
    {
        #region Initialization and layout

        public ucInvoicesHistory()
        {
            InitializeComponent();
            MinimumSize = Size.Empty;
            AutoScroll = true;
            roundedTextBox1.BackColor = Color.White;
            roundedTextBox1.BorderActiveColor = Color.FromArgb(32, 150, 110);
            roundedTextBox1.BorderColor = Color.FromArgb(210, 220, 220);
            roundedTextBox1.BorderInactiveColor = Color.FromArgb(210, 220, 220);
            roundedTextBox1.BorderRadius = 10;
            roundedTextBox1.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point, 0);
            roundedTextBox1.Height = 30;
            roundedTextBox1.Padding = new Padding(10, 4, 10, 4);
            roundedTextBox1.PlaceholderColor = Color.Gray;
            roundedTextBox1.PlaceholderText = "Search by Invoice ID, Customer Name, Phone...";
            flatComboBox2.Height = 24;
            btnReceipt.Height = 30;
            btnRefund.Height = 30;
            ApplyInvoiceButtonStyle(btnReceipt);
            ApplyInvoiceButtonStyle(btnRefund);
            dataGridView2.AutoGenerateColumns = false;
            dataGridView2.AllowUserToAddRows = false;
            dataGridView2.RowTemplate.Height = 30;
            dataGridView2.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
            dataGridView2.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            dataGridView2.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            InvoiceID.MinimumWidth = 70;
            CustomerName.MinimumWidth = 120;
            Total.MinimumWidth = 90;
            Status.MinimumWidth = 85;
            Cashier.MinimumWidth = 90;
            Date.MinimumWidth = 110;
            foreach (DataGridViewColumn column in dataGridView2.Columns)
            {
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }

            roundedTextBox1.TextChanged += (sender, args) => LoadInvoices();
            flatComboBox2.SelectedIndexChanged += (sender, args) => LoadInvoices();
            dataGridView2.SelectionChanged += dataGridView2_SelectionChanged;
            btnRefund.Click += btnRefund_Click;
            btnReceipt.Click += btnReceipt_Click;
            LoadInvoices();
            Resize += (sender, args) => LayoutInvoicesPage();
            LayoutInvoicesPage();
        }

        private void ApplyInvoiceButtonStyle(Button button)
        {
            button.BackColor = Color.FromArgb(40, 161, 116);
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(49, 177, 130);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(31, 130, 94);
            button.FlatStyle = FlatStyle.Flat;
            button.Font = new Font("Segoe UI", 8.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            button.ForeColor = Color.White;
            button.Height = 30;
            button.Cursor = Cursors.Hand;
            button.Padding = new Padding(0);
            button.TextAlign = ContentAlignment.MiddleCenter;
            button.UseVisualStyleBackColor = false;
        }

        private void LayoutInvoicesPage()
        {
            const int margin = 21;
            const int gap = 14;
            const int filterTop = 81;
            int width = Math.Max(1, ClientSize.Width);
            int height = Math.Max(1, ClientSize.Height);
            int tableTop = 132;

            roundedTextBox1.Left = margin;
            roundedTextBox1.Top = filterTop;

            if (width >= 760)
            {
                int summaryWidth = Math.Min(280, Math.Max(240, width / 3));
                panel1.Left = width - margin - summaryWidth;
                panel1.Top = tableTop;
                panel1.Width = summaryWidth;
                panel1.Height = Math.Max(240, height - tableTop - margin);

                flatComboBox2.Left = panel1.Left - gap - flatComboBox2.Width;
                roundedTextBox1.Width = Math.Max(180, flatComboBox2.Left - gap - margin);
                flatComboBox2.Top = filterTop;

                dataGridView2.Left = margin;
                dataGridView2.Top = tableTop;
                dataGridView2.Width = Math.Max(180, panel1.Left - gap - margin);
                dataGridView2.Height = Math.Max(180, height - tableTop - margin);
            }
            else
            {
                roundedTextBox1.Width = Math.Max(180, width - (margin * 2));
                flatComboBox2.Left = margin;
                flatComboBox2.Top = roundedTextBox1.Bottom + gap;
                tableTop = flatComboBox2.Bottom + gap;

                dataGridView2.Left = margin;
                dataGridView2.Top = tableTop;
                dataGridView2.Width = Math.Max(180, width - (margin * 2));
                dataGridView2.Height = 260;
                panel1.Left = margin;
                panel1.Top = dataGridView2.Bottom + gap;
                panel1.Width = Math.Max(240, width - (margin * 2));
                panel1.Height = 432;
            }

            LayoutInvoiceSummary();
        }

        private void LayoutInvoiceSummary()
        {
            int contentWidth = Math.Max(180, panel1.ClientSize.Width - 18);
            foreach (Label label in new[] { label3, label4, label5, label7, label8, label9, label10, label11, label12 })
            {
                label.MaximumSize = new Size(contentWidth, 0);
                label.AutoEllipsis = true;
            }

            btnReceipt.Left = 11;
            btnRefund.Left = Math.Max(btnReceipt.Right + 11, panel1.ClientSize.Width - btnRefund.Width - 11);
            btnReceipt.Top = panel1.ClientSize.Height - btnReceipt.Height - 11;
            btnRefund.Top = btnReceipt.Top;
        }

        #endregion

        #region Invoice data and commands

        private void LoadInvoices()
        {
            try
            {
                dataGridView2.Rows.Clear();
                string search = roundedTextBox1.Text.Trim();
                string status = flatComboBox2.SelectedItem == null ? "All Status" : flatComboBox2.SelectedItem.ToString();
                int invoiceId;
                int.TryParse(search, out invoiceId);

                using (OleDbConnection connection = Database.GetConnection())
                using (OleDbCommand command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = @"
                        SELECT i.InvoiceID, c.CustomerName, i.Total, i.Status,
                               u.FullName AS Cashier, i.InvoiceDate
                        FROM (Invoices AS i
                        LEFT JOIN Customers AS c ON i.CustomerID = c.CustomerID)
                        LEFT JOIN Users AS u ON i.CashierUserID = u.UserID
                        WHERE (? = 'All Status' OR i.Status = ?)
                          AND (? = '' OR c.CustomerName LIKE ? OR i.InvoiceID = ?)
                        ORDER BY i.InvoiceDate DESC";
                    command.Parameters.AddWithValue("@StatusAll", status);
                    command.Parameters.AddWithValue("@Status", status);
                    command.Parameters.AddWithValue("@SearchEmpty", search);
                    command.Parameters.AddWithValue("@SearchName", "%" + search + "%");
                    command.Parameters.AddWithValue("@InvoiceID", invoiceId);

                    using (OleDbDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int rowIndex = dataGridView2.Rows.Add(
                                reader["InvoiceID"],
                                reader["CustomerName"] == DBNull.Value ? "Walk-in Customer" : reader["CustomerName"],
                                Convert.ToDecimal(reader["Total"]).ToString("C2"),
                                reader["Status"],
                                reader["Cashier"] == DBNull.Value ? "System" : reader["Cashier"],
                                Convert.ToDateTime(reader["InvoiceDate"]).ToString("g"));
                            dataGridView2.Rows[rowIndex].Tag = Convert.ToInt32(reader["InvoiceID"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load invoices: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dataGridView2_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView2.SelectedRows.Count == 0 || dataGridView2.SelectedRows[0].Tag == null)
            {
                return;
            }

            LoadInvoiceDetails((int)dataGridView2.SelectedRows[0].Tag);
        }

        private void LoadInvoiceDetails(int invoiceId)
        {
            try
            {
                using (OleDbConnection connection = Database.GetConnection())
                {
                    connection.Open();
                    using (OleDbCommand invoiceCommand = new OleDbCommand(@"
                        SELECT i.Total, i.InvoiceDate, i.PaymentMethod, i.AmountPaid, i.ChangeAmount,
                               c.CustomerName
                        FROM Invoices AS i LEFT JOIN Customers AS c ON i.CustomerID = c.CustomerID
                        WHERE i.InvoiceID = ?", connection))
                    {
                        invoiceCommand.Parameters.AddWithValue("@InvoiceID", invoiceId);
                        using (OleDbDataReader reader = invoiceCommand.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                decimal total = Convert.ToDecimal(reader["Total"]);
                                label3.Text = "Customer: " + (reader["CustomerName"] == DBNull.Value ? "Walk-in Customer" : reader["CustomerName"]);
                                label4.Text = "Date: " + Convert.ToDateTime(reader["InvoiceDate"]).ToString("g");
                                label7.Text = "TOTAL: " + total.ToString("C2");
                                label8.Text = "VAT (10%): " + (total / 11).ToString("C2");
                                label9.Text = "Subtotal: " + (total - (total / 11)).ToString("C2");
                                label10.Text = "Payment Method: " + reader["PaymentMethod"];
                                label11.Text = "Cash Received: " + Convert.ToDecimal(reader["AmountPaid"]).ToString("C2");
                                label12.Text = "Change: " + Convert.ToDecimal(reader["ChangeAmount"]).ToString("C2");
                            }
                        }
                    }

                    using (OleDbCommand itemCommand = new OleDbCommand("SELECT SUM(Quantity) FROM InvoiceItems WHERE InvoiceID = ?", connection))
                    {
                        itemCommand.Parameters.AddWithValue("@InvoiceID", invoiceId);
                        object itemCount = itemCommand.ExecuteScalar();
                        label5.Text = "Items: " + (itemCount == DBNull.Value || itemCount == null ? 0 : Convert.ToInt32(itemCount));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load invoice details: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRefund_Click(object sender, EventArgs e)
        {
            if (dataGridView2.SelectedRows.Count == 0 || dataGridView2.SelectedRows[0].Tag == null)
            {
                MessageBox.Show("Select an invoice first.", "Invoices", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int invoiceId = (int)dataGridView2.SelectedRows[0].Tag;
            if (MessageBox.Show("Issue a refund for this invoice?", "Confirm Refund", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                using (OleDbConnection connection = Database.GetConnection())
                {
                    connection.Open();
                    using (OleDbTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            string currentStatus;
                            using (OleDbCommand statusCommand = new OleDbCommand("SELECT Status FROM Invoices WHERE InvoiceID = ?", connection, transaction))
                            {
                                statusCommand.Parameters.AddWithValue("@InvoiceID", invoiceId);
                                object status = statusCommand.ExecuteScalar();
                                currentStatus = status == null || status == DBNull.Value ? "" : status.ToString();
                            }

                            if (string.Equals(currentStatus, "Refunded", StringComparison.OrdinalIgnoreCase))
                            {
                                transaction.Rollback();
                                MessageBox.Show("This invoice has already been refunded.", "Invoices", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }

                            using (OleDbCommand itemCommand = new OleDbCommand("SELECT ProductID, Quantity FROM InvoiceItems WHERE InvoiceID = ?", connection, transaction))
                            {
                                itemCommand.Parameters.AddWithValue("@InvoiceID", invoiceId);
                                using (OleDbDataReader reader = itemCommand.ExecuteReader())
                                {
                                    List<Tuple<int, int>> items = new List<Tuple<int, int>>();
                                    while (reader.Read())
                                    {
                                        items.Add(Tuple.Create(Convert.ToInt32(reader["ProductID"]), Convert.ToInt32(reader["Quantity"])));
                                    }

                                    foreach (Tuple<int, int> item in items)
                                    {
                                        using (OleDbCommand stockCommand = new OleDbCommand(@"
                                            UPDATE Products
                                            SET StockQuantity = StockQuantity + ?,
                                                Status = IIF(StockQuantity + ? <= 0, 'Out of Stock', IIF(StockQuantity + ? <= 5, 'Low Stock', 'In Stock'))
                                            WHERE ProductID = ?", connection, transaction))
                                        {
                                            stockCommand.Parameters.AddWithValue("@Quantity1", item.Item2);
                                            stockCommand.Parameters.AddWithValue("@Quantity2", item.Item2);
                                            stockCommand.Parameters.AddWithValue("@Quantity3", item.Item2);
                                            stockCommand.Parameters.AddWithValue("@ProductID", item.Item1);
                                            stockCommand.ExecuteNonQuery();
                                        }
                                    }
                                }
                            }

                            using (OleDbCommand command = new OleDbCommand("UPDATE Invoices SET Status = 'Refunded' WHERE InvoiceID = ?", connection, transaction))
                            {
                                command.Parameters.AddWithValue("@InvoiceID", invoiceId);
                                command.ExecuteNonQuery();
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
                LoadInvoices();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to issue refund: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnReceipt_Click(object sender, EventArgs e)
        {
            if (dataGridView2.SelectedRows.Count == 0 || dataGridView2.SelectedRows[0].Tag == null)
            {
                MessageBox.Show("Select an invoice first.", "Invoices", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                string receipt = BuildReceipt((int)dataGridView2.SelectedRows[0].Tag);
                using (PrintDocument printDocument = new PrintDocument())
                {
                    printDocument.PrintPage += (senderObject, printEvent) =>
                    {
                        using (Font font = new Font("Consolas", 9))
                        {
                            printEvent.Graphics.DrawString(receipt, font, Brushes.Black, printEvent.MarginBounds.Left, printEvent.MarginBounds.Top);
                        }
                    };

                    using (PrintPreviewDialog preview = new PrintPreviewDialog { Document = printDocument, Width = 900, Height = 700 })
                    {
                        preview.ShowDialog(this);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to prepare receipt: " + ex.Message, "Receipt Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string BuildReceipt(int invoiceId)
        {
            StringBuilder receipt = new StringBuilder();
            using (OleDbConnection connection = Database.GetConnection())
            {
                connection.Open();
                using (OleDbCommand command = new OleDbCommand(@"
                    SELECT i.InvoiceID, i.InvoiceDate, i.Total, i.PaymentMethod, i.AmountPaid, i.ChangeAmount,
                           c.CustomerName
                    FROM Invoices AS i LEFT JOIN Customers AS c ON i.CustomerID = c.CustomerID
                    WHERE i.InvoiceID = ?", connection))
                {
                    command.Parameters.AddWithValue("@InvoiceID", invoiceId);
                    using (OleDbDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.Read()) throw new InvalidOperationException("Invoice was not found.");
                        receipt.AppendLine("SALES RECEIPT");
                        receipt.AppendLine("Invoice #: " + reader["InvoiceID"]);
                        receipt.AppendLine("Date: " + Convert.ToDateTime(reader["InvoiceDate"]).ToString("g"));
                        receipt.AppendLine("Customer: " + (reader["CustomerName"] == DBNull.Value ? "Walk-in Customer" : reader["CustomerName"]));
                        receipt.AppendLine(new string('-', 42));
                        receipt.AppendLine("Product                         Qty       Total");
                        receipt.AppendLine(new string('-', 42));
                    }
                }

                using (OleDbCommand itemCommand = new OleDbCommand(@"
                    SELECT p.ProductName, ii.Quantity, ii.Total
                    FROM InvoiceItems AS ii INNER JOIN Products AS p ON ii.ProductID = p.ProductID
                    WHERE ii.InvoiceID = ?", connection))
                {
                    itemCommand.Parameters.AddWithValue("@InvoiceID", invoiceId);
                    using (OleDbDataReader reader = itemCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string productName = reader["ProductName"].ToString();
                            if (productName.Length > 28) productName = productName.Substring(0, 28);
                            receipt.AppendLine(productName.PadRight(30) + Convert.ToInt32(reader["Quantity"]).ToString().PadLeft(4) + Convert.ToDecimal(reader["Total"]).ToString("C2").PadLeft(8));
                        }
                    }
                }

                using (OleDbCommand totalCommand = new OleDbCommand("SELECT Total, PaymentMethod, AmountPaid, ChangeAmount FROM Invoices WHERE InvoiceID = ?", connection))
                {
                    totalCommand.Parameters.AddWithValue("@InvoiceID", invoiceId);
                    using (OleDbDataReader reader = totalCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            decimal total = Convert.ToDecimal(reader["Total"]);
                            receipt.AppendLine(new string('-', 42));
                            receipt.AppendLine("TOTAL: ".PadRight(30) + total.ToString("C2").PadLeft(8));
                            receipt.AppendLine("Payment: " + reader["PaymentMethod"]);
                            receipt.AppendLine("Paid: ".PadRight(30) + Convert.ToDecimal(reader["AmountPaid"]).ToString("C2").PadLeft(8));
                            receipt.AppendLine("Change: ".PadRight(30) + Convert.ToDecimal(reader["ChangeAmount"]).ToString("C2").PadLeft(8));
                        }
                    }
                }
            }
            return receipt.ToString();
        }

        private void ucInvoicesHistory_Load(object sender, EventArgs e)
        {

        }

        private void roundedTextBox1_Load(object sender, EventArgs e)
        {
            roundedTextBox1.PlaceholderText = "Search by Invoice ID, Customer Name, Phone...";
            roundedTextBox1.PlaceholderColor = Color.Gray;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void flatComboBox1_Load(object sender, EventArgs e)
        {
        }

        private void flatComboBox2_Load(object sender, EventArgs e)
        {
            flatComboBox2.SelectedIndex = 0;
        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        #endregion
    }
}
