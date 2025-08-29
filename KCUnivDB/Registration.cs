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

        private void btnRegister_Click(object sender, EventArgs e)
        {
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
                    // 1. Open the connection
                    connection.Open();

                    // 2. Add the check for existing email
                    SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM Profiles WHERE Email = @email", connection);
                    checkCmd.Parameters.AddWithValue("@email", txtEmail.Text);

                    // 3. Execute the command while the connection is open
                    int userCount = (int)checkCmd.ExecuteScalar();

                    if (userCount > 0)
                    {
                        MessageBox.Show("This email is already registered. Please use a different one.", "Registration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    Random rnd = new Random();
                    string generatedUserID = "ST" + rnd.Next(100000, 999999).ToString();
                    string generatedPassword = generatedUserID;

                    // Step 3: Hash the generated password using the HashPassword function
                    string hashedPassword = HashPassword(generatedPassword);

                    // Step 4: Call the stored procedure with the generated data
                    SqlCommand cmd = new SqlCommand("Registration_SP", connection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Pass the user's personal details
                    cmd.Parameters.AddWithValue("@FirstName", txtFirstname.Text);
                    cmd.Parameters.AddWithValue("@LastName", txtLastname.Text);
                    cmd.Parameters.AddWithValue("@Age", age);
                    cmd.Parameters.AddWithValue("@Gender", cmbGender.Text);
                    cmd.Parameters.AddWithValue("@Phone", txtPhone.Text);
                    cmd.Parameters.AddWithValue("@Address", txtAddress.Text);
                    cmd.Parameters.AddWithValue("@Email", txtEmail.Text);

                    // Pass the generated username and the HASHED password
                    cmd.Parameters.AddWithValue("@Username", generatedUserID);
                    cmd.Parameters.AddWithValue("@HashedPassword", hashedPassword);


                    // Since we are not getting any output parameters back, we can just execute the command
                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Registration Successful!" + "\n Username: " + generatedUserID+ "\n Password: " + generatedPassword + "\n Paghuwat, i-approved pakas admin.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"A database error occurred: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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


    }
}
