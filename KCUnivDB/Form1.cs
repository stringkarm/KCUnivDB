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
        private void btnLogin_Click(object sender, EventArgs e)
        {
            // Validate that username and password fields are not empty.
            if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Please enter both a username and password.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("Login_SP", connection);
                cmd.CommandType = CommandType.StoredProcedure;

                // Pass the provided credentials to the stored procedure.
                cmd.Parameters.AddWithValue("@Username", txtUsername.Text);
                cmd.Parameters.AddWithValue("@Password", txtPassword.Text);

                try
                {
                    connection.Open();

                    // The stored procedure will return user data only if the credentials
                    // are correct and the account status is 'Active'.
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        // Login was successful.
                        string roleName = reader["RoleName"].ToString();

                        MessageBox.Show("Welcome, " + reader["FirstName"].ToString() + "!", "Login Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Hide();

                        // Redirect the user to the appropriate dashboard based on their role.
                        switch (roleName)
                        {
                            case "Admin":
                                AdminDashboard adminDash = new AdminDashboard();
                                adminDash.Show();
                                break;
                            case "Instructor":
                                InstructorDashboard teacherDash = new InstructorDashboard();
                                teacherDash.Show();
                                break;
                            case "Student":
                                StudentDashboard studentDash = new StudentDashboard();
                                studentDash.Show();
                                break;
                            default:
                                MessageBox.Show("Unknown user role. Please contact support.", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                this.Show();
                                break;
                        }
                    }
                    else
                    {
                        // Login failed for any reason (incorrect credentials or pending status).
                        MessageBox.Show("Login failed. Please check your username and password, or wait for an administrator to activate your account.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("Database error: " + ex.Message, "SQL Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An unexpected error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }


        }

        private string HashPassword(string plainPassword)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(plainPassword);
                byte[] hash = sha256.ComputeHash(bytes);
                StringBuilder builder = new StringBuilder();
                foreach (byte b in hash)
                    builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }

        private void lblRegister_Click(object sender, EventArgs e)
        {
            Registration regForm = new Registration();
            regForm.Show();
            this.Hide();
        }

        private void lblForgotPassword_Click(object sender, EventArgs e)
        {
            ForgotPassword forgotPassForm = new ForgotPassword();
            forgotPassForm.Show();
            this.Hide();
        }
    }
}
