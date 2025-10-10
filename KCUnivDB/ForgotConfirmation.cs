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
    public partial class ForgotConfirmation : Form
    {
        private string connectionString = @"Data Source=canasa\SQLEXPRESS; Initial catalog=KCUnivDB; Integrated Security=true";
        private string email;

        public ForgotConfirmation(string userEmail)
        {
            InitializeComponent();
            this.email = userEmail;
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

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            string newPassword = txtNewPassword.Text.Trim();
            string confirmPassword = txtConfirmPassword.Text.Trim();

            if (string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                MessageBox.Show("Please enter and confirm your new password.", "Validation Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (newPassword != confirmPassword)
            {
                MessageBox.Show("New password and confirmation do not match.", "Validation Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            string hashedNewPassword = HashPassword(newPassword);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    
                    string getProfileIdQuery = "SELECT ProfileID FROM Profiles WHERE Email = @Email";
                    SqlCommand getProfileIdCmd = new SqlCommand(getProfileIdQuery, connection);
                    getProfileIdCmd.Parameters.AddWithValue("@Email", email);
                    object profileIdObj = getProfileIdCmd.ExecuteScalar();

                    if (profileIdObj == null)
                    {
                        MessageBox.Show("User not found or email mismatch.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    int profileId = (int)profileIdObj;

                    
                    string updateQuery = "UPDATE Users SET Password = @NewPassword WHERE ProfileID = @ProfileID";
                    SqlCommand updateCmd = new SqlCommand(updateQuery, connection);
                    updateCmd.Parameters.AddWithValue("@NewPassword", hashedNewPassword);
                    updateCmd.Parameters.AddWithValue("@ProfileID", profileId);

                    int rows = updateCmd.ExecuteNonQuery();
                    if (rows > 0)
                    {
                        MessageBox.Show("Password changed successfully. You can now log in with your new password.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                 
                        Form1 form = new Form1();
                        form.Show();
                        this.Hide();
                    }
                    else
                    {
                        MessageBox.Show("Failed to update password. Try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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


        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            ForgotEmail email = new ForgotEmail();
            email.Show();
            this.Hide();
        }
    }
}
