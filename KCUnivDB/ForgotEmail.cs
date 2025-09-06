using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KCUnivDB
{
    public partial class ForgotEmail : Form
    {
        private string connectionString = @"Data Source=canasa\SQLEXPRESS; Initial catalog=KCUnivDB; Integrated Security=true";

        public ForgotEmail()
        {
            InitializeComponent();
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();

            if (string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Please enter your email address.", "Required Field", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check if the email exists in the database
            string query = "SELECT ProfileID FROM Profiles WHERE Email = @Email";
            int profileId = -1;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Email", email);

                try
                {
                    connection.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        profileId = Convert.ToInt32(result);
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("Database error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            if (profileId != -1)
            {
                // If email exists, open the password confirmation form
                ForgotConfirmation forgotConfirmForm = new ForgotConfirmation(profileId);
                this.Hide(); // Hide the current form
                forgotConfirmForm.Show(); // Show the new form
            }
            else
            {
                MessageBox.Show("Email not found. Please check your email and try again.", "Validation Failed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
