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
using System.IO;

namespace Activity1_FinalTerm
{
    public partial class Login : Form
    {
        private PictureBox passwordToggle;

        #region Initialization

        public Login()
        {
            InitializeComponent();
            AutoScaleMode = AutoScaleMode.Dpi;

            txtUsername.PlaceholderText = "Enter your username";
            txtPassword.PlaceholderText = "Enter your password";
            txtPassword.UseSystemPasswordChar = true;
            txtPassword.Padding = new Padding(10, 7, 36, 7);
            passwordToggle = CreatePasswordToggle(txtPassword);
            Controls.Add(passwordToggle);

            UiTheme.ApplyAuthForm(this, panel1, btnLogin);
            ClientSize = new Size(1102, 663);
            MinimumSize = new Size(1102, 663);
            StartPosition = FormStartPosition.CenterScreen;
            Resize += (sender, args) => LayoutLogin();
            LayoutLogin();

            string rememberedUsername = LoadRememberedUsername();
            if (!string.IsNullOrEmpty(rememberedUsername))
            {
                txtUsername.Text = rememberedUsername;
                checkBox1.Checked = true;
            }
        }

        #endregion

        #region Form events, authentication, and password recovery

        private void Login_Load(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void roundedTextBox1_Load(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void roundedTextBox2_Load(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void roundedTextBox3_Load(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void flatComboBox1_Load(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabel1.LinkVisited = true;

            Register registerForm = new Register();
            this.Hide();
            registerForm.Show();
        }

        private void Login_FormClosed(object sender, FormClosedEventArgs e)
        {
            if(Application.OpenForms.Count == 0)
            {
                Application.Exit();
            }
        }

        private void roundPictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if(username == "" || password == "")
            {
                MessageBox.Show("Please enter both username and password.", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

                            SaveRememberedUsername(checkBox1.Checked ? username : null);

            try
            {
                using(OleDbConnection con = Database.GetConnection())
                {
                    con.Open();

                    string q = "SELECT * FROM Users WHERE Username = ?";

                    using (OleDbCommand cmd = new OleDbCommand(q, con))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);

                        using (OleDbDataReader reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                MessageBox.Show("Invalid username or password. Please try again.", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            string storedHash = reader["PasswordHash"].ToString();
                            string fullName = reader["FullName"].ToString();
                            string role = reader["UserRole"].ToString();
                            bool isActive = reader["IsActive"] == DBNull.Value || Convert.ToBoolean(reader["IsActive"]);
                            int userId = Convert.ToInt32(reader["UserID"]);

                            if(!PasswordHelper.VerifyPassword(password, storedHash))
                            {
                                MessageBox.Show("Invalid username or password. Please try again.", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                            
                            if(!isActive)
                            {
                                MessageBox.Show("Your account is inactive. Please contact the administrator.", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            MessageBox.Show($"Welcome {fullName}!", "Login Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            reader.Close();

                            Dashboard dashboard = new Dashboard(fullName, userId);
                            dashboard.FormClosed += (s, args) => this.Close();
                            this.Hide();
                            dashboard.Show();
                        }
                    }

        #endregion
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while signing in: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void panel1_Paint_1(object sender, PaintEventArgs e)
        {

        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void LayoutLogin()
        {
            if (WindowState != FormWindowState.Maximized)
            {
                RestoreOriginalLayout();
                return;
            }

            const int brandWidth = 585;
            panel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            panel1.Width = Math.Min(brandWidth, Math.Max(420, ClientSize.Width / 2));
            panel1.Height = ClientSize.Height;
            panel1.Left = ClientSize.Width - panel1.Width;

            int contentWidth = panel1.Left;
            int left = Math.Max(24, (contentWidth - txtUsername.Width) / 2);
            txtUsername.Left = left;
            txtPassword.Left = left;
            btnLogin.Left = left;
            label5.Left = left;
            label8.Left = left;
            checkBox1.Left = left;
            label3.Left = left - 8;
            label4.Left = left;
            linkLabel2.Left = left + txtUsername.Width - linkLabel2.Width;
            label11.Left = left + (txtUsername.Width - label11.Width - linkLabel1.Width - 8) / 2;
            linkLabel1.Left = label11.Right + 8;
            PositionPasswordToggle();
        }

        private void RestoreOriginalLayout()
        {
            panel1.Location = new Point(518, -4);
            panel1.Size = new Size(585, 671);
            panel1.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            label3.Location = new Point(46, 125);
            label4.Location = new Point(50, 172);
            label5.Location = new Point(51, 217);
            txtUsername.Location = new Point(54, 242);
            label8.Location = new Point(51, 275);
            txtPassword.Location = new Point(54, 300);
            checkBox1.Location = new Point(54, 336);
            linkLabel2.Location = new Point(344, 333);
            btnLogin.Location = new Point(53, 372);
            label11.Location = new Point(126, 425);
            linkLabel1.Location = new Point(283, 425);
            PositionPasswordToggle();
        }

        private PictureBox CreatePasswordToggle(RoundedTextBox passwordBox)
        {
            PictureBox toggle = new PictureBox
            {
                Size = new Size(22, 22),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White,
                Cursor = Cursors.Hand,
                Image = Properties.Resources.eye,
                AccessibleName = "Show password",
                TabStop = false
            };
            toggle.Click += (sender, args) =>
            {
                passwordBox.UseSystemPasswordChar = !passwordBox.UseSystemPasswordChar;
                toggle.Image = passwordBox.UseSystemPasswordChar ? Properties.Resources.eye : Properties.Resources.eyeoff;
                toggle.AccessibleName = passwordBox.UseSystemPasswordChar ? "Show password" : "Hide password";
            };
            return toggle;
        }

        private PictureBox CreatePasswordToggle(TextBox passwordBox)
        {
            PictureBox toggle = new PictureBox
            {
                Size = new Size(22, 22),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White,
                Cursor = Cursors.Hand,
                Image = Properties.Resources.eye,
                AccessibleName = "Show password",
                TabStop = false
            };
            toggle.Click += (sender, args) =>
            {
                passwordBox.UseSystemPasswordChar = !passwordBox.UseSystemPasswordChar;
                toggle.Image = passwordBox.UseSystemPasswordChar ? Properties.Resources.eye : Properties.Resources.eyeoff;
                toggle.AccessibleName = passwordBox.UseSystemPasswordChar ? "Show password" : "Hide password";
            };
            return toggle;
        }

        private void PositionPasswordToggle()
        {
            if (passwordToggle == null) return;
            passwordToggle.Left = txtPassword.Right - passwordToggle.Width - 8;
            passwordToggle.Top = txtPassword.Top + (txtPassword.Height - passwordToggle.Height) / 2;
            passwordToggle.BringToFront();
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            using (Form dialog = new Form
            {
                Text = "Reset Password",
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MinimizeBox = false,
                MaximizeBox = false,
                ShowInTaskbar = false,
                ClientSize = new Size(360, 250),
                BackColor = UiTheme.Surface
            })
            {
                Label usernameLabel = new Label { Text = "Username", AutoSize = true, Location = new Point(24, 20) };
                TextBox usernameBox = new TextBox { Location = new Point(24, 43), Width = 312 };
                Label passwordLabel = new Label { Text = "New password (at least 8 characters)", AutoSize = true, Location = new Point(24, 78) };
                TextBox passwordBox = new TextBox { Location = new Point(24, 101), Width = 312, UseSystemPasswordChar = true, Padding = new Padding(3, 0, 30, 0) };
                Label confirmLabel = new Label { Text = "Confirm new password", AutoSize = true, Location = new Point(24, 136) };
                TextBox confirmBox = new TextBox { Location = new Point(24, 159), Width = 312, UseSystemPasswordChar = true, Padding = new Padding(3, 0, 30, 0) };
                Button resetButton = new Button { Text = "Reset Password", DialogResult = DialogResult.None, Location = new Point(204, 198), Size = new Size(132, 32) };
                PictureBox passwordToggle = CreatePasswordToggle(passwordBox);
                PictureBox confirmPasswordToggle = CreatePasswordToggle(confirmBox);
                passwordToggle.Size = new Size(17, 17);
                confirmPasswordToggle.Size = new Size(17, 17);
                passwordToggle.Location = new Point(passwordBox.Right - passwordToggle.Width - 5, passwordBox.Top + 1);
                confirmPasswordToggle.Location = new Point(confirmBox.Right - confirmPasswordToggle.Width - 5, confirmBox.Top + 1);

                UiTheme.ApplyControlTree(dialog);
                resetButton.BackColor = UiTheme.Primary;
                resetButton.ForeColor = Color.White;
                resetButton.FlatStyle = FlatStyle.Flat;
                resetButton.FlatAppearance.BorderSize = 0;
                resetButton.MakeRounded(8);

                resetButton.Click += (s, args) =>
                {
                    string username = usernameBox.Text.Trim();
                    string password = passwordBox.Text;

                    if (string.IsNullOrWhiteSpace(username) || password.Length < 8 || password != confirmBox.Text)
                    {
                        MessageBox.Show(dialog, "Enter a username, matching passwords, and a password of at least 8 characters.", "Reset Password", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    try
                    {
                        using (OleDbConnection connection = Database.GetConnection())
                        using (OleDbCommand command = new OleDbCommand("UPDATE Users SET PasswordHash = ? WHERE Username = ? AND IsActive = True", connection))
                        {
                            connection.Open();
                            command.Parameters.AddWithValue("@PasswordHash", PasswordHelper.HashPassword(password));
                            command.Parameters.AddWithValue("@Username", username);
                            if (command.ExecuteNonQuery() != 1)
                            {
                                MessageBox.Show(dialog, "The username was not found or the account is inactive.", "Reset Password", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                        }

                        MessageBox.Show(dialog, "Password reset successfully. You can now sign in.", "Reset Password", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        dialog.DialogResult = DialogResult.OK;
                        dialog.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(dialog, "Unable to reset the password: " + ex.Message, "Reset Password", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                dialog.Controls.AddRange(new Control[] { usernameLabel, usernameBox, passwordLabel, passwordBox, confirmLabel, confirmBox, resetButton, passwordToggle, confirmPasswordToggle });
                passwordToggle.BringToFront();
                confirmPasswordToggle.BringToFront();
                dialog.AcceptButton = resetButton;
                dialog.ShowDialog(this);
            }
        }

        private static string RememberedUsernamePath
        {
            get { return Path.Combine(Application.LocalUserAppDataPath, "remembered-user.txt"); }
        }

        private static string LoadRememberedUsername()
        {
            try
            {
                return File.Exists(RememberedUsernamePath) ? File.ReadAllText(RememberedUsernamePath).Trim() : string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static void SaveRememberedUsername(string username)
        {
            try
            {
                Directory.CreateDirectory(Application.LocalUserAppDataPath);
                if (string.IsNullOrEmpty(username))
                {
                    if (File.Exists(RememberedUsernamePath)) File.Delete(RememberedUsernamePath);
                }
                else
                {
                    File.WriteAllText(RememberedUsernamePath, username);
                }
            }
            catch
            {
                // Remembering the username is optional and must not block login.
            }
        }
    }
}
