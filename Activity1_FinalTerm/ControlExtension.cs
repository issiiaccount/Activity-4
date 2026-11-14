using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public static class ControlExtensions
{
    #region Control extensions

    /// <summary>
    /// Makes any control (like a Button) rounded by changing its Region.
    /// </summary>
    public static void MakeRounded(this Control control, int radius)
    {
        if (control == null) return;

        Rectangle rect = new Rectangle(0, 0, control.Width, control.Height);
        float size = radius * 2F;

        // Apply the rounded region immediately
        using (GraphicsPath path = new GraphicsPath())
        {
            path.StartFigure();
            path.AddArc(rect.X, rect.Y, size, size, 180, 90);
            path.AddArc(rect.Right - size, rect.Y, size, size, 270, 90);
            path.AddArc(rect.Right - size, rect.Bottom - size, size, size, 0, 90);
            path.AddArc(rect.X, rect.Bottom - size, size, size, 90, 90);
            path.CloseFigure();

            control.Region = new Region(path);
        }

        // Hook up the resize event so it doesn't break if the button changes size later
        control.Resize -= Control_Resize;
        control.Resize += Control_Resize;
    }

    private static void Control_Resize(object sender, EventArgs e)
    {
        Control c = (Control)sender;
        // Adjust this number if you want a sharper or softer curve during resize
        int radius = 10;
        float size = radius * 2F;

        Rectangle r = new Rectangle(0, 0, c.Width, c.Height);
        using (GraphicsPath p = new GraphicsPath())
        {
            p.StartFigure();
            p.AddArc(r.X, r.Y, size, size, 180, 90);
            p.AddArc(r.Right - size, r.Y, size, size, 270, 90);
            p.AddArc(r.Right - size, r.Bottom - size, size, size, 0, 90);
            p.AddArc(r.X, r.Bottom - size, size, size, 90, 90);
            p.CloseFigure();
            c.Region = new Region(p);
        }
    }

    #endregion
}

namespace Activity1_FinalTerm
{
    internal static class UiTheme
    {
        #region Theme palette

        internal static readonly Color Ink = Color.FromArgb(35, 52, 47);
        internal static readonly Color MutedInk = Color.FromArgb(105, 121, 115);
        internal static readonly Color Surface = Color.FromArgb(247, 250, 249);
        internal static readonly Color Card = Color.White;
        internal static readonly Color Primary = Color.FromArgb(28, 133, 95);
        internal static readonly Color PrimaryHover = Color.FromArgb(39, 157, 113);
        internal static readonly Color PrimaryPressed = Color.FromArgb(20, 103, 74);
        internal static readonly Color Navigation = Color.FromArgb(23, 54, 47);
        internal static readonly Color NavigationHover = Color.FromArgb(38, 84, 72);
        internal static readonly Color Border = Color.FromArgb(218, 228, 223);
        internal static readonly Color Danger = Color.FromArgb(190, 74, 74);

        #endregion

        #region Public styling operations

        internal static void ApplyAuthForm(Form form, Panel brandPanel, Button actionButton)
        {
            form.BackColor = Surface;
            form.Font = new Font("Segoe UI", 10F);
            brandPanel.BackColor = Navigation;
            StyleActionButton(actionButton, 11F);

            foreach (Control control in form.Controls)
            {
                ApplyAuthControl(control, brandPanel);
            }
        }

        #endregion

        #region Responsive layout helpers

        internal static void ApplyDashboard(Form form, Panel navigation, Panel header, Panel content, params Button[] navigationButtons)
        {
            form.BackColor = Surface;
            form.Font = new Font("Segoe UI", 10F);
            navigation.BackColor = Navigation;
            navigation.BorderStyle = BorderStyle.None;
            header.BackColor = Card;
            header.BorderStyle = BorderStyle.None;
            content.BackColor = Surface;

            foreach (Button button in navigationButtons)
            {
                button.BackColor = Navigation;
                button.ForeColor = Color.FromArgb(218, 235, 228);
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderSize = 0;
                button.FlatAppearance.MouseOverBackColor = NavigationHover;
                button.FlatAppearance.MouseDownBackColor = PrimaryPressed;
                button.Cursor = Cursors.Hand;
                button.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
                button.TextAlign = ContentAlignment.MiddleLeft;
                button.Padding = new Padding(22, 0, 0, 0);
            }

        #endregion

        #region Private styling helpers

            navigationButtons[0].BackColor = Primary;
            navigationButtons[0].ForeColor = Color.White;
            navigationButtons[navigationButtons.Length - 1].BackColor = Danger;
            navigationButtons[navigationButtons.Length - 1].ForeColor = Color.White;
        }

        internal static void ApplyControlTree(Control root)
        {
            root.Resize -= ResponsivePage_Resize;
            root.Resize += ResponsivePage_Resize;
            ApplyResponsiveAnchors(root);
            ResizePageSurfaces(root);

            foreach (Control control in root.Controls)
            {
                if (control is DataGridView grid)
                {
                    grid.BackgroundColor = Card;
                    grid.BorderStyle = BorderStyle.None;
                    grid.EnableHeadersVisualStyles = false;
                    grid.ColumnHeadersDefaultCellStyle.BackColor = Navigation;
                    grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                    grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
                    grid.DefaultCellStyle.ForeColor = Ink;
                    grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(218, 240, 230);
                    grid.DefaultCellStyle.SelectionForeColor = Ink;
                    grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(246, 249, 247);
                    grid.RowHeadersVisible = false;
                    grid.RowTemplate.Height = 34;
                }
                else if (control is Button button)
                {
                    ButtonStyle.Apply(button);
                }
                else if (control is RoundedTextBox textBox)
                {
                    textBox.BackColor = Card;
                    textBox.BorderActiveColor = Primary;
                    textBox.BorderInactiveColor = Border;
                    textBox.PlaceholderColor = MutedInk;
                }
                else if (control is FlatComboBox comboBox)
                {
                    comboBox.BackColor = Card;
                    comboBox.ForeColor = Ink;
                    comboBox.BorderColor = Border;
                    comboBox.BorderSize = 1;
                }

                if (control.HasChildren) ApplyControlTree(control);
            }
        }

        private static void ResponsivePage_Resize(object sender, EventArgs e)
        {
            ResizePageSurfaces((Control)sender);
        }

        private static void ResizePageSurfaces(Control root)
        {
            if (root is ucPOS || root is ucInvoicesHistory)
            {
                return;
            }

            ResizePageSurfacesRecursive(root);
        }

        private static void ApplyResponsiveAnchors(Control parent)
        {
            bool isDashboardPage = string.Equals(parent.GetType().Name, "ucDashboard", StringComparison.Ordinal);

            foreach (Control control in parent.Controls)
            {
                if (!isDashboardPage && control.Dock == DockStyle.None)
                {
                    bool rightAligned = control.Left > parent.ClientSize.Width / 3 &&
                                        control.Right >= parent.ClientSize.Width - 230;
                    bool bottomAligned = control.Top > parent.ClientSize.Height / 2 &&
                                         control.Bottom >= parent.ClientSize.Height - 100;

                    AnchorStyles anchors = AnchorStyles.Top | AnchorStyles.Left;
                    if (rightAligned) anchors |= AnchorStyles.Right;
                    if (bottomAligned) anchors |= AnchorStyles.Bottom;
                    control.Anchor = anchors;
                }

                if (control.HasChildren && !(control is RoundedTextBox) && !(control is FlatComboBox))
                {
                    ApplyResponsiveAnchors(control);
                }
            }
        }

        private static void ResizePageSurfacesRecursive(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                if (control is DataGridView || control is System.Windows.Forms.DataVisualization.Charting.Chart)
                {
                    int rightMargin = 20;
                    control.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                    control.Width = Math.Max(240, parent.ClientSize.Width - control.Left - rightMargin);
                }

                if (control.HasChildren)
                {
                    ResizePageSurfacesRecursive(control);
                }
            }
        }

        private static void ApplyAuthControl(Control control, Control brandPanel)
        {
            if (control is Label label)
                label.ForeColor = label.Parent == brandPanel ? Color.FromArgb(225, 241, 234) : Ink;
            else if (control is LinkLabel link)
            {
                link.LinkColor = Primary;
                link.ActiveLinkColor = PrimaryPressed;
                link.VisitedLinkColor = Primary;
            }
            else if (control is RoundedTextBox textBox)
            {
                textBox.BackColor = Card;
                textBox.BorderActiveColor = Primary;
                textBox.BorderInactiveColor = Border;
                textBox.PlaceholderColor = MutedInk;
            }

            if (control.HasChildren)
                foreach (Control child in control.Controls) ApplyAuthControl(child, brandPanel);
        }

        private static void StyleActionButton(Button button, float size)
        {
            button.BackColor = Primary;
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = PrimaryHover;
            button.FlatAppearance.MouseDownBackColor = PrimaryPressed;
            button.Cursor = Cursors.Hand;
            button.Font = new Font("Segoe UI Semibold", size, FontStyle.Bold);
            button.MakeRounded(8);
        }

        #endregion
    }
}
