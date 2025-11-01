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
    public partial class InstructorPersonalInfo : Form
    {
        private const string connectionString = @"Data Source = canasa\SQLEXPRESS; Initial catalog = KCUnivDB; Integrated Security = true";
        private int loggedInProfileId;


        public InstructorPersonalInfo(int profileId)
        {
            InitializeComponent();
            this.loggedInProfileId = profileId;

            txtAge.ReadOnly = true;
            txtGender.ReadOnly = true;
            txtAddress.ReadOnly = true;
            txtEmail.ReadOnly = true;
            txtPhoneNumber.ReadOnly = true; 

            this.Load += InstructorPersonalInfo_Load;
        }

        private void InstructorPersonalInfo_Load(object sender, EventArgs e)
        {
            LoadInstructorPersonalInfo(loggedInProfileId);
        }

        private void LoadInstructorPersonalInfo(int profileId)
        {
            string query = @"
                SELECT 
                    FirstName, 
                    LastName, 
                    Age, 
                    Gender, 
                    Phone, 
                    Email, 
                    Address
                FROM Profiles
                WHERE ProfileID = @ProfileID";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProfileID", profileId);

                    try
                    {
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        if (reader.Read())
                        {
                            string firstName = reader["FirstName"].ToString();
                            string lastName = reader["LastName"].ToString();

                            lblFullname.Text = $"{firstName} {lastName}";

                        
                            txtAge.Text = reader["Age"].ToString();
                            txtGender.Text = reader["Gender"].ToString();
                            txtAddress.Text = reader["Address"].ToString();

                            txtEmail.Text = reader["Email"].ToString();
                            txtPhoneNumber.Text = reader["Phone"].ToString();
                        }
                        else
                        {
                            MessageBox.Show("Instructor profile data not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        reader.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Database error loading instructor info: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }


        private void btnHome_Click(object sender, EventArgs e)
        {
            InstructorDashboard dashboard = new InstructorDashboard(this.loggedInProfileId);
            dashboard.Show();
            this.Hide();
        }

        private void btnSubjectHandled_Click(object sender, EventArgs e)
        {
            InstructorSubjectHandled sf = new InstructorSubjectHandled(this.loggedInProfileId);
            sf.Show();
            this.Hide();
        }
    }
}
