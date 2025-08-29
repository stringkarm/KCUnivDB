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

            string plainPassword = txtPassword.Text;
            // Hash the plain text password before sending it to the database
            string hashedPassword = HashPassword(plainPassword);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("Login_SP", connection);
                cmd.CommandType = CommandType.StoredProcedure;


                cmd.Parameters.AddWithValue("@username", txtUsername.Text);
                cmd.Parameters.AddWithValue("@password", hashedPassword);

                try
                {
                    connection.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {

                        string result = reader["Result"].ToString();
                        int userId = Convert.ToInt32(reader["UserID"]);
                        int profileId = Convert.ToInt32(reader["ProfileID"]);
                        int roleId = Convert.ToInt32(reader["RoleID"]);
                        string status = reader["Status"].ToString();

                        // Check if the user's status is "Active"
                        if (status != "Active")
                        {
                            // Updated message as requested
                            MessageBox.Show("Your account is pending approval. Please wait for the admin to approve your account.");
                            this.Show();
                            return;
                        }

                        MessageBox.Show(result);    
                        this.Hide();

                        if (roleId == 1)
                        {

                            AdminDashboard adminDash = new AdminDashboard();
                            adminDash.Show();
                        }
                        else if (roleId == 2)
                        {

                            InstructorDashboard teacherDash = new InstructorDashboard();
                            teacherDash.Show();
                        }
                        else if (roleId == 3)
                        {

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
                        // The login failed because the reader returned no rows.
                        MessageBox.Show("Login failed or no response from server. Please try again.");
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("Database error: " + ex.Message + "\n" + ex.Number);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An unexpected error occurred: " + ex.Message);
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
                {
                    builder.Append(b.ToString("x2"));
                }
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
