using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Data.OleDb;

namespace Activity1_FinalTerm
{
    public partial class ucDashboard : UserControl
    {
        #region Initialization and layout

        public ucDashboard()
        {
            InitializeComponent();
            Resize += (sender, args) => LayoutDashboardPage();
            LayoutDashboardPage();
        }

        private void LayoutDashboardPage()
        {
            int margin = 35;
            int gap = ClientSize.Width >= 900 ? 26 : 16;
            int columns = ClientSize.Width >= 900 ? 4 : 2;
            int availableWidth = Math.Max(300, ClientSize.Width - (margin * 2));
            int cardWidth = Math.Max(150, (availableWidth - (gap * (columns - 1))) / columns);
            Panel[] cards = { panel3, panel4, panel7, panel9 };
            Panel[] accents = { panel6, panel5, panel8, panel10 };

            for (int i = 0; i < cards.Length; i++)
            {
                int row = i / columns;
                int column = i % columns;
                cards[i].Left = margin + (cardWidth + gap) * column;
                cards[i].Top = 67 + (row * (cards[i].Height + gap));
                cards[i].Width = cardWidth;
                accents[i].Left = cards[i].Left - 12;
                accents[i].Top = cards[i].Top;
                accents[i].Width = cardWidth;
                accents[i].Height = cards[i].Height;
            }

            chart1.Left = margin;
            chart1.Width = Math.Max(300, ClientSize.Width - (margin * 2));
            chart1.Top = cards[3].Bottom + 32;
            chart1.Height = Math.Max(180, ClientSize.Height - chart1.Top - 24);

            foreach (Panel card in cards)
            {
                foreach (Control control in card.Controls)
                {
                    if (control is Label label)
                    {
                        label.Font = new Font("Segoe UI", cardWidth < 175 ? 10F : 12F, FontStyle.Bold);
                    }
                }

        #endregion

        #region Dashboard data
            }
        }

        private void ucDashboard_Load(object sender, EventArgs e)
        {
            LoadSummary();
        }

        private void LoadSummary()
        {
            try
            {
                using (OleDbConnection connection = Database.GetConnection())
                {
                    connection.Open();
                    label11.Text = ExecuteScalarInt(connection, "SELECT COUNT(*) FROM Products WHERE IsActive = True").ToString("N0");
                    label13.Text = ExecuteScalarInt(connection, "SELECT COUNT(*) FROM Products WHERE IsActive = True AND StockQuantity > 0 AND StockQuantity <= 5").ToString("N0");
                    label14.Text = ExecuteScalarInt(connection, "SELECT COUNT(*) FROM Products WHERE IsActive = True AND StockQuantity <= 0").ToString("N0");
                    label12.Text = ExecuteScalarDecimal(connection, "SELECT Sum(RetailPrice * StockQuantity) FROM Products WHERE IsActive = True").ToString("C0");

                    chart1.Series.Clear();
                    Series series = new Series("SalesData")
                    {
                        ChartType = SeriesChartType.Line,
                        BorderWidth = 3,
                        IsValueShownAsLabel = true
                    };

                    using (OleDbCommand command = new OleDbCommand(@"
                        SELECT Format(InvoiceDate, 'yyyy-mm') AS SalesMonth, Sum(Total) AS TotalSales
                        FROM Invoices
                        WHERE Status = 'Paid'
                        GROUP BY Format(InvoiceDate, 'yyyy-mm')
                        ORDER BY Format(InvoiceDate, 'yyyy-mm')", connection))
                    using (OleDbDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            series.Points.AddXY(reader["SalesMonth"].ToString(), Convert.ToDecimal(reader["TotalSales"]));
                        }
                    }

                    chart1.ChartAreas[0].AxisY.LabelStyle.Format = "'₱'#,##0";
                    chart1.Series.Add(series);
                    chart1.Titles.Clear();
                    chart1.Titles.Add("Sales Data");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load dashboard summary: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static int ExecuteScalarInt(OleDbConnection connection, string sql)
        {
            using (OleDbCommand command = new OleDbCommand(sql, connection))
            {
                object value = command.ExecuteScalar();
                return value == null || value == DBNull.Value ? 0 : Convert.ToInt32(value);
            }
        }

        private static decimal ExecuteScalarDecimal(OleDbConnection connection, string sql)
        {
            using (OleDbCommand command = new OleDbCommand(sql, connection))
            {
                object value = command.ExecuteScalar();
                return value == null || value == DBNull.Value ? 0 : Convert.ToDecimal(value);
            }
        }

        #endregion
    }
}
