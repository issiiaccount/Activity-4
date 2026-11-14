using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Activity1_FinalTerm
{
    public partial class ucSalesReport : UserControl
    {
        private readonly DataGridView reportGrid = new DataGridView();

        #region Initialization

        public ucSalesReport()
        {
            InitializeComponent();
            MinimumSize = Size.Empty;
            reportGrid.Location = new Point(21, 81);
            reportGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            reportGrid.Size = new Size(Math.Max(1, ClientSize.Width - 42), Math.Max(120, ClientSize.Height - 102));
            reportGrid.BackgroundColor = Color.White;
            reportGrid.BorderStyle = BorderStyle.None;
            reportGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            reportGrid.AllowUserToAddRows = false;
            reportGrid.ReadOnly = true;
            reportGrid.RowHeadersVisible = false;
            reportGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            reportGrid.RowTemplate.Height = 30;
            reportGrid.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
            reportGrid.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            reportGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            reportGrid.Columns.Add("SalesMonth", "Month");
            reportGrid.Columns.Add("InvoiceCount", "Invoices");
            reportGrid.Columns.Add("UnitsSold", "Units Sold");
            reportGrid.Columns.Add("TotalSales", "Total Sales");
            Controls.Add(reportGrid);
            Resize += (sender, args) =>
            {
                reportGrid.Left = 21;
                reportGrid.Top = 81;
                reportGrid.Width = Math.Max(1, ClientSize.Width - 42);
                reportGrid.Height = Math.Max(120, ClientSize.Height - 102);
            };
            LoadReport();
        }

        #endregion

        #region Report data

        private void LoadReport()
        {
            try
            {
                reportGrid.Rows.Clear();
                using (OleDbConnection connection = Database.GetConnection())
                using (OleDbCommand command = new OleDbCommand(@"
                    SELECT Format(i.InvoiceDate, 'yyyy-mm') AS SalesMonth,
                           Count(i.InvoiceID) AS InvoiceCount,
                           Sum(ii.Quantity) AS UnitsSold,
                           Sum(i.Total) AS TotalSales
                    FROM Invoices AS i INNER JOIN InvoiceItems AS ii ON i.InvoiceID = ii.InvoiceID
                    WHERE i.Status = 'Paid'
                    GROUP BY Format(i.InvoiceDate, 'yyyy-mm')
                    ORDER BY Format(i.InvoiceDate, 'yyyy-mm') DESC", connection))
                {
                    connection.Open();
                    using (OleDbDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            reportGrid.Rows.Add(
                                reader["SalesMonth"],
                                reader["InvoiceCount"],
                                reader["UnitsSold"],
                                Convert.ToDecimal(reader["TotalSales"]).ToString("C2"));
                        }
                    }

        #endregion
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load sales report: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
