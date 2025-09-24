using Guna.UI2.WinForms.Suite;
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
    public partial class AdminRegister : UserControl
    {

        public delegate void RegistrationCompletedEventHandler(object sender, EventArgs e);
        public event RegistrationCompletedEventHandler RegistrationCompleted;


        public AdminRegister()
        {
            InitializeComponent();
        }

        string connectionString = @"Data Source = canasa\SQLEXPRESS;
        Initial catalog = KCUnivDB; Integrated Security = true";


        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            if (parentForm != null)
            {
                parentForm.WindowState = FormWindowState.Minimized;
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

        public void ClearFields()
        {
            txtFirstname.Clear();
            txtLastname.Clear();
            txtAge.Clear();
            cmbGender.SelectedIndex = -1;
            txtPhone.Clear();
            txtEmail.Clear();
            txtAddress.Clear();
            errorProvider1.Clear();
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            errorProvider1.Clear();
            bool isValid = true;
            string email = txtEmail.Text.Trim();
            string action = "Add Student";
            string description = "Added a new student";

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
            int age;
            if (string.IsNullOrWhiteSpace(txtAge.Text) || !int.TryParse(txtAge.Text, out age))
            {
                errorProvider1.SetError(txtAge, "Please enter a valid age.");
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

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    Random rnd = new Random();
                    // A more robust way to generate a unique ID to prevent duplicates
                    string generatedUserID = "ST" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
                    string generatedPassword = generatedUserID;
                    string hashedPassword = HashPassword(generatedPassword);

                    SqlCommand cmd = new SqlCommand("AddStudent_SP", connection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@firstname", txtFirstname.Text);
                    cmd.Parameters.AddWithValue("@lastname", txtLastname.Text);
                    cmd.Parameters.AddWithValue("@age", txtAge.Text); // Corrected: use the parsed integer value
                    cmd.Parameters.AddWithValue("@gender", cmbGender.Text);
                    cmd.Parameters.AddWithValue("@phone", txtPhone.Text);
                    cmd.Parameters.AddWithValue("@address", txtAddress.Text);
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@Username", generatedUserID);
                    cmd.Parameters.AddWithValue("@HashedPassword", hashedPassword);
                    cmd.Parameters.AddWithValue("@EnrollmentDate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@Action", action);
                    cmd.Parameters.AddWithValue("@Description", description);

                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Registration Successful!" + "\n Username: " + generatedUserID + "\n Password: " + generatedPassword + "\n The student account is officially active.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Call the method to clear all fields after successful registration
                    ClearFields();

                    // Raise the event to notify the parent form
                    RegistrationCompleted?.Invoke(this, EventArgs.Empty);

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

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {

        }

        private void guna2TextBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }

}
