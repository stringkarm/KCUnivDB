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
    public partial class StudentEnrollment : Form
    {
        private const string connectionString = @"Data Source=canasa\SQLEXPRESS; Initial catalog=KCUnivDB; Integrated Security=true";
        private int loggedInProfileId;
        private int currentStudentID;

        public StudentEnrollment(int profileId)
        {
            InitializeComponent();
            this.loggedInProfileId = profileId;

            
            txtStudentID.ReadOnly = true;
            txtFullName.ReadOnly = true;
            txtDepartment.ReadOnly = true;
            txtProgramName.ReadOnly = true;

            this.Load += StudentEnrollment_Load;
        }

        private void StudentEnrollment_Load(object sender, EventArgs e)
        {
            LoadStudentEnrollmentInfo(loggedInProfileId);
        }

        private void LoadStudentEnrollmentInfo(int profileId)
        {

            string basicInfoQuery = @"
                SELECT
                    S.StudentID, P.FirstName, P.LastName
                FROM Students S
                INNER JOIN Profiles P ON S.ProfileID = P.ProfileID
                WHERE S.ProfileID = @ProfileID;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(basicInfoQuery, connection))
                {
                    command.Parameters.AddWithValue("@ProfileID", profileId);

                    try
                    {
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        if (reader.Read())
                        {
                            currentStudentID = Convert.ToInt32(reader["StudentID"]);
                            txtStudentID.Text = currentStudentID.ToString();
                            txtFullName.Text = $"{reader["FirstName"]} {reader["LastName"]}";

                            reader.Close(); 

                            LoadStudentProgramAndDepartment(currentStudentID);

                            LoadStudentGrades(currentStudentID);
                        }
                        else
                        {
                            MessageBox.Show("Student profile not linked. Cannot retrieve basic data.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Database error loading basic student info: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void LoadStudentProgramAndDepartment(int studentId)
        {
            string programQuery = @"
        SELECT TOP 1
            ISNULL(PR.ProgramName, '--- Data Link Missing ---') AS ProgramName,
            ISNULL(D.DepartmentName, '--- Data Link Missing ---') AS DepartmentName
        FROM StudentEnrollments SE
        LEFT JOIN Programs PR ON SE.ProgramID = PR.ProgramID
        LEFT JOIN Departments D ON PR.DepartmentID = D.DepartmentID
        WHERE SE.StudentID = @StudentID
        ORDER BY SE.EnrollDate DESC;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(programQuery, connection))
                {
                    command.Parameters.AddWithValue("@StudentID", studentId);

                    try
                    {
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        if (reader.Read())
                        {
                            txtProgramName.Text = reader["ProgramName"].ToString();
                            txtDepartment.Text = reader["DepartmentName"].ToString();
                        }
                        else
                        {
                            txtProgramName.Text = "--- No Enrollment Found ---";
                            txtDepartment.Text = "--- No Enrollment Found ---";
                        }

                        reader.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Database error loading Program/Department: {ex.Message}",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }


        private void LoadStudentGrades(int studentId)
        {
            string gradesQuery = @"
        SELECT
            C.CourseCode,
            C.CourseName,
            C.Credits AS Units, 
            ISNULL(P.FirstName + ' ' + P.LastName, 'TBA') AS Teacher, 
            ISNULL(CAST(SE.Grade AS NVARCHAR), 'N/A') AS Grade
        FROM StudentEnrollments SE
        INNER JOIN Courses C ON SE.CourseID = C.CourseID
        
        -- Joining through InstructorSubjects (assuming one instructor per subject for the student's enrollment period)
        LEFT JOIN InstructorSubjects ISUB ON C.CourseID = ISUB.CourseID
        LEFT JOIN Instructors I ON ISUB.InstructorID = I.InstructorID
        LEFT JOIN Profiles P ON I.ProfileID = P.ProfileID
        
        WHERE SE.StudentID = @StudentID;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(gradesQuery, connection))
                {
                    command.Parameters.AddWithValue("@StudentID", studentId);

                    try
                    {
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                    
                        dtgGrades.DataSource = dataTable;
                        dtgGrades.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Database error loading grades: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnHome_Click(object sender, EventArgs e)
        {
            StudentDashboard dashboard = new StudentDashboard(this.loggedInProfileId);
            dashboard.Show();
            this.Hide();
        }

        private void btnPersonalInformation_Click(object sender, EventArgs e)
        {
            StudentPersonalInfo personalInfoForm = new StudentPersonalInfo(this.loggedInProfileId);
            personalInfoForm.Show();
            this.Hide();
        }
    }
}
