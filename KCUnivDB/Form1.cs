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
            
        }

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
            // Clear any previous errors.
            errorProvider1.Clear();
            bool isValid = true;

            // Trim any leading or trailing spaces from the textboxes.
            string username = txtUsername.Text.Trim();
            string plainPassword = txtPassword.Text.Trim();

            // Check if username field is not empty.
            if (string.IsNullOrWhiteSpace(username))
            {
                errorProvider1.SetError(txtUsername, "Username is required.");
                isValid = false;
            }

            // Check if password field is not empty.
            if (string.IsNullOrWhiteSpace(plainPassword))
            {
                errorProvider1.SetError(txtPassword, "Password is required.");
                isValid = false;
            }

            if (isValid) 
            {
                // Hash the plain text password.
                string hashedPassword = HashPassword(plainPassword);

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("Login_SP", connection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Pass the trimmed username and hashed password to the stored procedure.
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", hashedPassword);


                        connection.Open();
                        SqlDataReader reader = cmd.ExecuteReader();

                        if (reader.Read())
                        {
                            string status = reader["Status"].ToString();

                            if (status != "Active")
                            {
                                MessageBox.Show("Your account is pending approval. Please wait for the admin to approve your account.");
                                this.Show();
                                return;
                            }

                            int roleId = Convert.ToInt32(reader["RoleID"]);

                            if (roleId == 1) // Admin
                            {
                                MessageBox.Show("Login Successful! Welcome, Admin.");
                                this.Hide();
                                AdminDashboard adminDash = new AdminDashboard();
                                adminDash.Show();
                            }
                            else if (roleId == 2) // Instructor
                            {
                                MessageBox.Show("Login Successful! Welcome, Instructor.");
                                this.Hide();
                                InstructorDashboard teacherDash = new InstructorDashboard();
                                teacherDash.Show();
                            }
                            else if (roleId == 3) // Student
                            {
                                MessageBox.Show("Login Successful! Welcome, Student.");
                                this.Hide();
                                StudentDashboard studentDash = new StudentDashboard();
                                studentDash.Show();
                            }

                            else
                            {
                                MessageBox.Show("Unknown user role. Please contact support.");
                                this.Show();
                            }
                        }
                        else
                        {
                            MessageBox.Show("Login failed. Invalid username or password.");
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
