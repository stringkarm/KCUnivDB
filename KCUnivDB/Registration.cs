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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

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


            using (SqlConnection connection = new SqlConnection(connectionString))
            {

                connection.Open();

                SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM Profiles WHERE Email = @email", connection);
                checkCmd.Parameters.AddWithValue("@email", txtEmail.Text);

                int userCount = (int)checkCmd.ExecuteScalar();

                if (userCount > 0)
                {
                    errorProvider1.SetError(txtEmail, "Email already exists. Please use a different one.");
                    return;
                }

                Random rnd = new Random();
                string generatedUserID = "ST" + rnd.Next(100000, 999999).ToString();
                string generatedPassword = generatedUserID;


                string hashedPassword = HashPassword(generatedPassword);

                SqlCommand cmd = new SqlCommand("Registration_SP", connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@FirstName", txtFirstname.Text);
                cmd.Parameters.AddWithValue("@LastName", txtLastname.Text);
                cmd.Parameters.AddWithValue("@Age", age);
                cmd.Parameters.AddWithValue("@Gender", cmbGender.Text);
                cmd.Parameters.AddWithValue("@Phone", txtPhone.Text);
                cmd.Parameters.AddWithValue("@Address", txtAddress.Text);
                cmd.Parameters.AddWithValue("@Email", txtEmail.Text);

                cmd.Parameters.AddWithValue("@Username", generatedUserID);
                cmd.Parameters.AddWithValue("@HashedPassword", hashedPassword);

                cmd.ExecuteNonQuery();

                MessageBox.Show("Registration Successful!" + "\n Username: " + generatedUserID + "\n Password: " + generatedPassword + "\n Wait for the admin to approve your account patiently.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Form1 form = new Form1();
                form.Show();
                this.Hide();
                
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


        private void label1_Click(object sender, EventArgs e)
        {
            Form1 form1 = new Form1();
            form1.Show();
            this.Hide();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            Form1 form1 = new Form1();
            form1.Show();
            this.Hide();
        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
