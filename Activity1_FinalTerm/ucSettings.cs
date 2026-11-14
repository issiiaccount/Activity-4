using System;
using System.Data.OleDb;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace Activity1_FinalTerm
{
    public partial class ucSettings : UserControl
    {
        private readonly int userId;
        private TextBox usernameBox;
        private TextBox fullNameBox;
        private TextBox currentPasswordBox;
        private TextBox newPasswordBox;
        private TextBox confirmPasswordBox;
        private ComboBox currencyBox;
        private NumericUpDown taxRateBox;

        public ucSettings() : this(0) { }

        public ucSettings(int userId)
        {
            this.userId = userId;
            InitializeComponent();
            BuildSettingsPage();
        }

        private void BuildSettingsPage()
        {
            Controls.Clear();
            BackColor = Color.FromArgb(245, 247, 251);
            AutoScroll = true;
            AutoScrollMinSize = new Size(0, 720);
            Label title = new Label { AutoSize = true, Font = new Font("Segoe UI", 20.25F, FontStyle.Bold), Location = new Point(14, 12), Text = "Settings" };
            Controls.Add(title);
            int width = Math.Max(520, ClientSize.Width - 42);

            GroupBox profileGroup = CreateGroup("User Profile", 21, 70, width, 150);
            usernameBox = AddTextField(profileGroup, "Username", 18, 30, 250);
            fullNameBox = AddTextField(profileGroup, "Full name", 300, 30, 250);
            Button saveProfile = CreateButton("Save Profile", 18, 92, 130);
            saveProfile.Click += SaveProfile_Click;
            profileGroup.Controls.Add(saveProfile);
            Controls.Add(profileGroup);

            GroupBox passwordGroup = CreateGroup("Change Password", 21, 235, width, 180);
            currentPasswordBox = AddPasswordField(passwordGroup, "Current password", 18, 30, 250);
            newPasswordBox = AddPasswordField(passwordGroup, "New password", 300, 30, 250);
            confirmPasswordBox = AddPasswordField(passwordGroup, "Confirm password", 18, 98, 250);
            Button changePassword = CreateButton("Change Password", 300, 98, 145);
            changePassword.Click += ChangePassword_Click;
            passwordGroup.Controls.Add(changePassword);
            Controls.Add(passwordGroup);

            GroupBox financeGroup = CreateGroup("Currency and Tax", 21, 430, width, 145);
            currencyBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(18, 50), Size = new Size(160, 28) };
            currencyBox.Items.AddRange(new object[] { "PHP", "USD", "EUR", "JPY", "GBP" });
            currencyBox.SelectedItem = LocalSettings.Currency;
            if (currencyBox.SelectedIndex < 0) currencyBox.SelectedIndex = 0;
            financeGroup.Controls.Add(CreateLabel("Currency", 18, 28));
            financeGroup.Controls.Add(currencyBox);
            taxRateBox = new NumericUpDown { DecimalPlaces = 2, Increment = 0.5m, Minimum = 0, Maximum = 100, Location = new Point(220, 50), Size = new Size(120, 28), Value = LocalSettings.TaxRate };
            financeGroup.Controls.Add(CreateLabel("Tax rate (%)", 220, 28));
            financeGroup.Controls.Add(taxRateBox);
            Button saveFinance = CreateButton("Save Finance Settings", 370, 48, 170);
            saveFinance.Click += SaveFinance_Click;
            financeGroup.Controls.Add(saveFinance);
            Controls.Add(financeGroup);

            GroupBox databaseGroup = CreateGroup("Database Backup and Restore", 21, 590, width, 105);
            Button backup = CreateButton("Backup Database", 18, 40, 145);
            backup.Click += Backup_Click;
            Button restore = CreateButton("Restore Database", 180, 40, 145);
            restore.Click += Restore_Click;
            databaseGroup.Controls.Add(backup);
            databaseGroup.Controls.Add(restore);
            Controls.Add(databaseGroup);
            LoadProfile();
            Resize += (sender, args) => ResizeSettingsGroups(profileGroup, passwordGroup, financeGroup, databaseGroup);
            ResizeSettingsGroups(profileGroup, passwordGroup, financeGroup, databaseGroup);
            PerformLayout();
        }

        private void ResizeSettingsGroups(params GroupBox[] groups)
        {
            int width = Math.Max(520, ClientSize.Width - 42);
            foreach (GroupBox group in groups) group.Width = width;
        }

        private GroupBox CreateGroup(string text, int left, int top, int width, int height)
        {
            return new GroupBox { Text = text, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Location = new Point(left, top), Size = new Size(width, height), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
        }

        private Label CreateLabel(string text, int left, int top)
        {
            return new Label { AutoSize = true, Font = new Font("Segoe UI", 9F), Location = new Point(left, top), Text = text };
        }

        private TextBox AddTextField(Control parent, string label, int left, int top, int width)
        {
            parent.Controls.Add(CreateLabel(label, left, top));
            TextBox box = new TextBox { Location = new Point(left, top + 20), Size = new Size(width, 26) };
            parent.Controls.Add(box);
            return box;
        }

        private TextBox AddPasswordField(Control parent, string label, int left, int top, int width)
        {
            TextBox box = AddTextField(parent, label, left, top, width);
            box.UseSystemPasswordChar = true;
            return box;
        }

        private Button CreateButton(string text, int left, int top, int width)
        {
            Button button = new Button { BackColor = Color.FromArgb(40, 161, 116), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8.75F, FontStyle.Bold), ForeColor = Color.White, Location = new Point(left, top), Size = new Size(width, 30), Text = text, UseVisualStyleBackColor = false };
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(49, 177, 130);
            return button;
        }

        private void LoadProfile()
        {
            if (userId <= 0) return;
            try
            {
                using (OleDbConnection connection = Database.GetConnection())
                using (OleDbCommand command = new OleDbCommand("SELECT Username, FullName FROM Users WHERE UserID = ?", connection))
                {
                    command.Parameters.AddWithValue("@UserID", userId);
                    connection.Open();
                    using (OleDbDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read()) { usernameBox.Text = Convert.ToString(reader["Username"]); fullNameBox.Text = Convert.ToString(reader["FullName"]); }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Unable to load profile: " + ex.Message, "Settings", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void SaveProfile_Click(object sender, EventArgs e)
        {
            if (userId <= 0 || string.IsNullOrWhiteSpace(usernameBox.Text) || string.IsNullOrWhiteSpace(fullNameBox.Text)) { MessageBox.Show("Enter a username and full name.", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            try
            {
                using (OleDbConnection connection = Database.GetConnection())
                using (OleDbCommand command = new OleDbCommand("UPDATE Users SET Username = ?, FullName = ? WHERE UserID = ?", connection))
                {
                    command.Parameters.AddWithValue("@Username", usernameBox.Text.Trim()); command.Parameters.AddWithValue("@FullName", fullNameBox.Text.Trim()); command.Parameters.AddWithValue("@UserID", userId); connection.Open(); command.ExecuteNonQuery();
                }
                MessageBox.Show("Profile updated successfully.", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show("Unable to update profile: " + ex.Message, "Settings", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void ChangePassword_Click(object sender, EventArgs e)
        {
            if (userId <= 0 || string.IsNullOrEmpty(currentPasswordBox.Text) || string.IsNullOrEmpty(newPasswordBox.Text)) { MessageBox.Show("Enter the current and new passwords.", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (newPasswordBox.Text != confirmPasswordBox.Text) { MessageBox.Show("The new passwords do not match.", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            try
            {
                string storedHash;
                using (OleDbConnection connection = Database.GetConnection())
                using (OleDbCommand command = new OleDbCommand("SELECT PasswordHash FROM Users WHERE UserID = ?", connection))
                { command.Parameters.AddWithValue("@UserID", userId); connection.Open(); storedHash = Convert.ToString(command.ExecuteScalar()); }
                if (!PasswordHelper.VerifyPassword(currentPasswordBox.Text, storedHash)) { MessageBox.Show("The current password is incorrect.", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                using (OleDbConnection connection = Database.GetConnection())
                using (OleDbCommand command = new OleDbCommand("UPDATE Users SET PasswordHash = ? WHERE UserID = ?", connection))
                { command.Parameters.AddWithValue("@PasswordHash", PasswordHelper.HashPassword(newPasswordBox.Text)); command.Parameters.AddWithValue("@UserID", userId); connection.Open(); command.ExecuteNonQuery(); }
                currentPasswordBox.Clear(); newPasswordBox.Clear(); confirmPasswordBox.Clear(); MessageBox.Show("Password changed successfully.", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show("Unable to change password: " + ex.Message, "Settings", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void SaveFinance_Click(object sender, EventArgs e)
        {
            LocalSettings.Save(currencyBox.SelectedItem.ToString(), taxRateBox.Value);
            MessageBox.Show("Currency and tax settings saved.", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Backup_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog { Filter = "Access Database (*.accdb)|*.accdb", FileName = "InventorySys_Backup.accdb" })
            {
                if (dialog.ShowDialog() != DialogResult.OK) return;
                try { File.Copy(Database.DB_PATH, dialog.FileName, true); MessageBox.Show("Database backup created.", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information); }
                catch (Exception ex) { MessageBox.Show("Unable to create backup: " + ex.Message, "Settings", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void Restore_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog { Filter = "Access Database (*.accdb)|*.accdb" })
            {
                if (dialog.ShowDialog() != DialogResult.OK) return;
                if (MessageBox.Show("Restore this database? Current data will be replaced.", "Confirm Restore", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                try { File.Copy(dialog.FileName, Database.DB_PATH, true); MessageBox.Show("Database restored. Restart the application to reload the data.", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information); }
                catch (Exception ex) { MessageBox.Show("Unable to restore database: " + ex.Message, "Settings", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }
    }
}
