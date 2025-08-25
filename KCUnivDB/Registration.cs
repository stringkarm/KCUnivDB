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
    public partial class Registration : Form
    {
        public Registration()
        {
            InitializeComponent();
        }

        string connectionString = @"Data Source = canasa\SQLEXPRESS;
                                Initial catalog = KCUnivDB; Integrated Security = true";


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

        private void btnRegister_Click(object sender, EventArgs e)
        {
            // 1. Client-side Validation (More robust)
            if (string.IsNullOrWhiteSpace(txtStudentID.Text) ||
                string.IsNullOrWhiteSpace(txtFirstname.Text) ||
                string.IsNullOrWhiteSpace(txtLastname.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtAge.Text) ||
                string.IsNullOrWhiteSpace(txtGender.Text) ||
                string.IsNullOrWhiteSpace(txtPhone.Text) ||
                string.IsNullOrWhiteSpace(txtAddress.Text))
            {
                MessageBox.Show("Please fill in all required fields.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate age is a number
            if (!int.TryParse(txtAge.Text, out int age))
            {
                MessageBox.Show("Please enter a valid age.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Call the Stored Procedure
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("Registration_SP", connection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Pass all parameters to the stored procedure.
                    cmd.Parameters.AddWithValue("@UserID", txtStudentID.Text);
                    cmd.Parameters.AddWithValue("@FirstName", txtFirstname.Text);
                    cmd.Parameters.AddWithValue("@LastName", txtLastname.Text);
                    cmd.Parameters.AddWithValue("@Age", age);
                    cmd.Parameters.AddWithValue("@Gender", txtGender.Text);
                    cmd.Parameters.AddWithValue("@Phone", txtPhone.Text);
                    cmd.Parameters.AddWithValue("@Address", txtAddress.Text);
                    cmd.Parameters.AddWithValue("@Email", txtEmail.Text);

                    // Add output parameters to retrieve the generated UserID and Password
                    SqlParameter usernameParam = new SqlParameter("@Username", SqlDbType.NVarChar, 50);
                    usernameParam.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(usernameParam);

                    SqlParameter passwordParam = new SqlParameter("@Password", SqlDbType.NVarChar, 255);
                    passwordParam.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(passwordParam);

                    connection.Open();
                    cmd.ExecuteNonQuery();

                    // Retrieve the output values from the stored procedure
                    string generatedUsername = usernameParam.Value.ToString();
                    string generatedPassword = passwordParam.Value.ToString();

                    MessageBox.Show("Registration successful!\n\n" +
                                    "Your Student ID (Username) is: " + generatedUsername + "\n" +
                                    "Your temporary password is: " + generatedPassword + "\n\n" +
                                    "Your account status is currently PENDING. An administrator must activate your account before you can log in.",
                                    "Registration Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Optional: Clear form fields after successful registration.
                    txtStudentID.Clear();
                    txtFirstname.Clear();
                    txtLastname.Clear();
                    txtAge.Clear();
                    txtGender.Clear();
                    txtPhone.Clear();
                    txtAddress.Clear();
                    txtEmail.Clear();
                }
                catch (SqlException ex)
                {
                    // Catch and display any database-specific errors.
                    MessageBox.Show("Database error: " + ex.Message, "SQL Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    // Catch any other unexpected errors.
                    MessageBox.Show("An unexpected error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
