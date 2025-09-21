using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KCUnivDB
{
    public partial class AddTeacher : UserControl
    {
        public AddTeacher()
        {
            InitializeComponent();
        }

        private string connectionString = @"Data Source=canasa\SQLEXPRESS; Initial catalog=KCUnivDB; Integrated Security=true";

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

        private void btnRegister_Click(object sender, EventArgs e)
        {
            errorProvider1.Clear();
            bool isValid = true;
            string email = txtEmail.Text.Trim();

            // First Name Validation
            if (string.IsNullOrWhiteSpace(txtFirstname.Text))
            {
                errorProvider1.SetError(txtFirstname, "First Name is required.");
                isValid = false;
            }

            // Last Name Validation
            if (string.IsNullOrWhiteSpace(txtLastname.Text))
            {
                errorProvider1.SetError(txtLastname, "Last Name is required.");
                isValid = false;
            }

            // Age Validation
            if (string.IsNullOrWhiteSpace(txtAge.Text))
            {
                errorProvider1.SetError(txtAge, "Age is required.");
                isValid = false;
            }


            // Gender Validation
            if (string.IsNullOrWhiteSpace(cmbGender.Text))
            {
                errorProvider1.SetError(cmbGender, "Gender is required.");
                isValid = false;
            }

            // Phone Number Validation
            if (string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                errorProvider1.SetError(txtPhone, "Phone Number is required.");
                isValid = false;
            }
            else if (!Regex.IsMatch(txtPhone.Text, @"^\d{11}$"))
            {
                errorProvider1.SetError(txtPhone, "Phone Number must be 11 digits.");
                isValid = false;
            }

            // Email Validation
            if (string.IsNullOrWhiteSpace(email))
            {
                errorProvider1.SetError(txtEmail, "Email Address is required.");
                isValid = false;
            }
            else if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                errorProvider1.SetError(txtEmail, "Please enter a valid email address.");
                isValid = false;
            }
            else if (IsEmailExist(email))
            {
                errorProvider1.SetError(txtEmail, "Email already exists. Please use a different one.");
                isValid = false;
            }

            // Address Validation
            if (string.IsNullOrWhiteSpace(txtAddress.Text))
            {
                errorProvider1.SetError(txtAddress, "Address is required.");
                isValid = false;
            }

            if (!isValid)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(txtFirstname.Text) || string.IsNullOrWhiteSpace(txtLastname.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("Please fill in all required fields.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int age;
            if (!int.TryParse(txtAge.Text, out age))
            {
                MessageBox.Show("Please enter a valid age.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        // Generate a new unique teacher username (e.g., TC000001, TC000002)
                        string getNextIdQuery = "SELECT ISNULL(MAX(SUBSTRING(Username, 3, 6)), 0) FROM Users WHERE Username LIKE 'TC%'";
                        SqlCommand getNextIdCmd = new SqlCommand(getNextIdQuery, connection);
                        int nextId = Convert.ToInt32(getNextIdCmd.ExecuteScalar()) + 1;
                        string generatedUsername = $"TC{nextId:D6}";
                        string generatedPassword = generatedUsername; // Password is the same as the username
                        string hashedPassword = HashPassword(generatedPassword);

                        // 1. Insert into Profiles table
                        string insertProfileQuery = "INSERT INTO Profiles (FirstName, LastName, Age, Gender, Phone, Email, Address, Status) VALUES (@FirstName, @LastName, @Age, @Gender, @Phone, @Email, @Address, 'Active'); SELECT SCOPE_IDENTITY();";
                        SqlCommand insertProfileCmd = new SqlCommand(insertProfileQuery, connection);
                        insertProfileCmd.Parameters.AddWithValue("@FirstName", txtFirstname.Text.Trim());
                        insertProfileCmd.Parameters.AddWithValue("@LastName", txtLastname.Text.Trim());
                        insertProfileCmd.Parameters.AddWithValue("@Age", int.Parse(txtAge.Text.Trim()));
                        insertProfileCmd.Parameters.AddWithValue("@Gender", cmbGender.Text);
                        insertProfileCmd.Parameters.AddWithValue("@Phone", txtPhone.Text.Trim());
                        insertProfileCmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                        insertProfileCmd.Parameters.AddWithValue("@Address", txtAddress.Text.Trim());

                        int profileId = Convert.ToInt32(insertProfileCmd.ExecuteScalar());

                        // 2. Insert into Users table
                        string insertUserQuery = "INSERT INTO Users (Username, Password, RoleID, ProfileID) VALUES (@Username, @Password, @RoleID, @ProfileID)";
                        SqlCommand insertUserCmd = new SqlCommand(insertUserQuery, connection);
                        insertUserCmd.Parameters.AddWithValue("@Username", generatedUsername);
                        insertUserCmd.Parameters.AddWithValue("@Password", hashedPassword);
                        insertUserCmd.Parameters.AddWithValue("@RoleID", 2); 
                        insertUserCmd.Parameters.AddWithValue("@ProfileID", profileId);

                        insertUserCmd.ExecuteNonQuery();

                        MessageBox.Show($"Teacher registered successfully!\nUsername: {generatedUsername}\nPassword: {generatedPassword}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearFields();
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("Database error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An unexpected error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            
            
        }
        private void ClearFields()
        {
            txtFirstname.Clear();
            txtLastname.Clear();
            txtAge.Clear();
            cmbGender.SelectedIndex = -1;
            txtPhone.Clear();
            txtEmail.Clear();
            txtAddress.Clear();
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private bool IsEmailExist(string email)
        {

            string query = "SELECT COUNT(*) FROM Users WHERE Email = @Email";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Email", email);

                try
                {
                    connection.Open();
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
                catch (SqlException ex)
                {

                    Console.WriteLine("Database error in IsEmailExist: " + ex.Message);
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An unexpected error occurred in IsEmailExist: " + ex.Message);
                    return false;
                }
            }
        }
    }

}
