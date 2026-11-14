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
    public partial class Register : Form
    {
        private PictureBox passwordToggle;
        private PictureBox confirmPasswordToggle;

        #region Initialization

        public Register()
        {
            InitializeComponent();
            AutoScaleMode = AutoScaleMode.Dpi;

            txtFullName.PlaceholderText = "Enter your full name";
            txtUsername.PlaceholderText = "Enter your username";
            txtPassword.PlaceholderText = "Enter your password";
            txtConfirmPassword.PlaceholderText = "Confirm your password";
            txtPassword.UseSystemPasswordChar = true;
            txtConfirmPassword.UseSystemPasswordChar = true; 
            txtPassword.Padding = new Padding(10, 7, 36, 7);
            txtConfirmPassword.Padding = new Padding(10, 7, 36, 7);
            passwordToggle = CreatePasswordToggle(txtPassword);
            confirmPasswordToggle = CreatePasswordToggle(txtConfirmPassword);
            Controls.Add(passwordToggle);
            Controls.Add(confirmPasswordToggle);

            UiTheme.ApplyAuthForm(this, panel1, btnRegister);
            MinimumSize = new Size(1000, 650);
            StartPosition = FormStartPosition.CenterScreen;
            Resize += (sender, args) => LayoutRegister();
            LayoutRegister();
        }

        #endregion

        #region Form events and registration

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Register_Load(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void roundPictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void roundPictureBox2_Click(object sender, EventArgs e)
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

        private void roundedTextBox2_Load(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void roundedTextBox1_Enter(object sender, EventArgs e)
        {

        }

        private void roundedTextBox1_Leave(object sender, EventArgs e)
        {

        }

        private void roundedTextBox1_Load_1(object sender, EventArgs e)
        {

        }

        private void roundedTextBox3_Load(object sender, EventArgs e)
        {

        }

        private void roundedTextBox4_Load(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void flatComboBox1_Load(object sender, EventArgs e)
        {

        }

        private void roundedTextBox5_Load(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabel1.LinkVisited = true;

            Login loginForm = Application.OpenForms.OfType<Login>().FirstOrDefault();
            if (loginForm == null)
            {
                loginForm = new Login();
            }

            loginForm.Show();
            this.Close();
        }

        private void Register_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Return to the existing login form when registration is closed.
            // The login form is hidden while registration is open.
            Login loginForm = Application.OpenForms.OfType<Login>().FirstOrDefault();
            if (loginForm != null && !loginForm.Visible)
            {
                loginForm.Show();
            }
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            string fullName = txtFullName.Text.Trim();
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;
            string confirmPassword = txtConfirmPassword.Text;

            if (fullName == "" || username == "" || password == "" || confirmPassword == "")
            {
                MessageBox.Show("Please fill all the fields.", "Registration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if(password != confirmPassword)
            {
                MessageBox.Show("Passwords do not match. Please try again.", "Registration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if(password.Length < 8)
            {
                MessageBox.Show("Password must be at least 8 characters long.", "Registration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (OleDbConnection con = Database.GetConnection())
                {
                    con.Open();

                    string checkQuery = "SELECT COUNT(*) FROM Users WHERE Username = ?";

                    using (OleDbCommand checkCmd = new OleDbCommand(checkQuery, con))
                    {
                        checkCmd.Parameters.AddWithValue("@Username", username);
                        int userCount = (int)checkCmd.ExecuteScalar();
                        if(userCount > 0)
                        {
                            MessageBox.Show("Username already exists. Please choose another username.", "Registration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        string q = @"INSERT INTO Users (Username, PasswordHash, FullName, UserRole, isActive) VALUES (?, ?, ?, ?, ?)";

                        using (OleDbCommand cmd = new OleDbCommand(q, con))
                        {
                            string passwordHash = PasswordHelper.HashPassword(password);
                            cmd.Parameters.AddWithValue("@Username", username);
                            cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                            cmd.Parameters.AddWithValue("@FullName", fullName);
                            cmd.Parameters.AddWithValue("@UserRole", "User");
                            cmd.Parameters.AddWithValue("@isActive", true);
                            int rowsAffected = cmd.ExecuteNonQuery();
                            if(rowsAffected > 0)
                            {
                                MessageBox.Show("User registered successfully.", "Registration Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                txtFullName.Clear();
                                txtUsername.Clear();
                                txtPassword.Clear();
                                txtConfirmPassword.Clear();

                                Login login = Application.OpenForms.OfType<Login>().FirstOrDefault();
                                if (login == null)
                                {
                                    login = new Login();
                                }

                                login.Show();
                                login.BringToFront();
                                this.FormClosed -= Register_FormClosed;
                                this.Close();
                            } else
                            {
                                MessageBox.Show("Failed to register user. Please try again.", "Registration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while registering the user: " + ex.Message, "Registration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }

        private void LayoutRegister()
        {
            const int brandWidth = 585;
            panel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            panel1.Width = Math.Min(brandWidth, Math.Max(420, ClientSize.Width / 2));
            panel1.Height = ClientSize.Height;
            panel1.Left = 0;

            int contentLeft = panel1.Right;
            int contentWidth = ClientSize.Width - contentLeft;
            int left = contentLeft + Math.Max(24, (contentWidth - txtUsername.Width) / 2);
            txtUsername.Left = left;
            txtFullName.Left = left;
            txtPassword.Left = left;
            txtConfirmPassword.Left = left;
            btnRegister.Left = left;
            label5.Left = left;
            label6.Left = left;
            label8.Left = left;
            label9.Left = left;
            label3.Left = left - 8;
            label4.Left = left;
            label11.Left = left + (txtUsername.Width - label11.Width - linkLabel1.Width - 8) / 2;
            linkLabel1.Left = label11.Right + 8;
            PositionPasswordToggles();
        }

        private PictureBox CreatePasswordToggle(RoundedTextBox passwordBox)
        {
            PictureBox toggle = new PictureBox
            {
                Size = new Size(28, 28),
                SizeMode = PictureBoxSizeMode.CenterImage,
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

        private void PositionPasswordToggles()
        {
            if (passwordToggle != null)
            {
                passwordToggle.Left = txtPassword.Right - passwordToggle.Width - 5;
                passwordToggle.Top = txtPassword.Top + (txtPassword.Height - passwordToggle.Height) / 2;
                passwordToggle.BringToFront();
            }

            if (confirmPasswordToggle != null)
            {
                confirmPasswordToggle.Left = txtConfirmPassword.Right - confirmPasswordToggle.Width - 5;
                confirmPasswordToggle.Top = txtConfirmPassword.Top + (txtConfirmPassword.Height - confirmPasswordToggle.Height) / 2;
                confirmPasswordToggle.BringToFront();
            }
        }

        #endregion
    }
}
