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
    public partial class InstructorDashboard : Form
    {

        string connectionString = @"Data Source = canasa\SQLEXPRESS;
        Initial catalog = KCUnivDB; Integrated Security = true";
        private int loggedInProfileId;

        public InstructorDashboard(int profileId)
        {
            InitializeComponent();
            this.loggedInProfileId = profileId;
            LoadInstructorName();
        }

        private void LoadInstructorName()
        {
          
            string query = @"
        SELECT 
            FirstName, 
            LastName 
        FROM Profiles 
        WHERE ProfileID = @ProfileID";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ProfileID", loggedInProfileId);

                    try
                    {
                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();

                        if (reader.Read())
                        {
                            string firstName = reader["FirstName"].ToString();
                            string lastName = reader["LastName"].ToString();
                            string fullName = $"{firstName} {lastName}";

                         
                            lblInstructorName.Text = $"{fullName}"; 
                            lblWelcome.Text = $"Welcome Back, {firstName}!"; 

                           
                            lblDateDisplay.Text = DateTime.Now.ToString("dd/MM/yyyy");
                        }
                        else
                        {
                         
                            lblInstructorName.Text = "Profile Not Found";
                            lblWelcome.Text = "Welcome Back, User!";
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading instructor profile: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        lblInstructorName.Text = "[Database Error]";
                    }
                }
            }
        }

       

        private void lblLogout_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to logout?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Form1 form1 = new Form1();
                form1.Show();
                this.Hide();
            }
    }
}
}
