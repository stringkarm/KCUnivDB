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
    public partial class InstructorSubjectHandled : Form
    {
        private const string connectionString = @"Data Source=canasa\SQLEXPRESS;Initial Catalog=KCUnivDB;Integrated Security=True";
        private int loggedInProfileId;


        public InstructorSubjectHandled(int profileId)
        {
            InitializeComponent();
            this.loggedInProfileId = profileId;

            this.Load += InstructorSubjectHandled_Load;
        }

        private void InstructorSubjectHandled_Load(object sender, EventArgs e)
        {
            LoadSubjectsHandled();
            LoadStudentsHandled();
        }

        private void LoadSubjectsHandled()
        {
            string query = @"
                SELECT 
                    c.CourseCode AS [Course Code],
                    c.CourseName AS [Course Name],
                    c.Credits AS [Units],
                    s.TermName AS [Semester]
                FROM InstructorSubjects i
                INNER JOIN Courses c ON i.CourseID = c.CourseID
                INNER JOIN Semesters s ON i.SemesterID = s.SemesterID
                INNER JOIN Instructors inst ON i.InstructorID = inst.InstructorID
                WHERE inst.ProfileID = @ProfileID
                  AND c.Status = 'Active';";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ProfileID", loggedInProfileId);

                try
                {
                    connection.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    dtgSubjectHandled.DataSource = table;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Database error loading subjects: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void LoadStudentsHandled()
        {
            string query = @"
                SELECT 
                    st.StudentID AS [Student ID],
                    p.FirstName AS [First Name],
                    p.LastName AS [Last Name],
                    pr.ProgramName AS [Program Name]
                FROM StudentEnrollments se
                INNER JOIN Students st ON se.StudentID = st.StudentID
                INNER JOIN Profiles p ON st.ProfileID = p.ProfileID
                INNER JOIN Programs pr ON se.ProgramID = pr.ProgramID
                INNER JOIN Courses c ON se.CourseID = c.CourseID
                INNER JOIN InstructorSubjects ins ON se.CourseID = ins.CourseID
                INNER JOIN Instructors i ON ins.InstructorID = i.InstructorID
                WHERE i.ProfileID = @ProfileID
                  AND p.Status = 'Active'
                  AND c.Status = 'Active';";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ProfileID", loggedInProfileId);

                try
                {
                    connection.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    dtgStudents.DataSource = table;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Database error loading students: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        private void dtgSubjectHandled_SelectionChanged(object sender, EventArgs e)
        {
            
        }


        private void btnHome_Click(object sender, EventArgs e)
        {
            InstructorDashboard dashboard = new InstructorDashboard(this.loggedInProfileId);
            dashboard.Show();
            this.Hide();
        }

        private void btnSubjectHandled_Click(object sender, EventArgs e)
        {

        }

        private void btnPersonalInformation_Click(object sender, EventArgs e)
        {
            InstructorPersonalInfo personalInfo = new InstructorPersonalInfo(this.loggedInProfileId);
            personalInfo.Show();
            this.Hide();
        }
    }
}
