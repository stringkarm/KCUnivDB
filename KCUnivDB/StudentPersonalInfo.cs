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
    public partial class StudentPersonalInfo : Form
    {
        private const string connectionString = @"Data Source=canasa\SQLEXPRESS; Initial catalog=KCUnivDB; Integrated Security=true";
        private int loggedInProfileId;

        public StudentPersonalInfo(int profileId)
        {
            InitializeComponent();
            this.loggedInProfileId = profileId;

            this.Load += StudentPersonalInfo_Load;
        }

        private void StudentPersonalInfo_Load(object sender, EventArgs e)
        {
            LoadStudentData(loggedInProfileId);
        }

        private void LoadStudentData(int profileId)
        {
            string sqlQuery = @"
                SELECT 
                    P.FirstName, P.LastName, P.Age, P.Gender, P.Address, P.Email, P.Phone 
                FROM Profiles P
                WHERE P.ProfileID = @ProfileID;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
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
                            MessageBox.Show("Student profile data not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        reader.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnEnrollment_Click(object sender, EventArgs e)
        {

        }

        private void btnHome_Click(object sender, EventArgs e)
        {
            StudentDashboard dashboard = new StudentDashboard(this.loggedInProfileId);
            dashboard.Show();
            this.Hide();
        }
    }
}
