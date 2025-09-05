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
        private string 
            HashPassword(string plainPassword)
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


      
        private void lblForgotPassword_Click(object sender, EventArgs e)
        {
            ForgotPassword forgotPassForm = new ForgotPassword();
            forgotPassForm.Show();
            this.Hide();
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
            // 1. Get the plain text password from the textbox.
            string plainPassword = txtPassword.Text;

            // 2. Hash the plain text password.
            // This is the CRUCIAL step. The database must contain this hashed value.
            string hashedPassword = HashPassword(plainPassword);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // 3. Set up the SQL command to execute the stored procedure.
                SqlCommand cmd = new SqlCommand("Login_SP", connection);
                cmd.CommandType = CommandType.StoredProcedure;

                // 4. Add the username and the *hashed* password as parameters.
                cmd.Parameters.AddWithValue("@username", txtUsername.Text);
                cmd.Parameters.AddWithValue("@password", hashedPassword);

                try
                {
                    // 5. Open the connection and execute the command.
                    connection.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    // 6. Check if the reader found a matching user.
                    if (reader.Read())
                    {
                        // The user was found. Now check their status and role.
                        string status = reader["Status"].ToString();

                        if (status != "Active")
                        {
                            MessageBox.Show("Your account is pending approval. Please wait for the admin to approve your account.");
                            this.Show();
                            return;
                        }

                        // Get the user's role ID.
                        int roleId = Convert.ToInt32(reader["RoleID"]);

                        // Redirect the user based on their role.
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
                        // The reader returned no rows, which means no matching username/password pair was found.
                        MessageBox.Show("Login failed. Invalid username or password.");
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("Database error: " + ex.Message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An unexpected error occurred: " + ex.Message);
                }
            }


        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            Registration register = new Registration();
            register.Show();
            this.Hide();
        }
    }
}
