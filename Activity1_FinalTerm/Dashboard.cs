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

namespace Activity1_FinalTerm
{
    public partial class Dashboard : Form
    {
        private readonly int cashierUserId;

        #region Initialization

        public Dashboard(string DisplayName) : this(DisplayName, 0)
        {
        }

        public Dashboard(string DisplayName, int userId)
        {
            InitializeComponent();
            AutoScaleMode = AutoScaleMode.Dpi;
            cashierUserId = userId;

            txtUsername.Text = DisplayName;

            txtUsername.AutoSize = true;
            txtUsername.ForeColor = UiTheme.Ink;
            label1.Text = "SALES HUB";
            label1.ForeColor = Color.FromArgb(230, 244, 237);
            label1.Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold);
            label1.Padding = new Padding(22, 0, 0, 0);
            label3.BackColor = UiTheme.NavigationHover;
            label4.BackColor = UiTheme.Border;
            label5.BackColor = UiTheme.Border;

            Label headerTitle = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 15F, FontStyle.Bold),
                ForeColor = UiTheme.Ink,
                Location = new Point(24, 13),
                Text = "Workspace"
            };
            Label headerSubtitle = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = UiTheme.MutedInk,
                Location = new Point(26, 42),
                Text = "Sales and inventory overview"
            };
            panel2.Controls.Add(headerTitle);
            panel2.Controls.Add(headerSubtitle);
            headerTitle.BringToFront();
            headerSubtitle.BringToFront();

            UiTheme.ApplyDashboard(this, panel1, panel2, mainPanel,
                btnDashboard, btnCustomers, btnProducts, btnPOS,
                btnInvoices, btnSalesReport, btnSettings, btnLogout);
            MinimumSize = new Size(1240, 663);
            StartPosition = FormStartPosition.CenterScreen;
            Resize += (sender, args) => LayoutDashboard();
            LayoutDashboard();
            SetActiveButton(btnDashboard);
        }

        #endregion

        #region Responsive layout

        private void LayoutDashboard()
        {
            const int navigationWidth = 205;
            const int headerHeight = 78;
            panel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            panel1.Width = navigationWidth;
            panel1.Height = ClientSize.Height;
            panel1.Left = 0;
            btnLogout.Top = panel1.Height - btnLogout.Height - 1;

            Button[] navigationButtons = { btnDashboard, btnCustomers, btnProducts, btnPOS, btnInvoices, btnSalesReport, btnSettings, btnLogout };
            foreach (Button button in navigationButtons)
            {
                button.Left = 0;
                button.Width = panel1.ClientSize.Width;
                button.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            }

            panel2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panel2.Left = navigationWidth;
            panel2.Top = 0;
            panel2.Width = Math.Max(1, ClientSize.Width - navigationWidth);
            panel2.Height = headerHeight;

            label4.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            label4.Left = navigationWidth - 1;
            label4.Height = ClientSize.Height;
            label5.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            label5.Left = navigationWidth;
            label5.Width = Math.Max(1, ClientSize.Width - navigationWidth);
            mainPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            mainPanel.Left = navigationWidth + 2;
            mainPanel.Top = headerHeight;
            mainPanel.Width = Math.Max(1, ClientSize.Width - navigationWidth - 2);
            mainPanel.Height = Math.Max(1, ClientSize.Height - headerHeight);
            mainPanel.AutoScroll = true;

            txtUsername.Left = panel2.Width - txtUsername.Width - 16;
            pictureBox2.Left = txtUsername.Left - pictureBox2.Width - 12;
            pictureBox1.Left = pictureBox2.Left - pictureBox1.Width - 12;
        }

        #endregion

        #region Navigation and page management

        private void ShowPage(UserControl page)
        {
            mainPanel.Controls.Clear();
            mainPanel.AutoScroll = true;

            page.Dock = DockStyle.Fill;
            page.MinimumSize = Size.Empty;
            UiTheme.ApplyControlTree(page);

            mainPanel.Controls.Add(page);
            page.BringToFront();
        }

        private void SetActiveButton(Button activeButton)
        {
            foreach (Control control in panel1.Controls)
            {
                if (control is Button btn)
                {
                    if (btn == btnLogout) continue;

                    if (btn == activeButton)
                    {
                        btn.BackColor = UiTheme.Primary;
                        btn.ForeColor = Color.White;
                        btn.FlatAppearance.MouseDownBackColor = UiTheme.PrimaryPressed;
                        btn.FlatAppearance.MouseOverBackColor = UiTheme.PrimaryHover;
                    }
                    else
                    {
                        btn.BackColor = UiTheme.Navigation;
                        btn.ForeColor = Color.FromArgb(218, 235, 228);
                        btn.FlatAppearance.MouseOverBackColor = UiTheme.NavigationHover;
                        btn.FlatAppearance.MouseDownBackColor = UiTheme.PrimaryPressed;
                    }
                }

        #endregion

        #region Event handlers
            }
        }

        private void Dashboard_Load(object sender, EventArgs e)
        {
            ShowPage(new ucDashboard());
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to logout?", "Confirm Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                Application.Restart();
            }
        }

        private void btnCustomers_Click(object sender, EventArgs e)
        {
            ShowPage(new ucCustomers());
            SetActiveButton(btnCustomers);
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void txtUsername_Click(object sender, EventArgs e)
        {

        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void toolStripContainer1_ContentPanel_Load(object sender, EventArgs e)
        {

        }

        private void toolStripContainer2_LeftToolStripPanel_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void panel3_Paint_1(object sender, PaintEventArgs e)
        {

        }

        private void btnDashboard_Click_1(object sender, EventArgs e)
        {
            ShowPage(new ucDashboard());
            SetActiveButton(btnDashboard);
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel7_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void btnSalesReport_Click(object sender, EventArgs e)
        {
            ShowPage(new ucSalesReport());
            SetActiveButton(btnSalesReport);
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {

        }

        private void chart1_Click_1(object sender, EventArgs e)
        {

        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnInvoices_Click(object sender, EventArgs e)
        {
            ShowPage(new ucInvoicesHistory());
            SetActiveButton(btnInvoices);
        }

        private void btnPOS_Click(object sender, EventArgs e)
        {
            ShowPage(new ucPOS(cashierUserId));
            SetActiveButton(btnPOS);
        }

        private void btnProducts_Click(object sender, EventArgs e)
        {
            ShowPage(new ucProducts());
            SetActiveButton(btnProducts);
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            ShowPage(new ucSettings(cashierUserId));
            SetActiveButton(btnSettings);
        }

        #endregion
    }
}
