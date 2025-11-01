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
    public partial class StudentDashboard : Form
    {
        private string connectionString = @"Data Source=canasa\SQLEXPRESS;Initial catalog=KCUnivDB;Integrated Security=true";

        private int loggedInProfileId;

        public StudentDashboard(int profileId)
        {
            InitializeComponent();
            this.loggedInProfileId = profileId;
            LoadStudentName();
        }


        private void LoadStudentName()
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

                            
                            lblStudentName.Text = $"{fullName}";
                            lblWelcome.Text = $"Welcome Back, {firstName}!";

                            lblDateDisplay.Text = DateTime.Now.ToString("MM/dd/yyyy");
                        }
                        else
                        {
                            lblStudentName.Text = "Profile Not Found";
                            lblWelcome.Text = "Welcome Back, User!";
                        }
                        reader.Close(); 
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading student profile: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        lblStudentName.Text = "[Database Error]";
                    }
                } 
            } 
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnHome_Click(object sender, EventArgs e)
        {

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

        private void btnPersonalInformation_Click(object sender, EventArgs e)
        {
            StudentPersonalInfo studentPersonalInfoForm = new StudentPersonalInfo(this.loggedInProfileId);

            studentPersonalInfoForm.Show();
            this.Hide();
        }

        private void btnEnrollment_Click(object sender, EventArgs e)
        {
            StudentEnrollment enrol = new StudentEnrollment(this.loggedInProfileId);

            enrol.Show();
            this.Hide();
        }
    }
}
