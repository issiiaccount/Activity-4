using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Activity1_FinalTerm
{
    public partial class ucCustomers : UserControl
    {
        #region Initialization and layout

        public ucCustomers()
        {
            InitializeComponent();
            MinimumSize = Size.Empty;
            roundedTextBox1.BackColor = Color.White;
            roundedTextBox1.BorderActiveColor = Color.FromArgb(32, 150, 110);
            roundedTextBox1.BorderColor = Color.FromArgb(210, 220, 220);
            roundedTextBox1.BorderInactiveColor = Color.FromArgb(210, 220, 220);
            roundedTextBox1.BorderRadius = 10;
            roundedTextBox1.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point, 0);
            roundedTextBox1.Height = 30;
            roundedTextBox1.Padding = new Padding(10, 4, 10, 4);
            roundedTextBox1.PlaceholderColor = Color.Gray;
            button1.Height = 30;
            button1.Width = 120;
            button2.Height = 30;
            button3.Height = 30;
            button4.Height = 30;
            ApplyCustomersButtonStyle(button1);
            ApplyCustomersButtonStyle(button2);
            ApplyCustomersButtonStyle(button3);
            ApplyCustomersButtonStyle(button4);
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.RowTemplate.Height = 30;
            dataGridView1.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
            dataGridView1.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            CustomerID.MinimumWidth = 80;
            CustomerName.MinimumWidth = 120;
            ContactNumber.MinimumWidth = 105;
            EmailAddress.MinimumWidth = 145;
            TotalOrderPurchased.MinimumWidth = 115;
            OutstandingBalance.MinimumWidth = 115;
            CustomerID.FillWeight = 70;
            CustomerName.FillWeight = 140;
            ContactNumber.FillWeight = 115;
            EmailAddress.FillWeight = 170;
            TotalOrderPurchased.FillWeight = 145;
            OutstandingBalance.FillWeight = 145;
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }

            dataGridView1.CellDoubleClick += dataGridView1_CellDoubleClick;
            button1.Click += button1_Click;
            button2.Click += button2_Click;
            button3.Click += button3_Click;
            button4.Click += button4_Click;
            roundedTextBox1.TextChanged += (sender, args) => LoadCustomers();
            Resize += (sender, args) => LayoutCustomersPage();
            LoadCustomers();
            LayoutCustomersPage();
        }

        private void ApplyCustomersButtonStyle(Button button)
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

        private void LayoutCustomersPage()
        {
            const int margin = 21;
            const int gap = 12;
            const int filterTop = 81;
            const int tableTop = 132;
            int width = Math.Max(1, ClientSize.Width);
            int currentTableTop = tableTop;

            roundedTextBox1.Left = margin;
            roundedTextBox1.Width = Math.Max(180, width - (margin * 2));

            if (width >= 760)
            {
                button1.Left = width - margin - button1.Width;
                roundedTextBox1.Width = Math.Max(180, button1.Left - gap - margin);
                button1.Top = filterTop;
            }
            else
            {
                button1.Left = margin;
                button1.Top = roundedTextBox1.Bottom + gap;
                currentTableTop = button1.Bottom + gap;
            }

            dataGridView1.Left = margin;
            dataGridView1.Top = currentTableTop;
            dataGridView1.Width = Math.Max(1, width - (margin * 2));

            int footerHeight = Math.Max(button2.Height, label1.Height) + margin;
            dataGridView1.Height = Math.Max(120, ClientSize.Height - currentTableTop - footerHeight);

            button2.Left = margin;
            button2.Top = dataGridView1.Bottom + gap;
            button3.Left = button2.Right + gap;
            button3.Top = button2.Top;
            button4.Left = button3.Right + gap;
            button4.Top = button2.Top;
            label1.Left = Math.Max(margin, width - margin - label1.Width);
            label1.Top = button2.Top + 2;
        }

        #endregion

        #region Customer commands

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) button3_Click(sender, EventArgs.Empty);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Dictionary<string, string> values;
            if (!ShowEditor("Register New Customer", new[] { "Customer Name", "Contact Number", "Email Address" }, null, out values)) return;
            if (string.IsNullOrWhiteSpace(values["Customer Name"]))
            {
                MessageBox.Show("Customer name is required.", "Customers", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!IsValidEmail(values["Email Address"]))
            {
                MessageBox.Show("Enter a valid email address or leave the email field blank.", "Customers", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (OleDbConnection connection = Database.GetConnection())
                using (OleDbCommand command = new OleDbCommand("INSERT INTO Customers (CustomerName, ContactNumber, EmailAddress, IsActive) VALUES (?, ?, ?, True)", connection))
                {
                    connection.Open();
                    command.Parameters.AddWithValue("@Name", values["Customer Name"]);
                    command.Parameters.AddWithValue("@Phone", values["Contact Number"]);
                    command.Parameters.AddWithValue("@Email", values["Email Address"]);
                    command.ExecuteNonQuery();
                }
                LoadCustomers();
            }
            catch (Exception ex) { MessageBox.Show("Unable to register customer: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = GetSelectedRow();
            if (row == null) return;
            Dictionary<string, string> values = new Dictionary<string, string>
            {
                ["Customer Name"] = Convert.ToString(row.Cells[1].Value),
                ["Contact Number"] = Convert.ToString(row.Cells[2].Value),
                ["Email Address"] = Convert.ToString(row.Cells[3].Value)
            };
            Dictionary<string, string> updated;
            if (!ShowEditor("Update Customer", new[] { "Customer Name", "Contact Number", "Email Address" }, values, out updated)) return;
            if (string.IsNullOrWhiteSpace(updated["Customer Name"]))
            {
                MessageBox.Show("Customer name is required.", "Customers", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!IsValidEmail(updated["Email Address"]))
            {
                MessageBox.Show("Enter a valid email address or leave the email field blank.", "Customers", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (OleDbConnection connection = Database.GetConnection())
                using (OleDbCommand command = new OleDbCommand("UPDATE Customers SET CustomerName = ?, ContactNumber = ?, EmailAddress = ? WHERE CustomerID = ?", connection))
                {
                    connection.Open();
                    command.Parameters.AddWithValue("@Name", updated["Customer Name"]);
                    command.Parameters.AddWithValue("@Phone", updated["Contact Number"]);
                    command.Parameters.AddWithValue("@Email", updated["Email Address"]);
                    command.Parameters.AddWithValue("@ID", Convert.ToInt32(row.Cells[0].Value));
                    command.ExecuteNonQuery();
                }
                LoadCustomers();
            }
            catch (Exception ex) { MessageBox.Show("Unable to update customer: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = GetSelectedRow();
            if (row == null) return;

            int customerId = Convert.ToInt32(row.Cells[0].Value);
            string customerName = Convert.ToString(row.Cells[1].Value);
            try
            {
                using (OleDbConnection connection = Database.GetConnection())
                {
                    connection.Open();
                    using (OleDbCommand command = new OleDbCommand(@"
                        SELECT InvoiceID, InvoiceDate, Total, Status, PaymentMethod
                        FROM Invoices
                        WHERE CustomerID = ?
                        ORDER BY InvoiceDate DESC", connection))
                    {
                        command.Parameters.AddWithValue("@CustomerID", customerId);

                        using (OleDbDataReader reader = command.ExecuteReader())
                        {
                            using (Form history = new Form
                            {
                                Text = "Purchase History - " + customerName,
                                StartPosition = FormStartPosition.CenterParent,
                                Size = new Size(720, 380),
                                MinimumSize = new Size(600, 300),
                                BackColor = Color.White,
                                ShowInTaskbar = false
                            })
                            {
                                DataGridView grid = new DataGridView
                                {
                                    Dock = DockStyle.Fill,
                                    ReadOnly = true,
                                    AllowUserToAddRows = false,
                                    AllowUserToDeleteRows = false,
                                    AutoGenerateColumns = false,
                                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                                    BackgroundColor = Color.White,
                                    MultiSelect = false,
                                    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                                    RowHeadersVisible = false
                                };
                                grid.Columns.Add("InvoiceID", "Invoice ID");
                                grid.Columns.Add("InvoiceDate", "Date");
                                grid.Columns.Add("Total", "Total");
                                grid.Columns.Add("Status", "Status");
                                grid.Columns.Add("PaymentMethod", "Payment Method");

                                while (reader.Read())
                                {
                                    grid.Rows.Add(
                                        reader["InvoiceID"],
                                        Convert.ToDateTime(reader["InvoiceDate"]).ToString("g"),
                                        Convert.ToDecimal(reader["Total"]).ToString("C2"),
                                        reader["Status"],
                                        reader["PaymentMethod"] == DBNull.Value ? "" : reader["PaymentMethod"]);
                                }

                                if (grid.Rows.Count == 0)
                                {
                                    grid.Columns.Add("Message", "Purchase History");
                                    grid.Rows.Add("No purchases found for this customer.");
                                }

                                history.Controls.Add(grid);
                                history.ShowDialog(FindForm());
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Unable to load purchase history: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = GetSelectedRow();
            if (row == null) return;

            if (MessageBox.Show("Deactivate the selected customer? Existing purchase history will be preserved.", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                using (OleDbConnection connection = Database.GetConnection())
                using (OleDbCommand command = new OleDbCommand("UPDATE Customers SET IsActive = False WHERE CustomerID = ?", connection))
                {
                    connection.Open();
                    command.Parameters.AddWithValue("@ID", Convert.ToInt32(row.Cells[0].Value));
                    command.ExecuteNonQuery();
                }
                LoadCustomers();
            }
            catch (Exception ex) { MessageBox.Show("Unable to delete customer: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private DataGridViewRow GetSelectedRow()
        {
            DataGridViewRow row = dataGridView1.SelectedRows.Count > 0
                ? dataGridView1.SelectedRows[0]
                : dataGridView1.CurrentRow;
            if (row == null || row.IsNewRow)
            {
                MessageBox.Show("Select a customer first.", "Customers", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }
            return row;
        }

        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return true;

            try
            {
                MailAddress address = new MailAddress(email.Trim());
                return string.Equals(address.Address, email.Trim(), StringComparison.OrdinalIgnoreCase);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        internal static bool ShowEditor(string title, string[] fieldNames, IDictionary<string, string> values, out Dictionary<string, string> result)
        {
            using (Form dialog = new Form
            {
                Text = title,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MinimizeBox = false,
                MaximizeBox = false,
                ShowInTaskbar = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(12)
            })
            {
                TableLayoutPanel layout = new TableLayoutPanel { AutoSize = true, ColumnCount = 2, Dock = DockStyle.Fill };
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 250));
                Dictionary<string, TextBox> controls = new Dictionary<string, TextBox>();

                foreach (string fieldName in fieldNames)
                {
                    layout.Controls.Add(new Label { Text = fieldName + ":", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 7, 8, 3) });
                    TextBox textBox = new TextBox { Width = 240, Anchor = AnchorStyles.Left | AnchorStyles.Right };
                    string value;
                    if (values != null && values.TryGetValue(fieldName, out value)) textBox.Text = value;
                    controls[fieldName] = textBox;
                    layout.Controls.Add(textBox);
                }

                FlowLayoutPanel buttons = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Bottom };
                Button cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, AutoSize = true };
                Button save = new Button { Text = "Save", DialogResult = DialogResult.OK, AutoSize = true };
                buttons.Controls.Add(cancel);
                buttons.Controls.Add(save);
                dialog.Controls.Add(layout);
                dialog.Controls.Add(buttons);
                dialog.AcceptButton = save;
                dialog.CancelButton = cancel;

                result = new Dictionary<string, string>();
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return false;
                }

                foreach (KeyValuePair<string, TextBox> control in controls)
                {
                    result[control.Key] = control.Value.Text.Trim();
                }
                return true;
            }
        }

        private void LoadCustomers()
        {
            try
            {
                dataGridView1.Rows.Clear();
                string search = roundedTextBox1.Text.Trim();

                using (OleDbConnection connection = Database.GetConnection())
                using (OleDbCommand command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = @"
                        SELECT CustomerID, CustomerName, ContactNumber, EmailAddress,
                               TotalOrderPurchased, OutstandingBalance
                        FROM Customers
                        WHERE IsActive = True
                          AND (CustomerName LIKE ? OR EmailAddress LIKE ? OR ContactNumber LIKE ?)
                        ORDER BY CustomerName";

                    string pattern = "%" + search + "%";
                    command.Parameters.AddWithValue("@Name", pattern);
                    command.Parameters.AddWithValue("@Email", pattern);
                    command.Parameters.AddWithValue("@Phone", pattern);

                    using (OleDbDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dataGridView1.Rows.Add(
                                reader["CustomerID"],
                                reader["CustomerName"],
                                reader["ContactNumber"],
                                reader["EmailAddress"],
                                reader["TotalOrderPurchased"],
                                Convert.ToDecimal(reader["OutstandingBalance"]).ToString("C2"));
                        }
                    }
                }

                label1.Text = "Showing " + dataGridView1.Rows.Count + " Active Profiles";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load customers: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion
    }
}
