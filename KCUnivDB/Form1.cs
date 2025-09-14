using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KCUnivDB
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
            lockoutTime = DateTime.Now;
        }

        private int loginAttempts = 0;
        private const int MAX_ATTEMPTS = 3;
        private DateTime lockoutTime;


        string connectionString = @"Data Source = canasa\SQLEXPRESS;
        Initial catalog = KCUnivDB; Integrated Security = true";
        private static string HashPassword(string plainPassword)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(plainPassword);
                byte[] hash = sha256.ComputeHash(bytes);
                StringBuilder builder = new StringBuilder();
                foreach (byte b in hash)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }


        private void btnEyesOn_Click(object sender, EventArgs e)
        {
            if (txtPassword.PasswordChar == '•')
            {
                btnEyesOff.BringToFront();
                txtPassword.PasswordChar = '\0';
            }
        }

        private void btnEyesOff_Click_1(object sender, EventArgs e)
        {
            if (txtPassword.PasswordChar == '\0')
            {
                btnEyesOn.BringToFront();
                txtPassword.PasswordChar = '•';
            }
        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnLogins_Click(object sender, EventArgs e)
        {
            if (DateTime.Now < lockoutTime)
            {
                TimeSpan remainingTime = lockoutTime - DateTime.Now;
                MessageBox.Show($"Maximum login attempts exceeded. Please try again after {remainingTime.Minutes} minutes and {remainingTime.Seconds} seconds.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            errorProvider1.Clear();
            bool isValid = true;

            string username = txtUsername.Text.Trim();
            string plainPassword = txtPassword.Text.Trim();

            if (string.IsNullOrWhiteSpace(username))
            {
                errorProvider1.SetError(txtUsername, "Username is required.");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(plainPassword))
            {
                errorProvider1.SetError(txtPassword, "Password is required.");
                isValid = false;
            }

            if (isValid) 
            {
                string hashedPassword = HashPassword(plainPassword);

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("Login_SP", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", hashedPassword);


                        connection.Open();
                        SqlDataReader reader = cmd.ExecuteReader();

                        if (reader.Read())
                        {
                            loginAttempts = 0;
                            lockoutTime = DateTime.Now;

                           string status = reader["Status"].ToString();

                            if (status != "Active")
                            {
                                MessageBox.Show("Your account is pending approval. Please wait for the admin to approve your account.");
                                this.Show();
                                return;
                            }

                            int roleId = Convert.ToInt32(reader["RoleID"]);

                        // Admin
                            if (roleId == 1) 
                            {
                                MessageBox.Show("Login Successful! Welcome, Admin.", "KCUnivDB",MessageBoxButtons.OK, MessageBoxIcon.Information);
                                this.Hide();
                                AdminDashboard adminDash = new AdminDashboard();
                                adminDash.Show();
                            }
                        // Instructor
                            else if (roleId == 2) 
                            {
                                MessageBox.Show("Login Successful! Welcome, Instructor.", "KCUnivDB", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                this.Hide();
                                InstructorDashboard teacherDash = new InstructorDashboard();
                                teacherDash.Show();
                            }
                        // Student
                            else if (roleId == 3)
                            {
                                MessageBox.Show("Login Successful!", "KCUnivDB", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                this.Hide();
                                StudentDashboard studentDash = new StudentDashboard();
                                studentDash.Show();
                            }

                            else
                            {
                                MessageBox.Show("Unknown user role. Please contact support.", "KCUnivDB", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                this.Show();
                            }
                        }
                        else
                        {
                            loginAttempts++;

                            if (loginAttempts >= MAX_ATTEMPTS)
                            {
                                lockoutTime = DateTime.Now.AddMinutes(3);
                                MessageBox.Show($"Maximum login attempts exceeded. You are locked out for 3 minutes.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            else
                            {
                                MessageBox.Show($"Login failed. Invalid username or password. You have {MAX_ATTEMPTS - loginAttempts} attempts remaining.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }


                }
            }
            
            

        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            Registration register = new Registration();
            register.Show();
            this.Hide();
        }

        private void lblForgotPass_Click(object sender, EventArgs e)
        {
            ForgotEmail forgotPassForm = new ForgotEmail();
            forgotPassForm.Show();
            this.Hide();
        }
    }
}
