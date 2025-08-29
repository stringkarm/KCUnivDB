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

        string connectionString = @"Data Source = LAB1-PC18\LAB1PC35;
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

                SqlCommand cmd = new SqlCommand("Registration_SP", connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@firstname", txtFirstname.Text);
                cmd.Parameters.AddWithValue("@lastname", txtLastname.Text);
                cmd.Parameters.AddWithValue("@age", age);
                cmd.Parameters.AddWithValue("@gender",cmbGender.Text);
                cmd.Parameters.AddWithValue("@phone", txtPhone.Text);
                cmd.Parameters.AddWithValue("@address", txtAddress.Text);
                cmd.Parameters.AddWithValue("@email", txtEmail.Text);




                connection.Open();
                cmd.ExecuteNonQuery();

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

        
    }
}
