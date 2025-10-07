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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace KCUnivDB
{
    public partial class AddSubject : UserControl
    {

        public AddSubject()
        {
            InitializeComponent();
            LoadDepartments();
            LoadSemesters();
        }


        private string connectionString = @"Data Source = canasa\SQLEXPRESS;
        Initial catalog = KCUnivDB; Integrated Security = true";

        private void LoadDepartments()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT DepartmentID, DepartmentName FROM Departments";
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    cmbDepartment.DataSource = dt;
                    cmbDepartment.DisplayMember = "DepartmentName";
                    cmbDepartment.ValueMember = "DepartmentID";
                    cmbDepartment.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading departments: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSemesters()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT SemesterID, TermName FROM Semesters ORDER BY SemesterID";
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    cmbSemester.DataSource = dt;
                    cmbSemester.DisplayMember = "TermName";
                    cmbSemester.ValueMember = "SemesterID";
                    cmbSemester.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading semesters: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private bool CourseNameExists(string courseName, SqlConnection conn)
        {
            string query = "SELECT COUNT(1) FROM Courses WHERE CourseName = @CourseName";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@CourseName", courseName);
                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        private bool CourseCodeExists(string courseCode, SqlConnection conn)
        {
            string query = "SELECT COUNT(1) FROM Courses WHERE CourseCode = @CourseCode";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@CourseCode", courseCode);
                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        private bool ValidateForm(SqlConnection conn)
        {
            errorProvider1.Clear();
            bool isValid = true;


            if (string.IsNullOrWhiteSpace(txtCourseName.Text))
            {
                errorProvider1.SetError(txtCourseName, "Course Name is required.");
                isValid = false;
            }


            if (string.IsNullOrWhiteSpace(txtCourseCode.Text))
            {
                errorProvider1.SetError(txtCourseCode, "Course Code is required.");
                isValid = false;
            }



            if (string.IsNullOrWhiteSpace(txtCredits.Text))
            {
                errorProvider1.SetError(txtCredits, "Credits field is required.");
                isValid = false;
            }
            else if (!int.TryParse(txtCredits.Text, out int credits) || credits <= 0)
            {
                errorProvider1.SetError(txtCredits, "Credits must be a positive whole number.");
                isValid = false;
            }


            if (cmbDepartment.SelectedValue == null || cmbDepartment.SelectedIndex == -1)
            {
                errorProvider1.SetError(cmbDepartment, "A Department selection is required.");
                isValid = false;
            }

            if (cmbSemester.SelectedValue == null || cmbSemester.SelectedIndex == -1)
            {
                errorProvider1.SetError(cmbSemester, "A Semester selection is required.");
                isValid = false;
            }

            if (isValid)
            {
                try
                {


                    if (CourseNameExists(txtCourseName.Text.Trim(), conn))
                    {
                        errorProvider1.SetError(txtCourseName, "A course with this Course Name already exists.");
                        isValid = false;
                    }

                    if (CourseCodeExists(txtCourseCode.Text.Trim(), conn))
                    {
                        errorProvider1.SetError(txtCourseCode, "A course with this Course Code already exists.");
                        isValid = false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Database check failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    isValid = false;
                }
            }

            return isValid;
        }



          private void InsertLog(SqlConnection conn, SqlTransaction transaction, string action, string description)
            {
                string query = @"
        INSERT INTO Logs (ProfileID, Action, Date, Time, Description)
        VALUES (@ProfileID, @Action, CAST(GETDATE() AS DATE), CONVERT(VARCHAR(8), GETDATE(), 108), @Description)";

                using (SqlCommand cmd = new SqlCommand(query, conn, transaction)) 
                {
                    cmd.Parameters.AddWithValue("@ProfileID", DBNull.Value);
                    cmd.Parameters.AddWithValue("@Action", action);
                    cmd.Parameters.AddWithValue("@Description", description);
                    cmd.ExecuteNonQuery();
                }
            }

     



        private void btnSubmit_Click(object sender, EventArgs e)
        {

            errorProvider1.Clear();
            bool isValid = true;

            string courseName = txtCourseName.Text.Trim();
            string courseCode = txtCourseCode.Text.Trim();
            string description = txtDescription.Text.Trim();
            int credits = 0;
            int departmentID = 0;
            int? semesterID = null;


            if (string.IsNullOrWhiteSpace(courseName))
            {
                errorProvider1.SetError(txtCourseName, "Course name is required.");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(courseCode))
            {
                errorProvider1.SetError(txtCourseCode, "Course code is required.");
                isValid = false;
            }

            if (!int.TryParse(txtCredits.Text.Trim(), out credits) || credits <= 0)
            {
                errorProvider1.SetError(txtCredits, "Credits must be a positive number.");
                isValid = false;
            }

            if (cmbDepartment.SelectedValue == null)
            {
                errorProvider1.SetError(cmbDepartment, "Please select a department.");
                isValid = false;
            }
            else
            {
                departmentID = Convert.ToInt32(cmbDepartment.SelectedValue);
            }

            if (cmbSemester.SelectedValue == null || cmbSemester.SelectedIndex == -1)
            {
                errorProvider1.SetError(cmbSemester, "Please select a semester.");
                isValid = false;
            }
            else
            {
             
                semesterID = Convert.ToInt32(cmbSemester.SelectedValue);
            }

            if (!isValid)
            {
                MessageBox.Show("Please correct the highlighted errors.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            InsertCourse(courseName, courseCode, credits, description, departmentID, semesterID);
        }

        private void InsertCourse(string courseName, string courseCode, int credits, string description, int departmentID, int? semesterID)
        {
            string insertCourseQuery = @"
        INSERT INTO Courses (CourseName, CourseCode, Credits, Description, DepartmentID, SemesterID, Status)
        VALUES (@CourseName, @CourseCode, @Credits, @Description, @DepartmentID, @SemesterID, 'Active')";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    SqlTransaction transaction = conn.BeginTransaction();

                    using (SqlCommand cmd = new SqlCommand(insertCourseQuery, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@CourseName", courseName);
                        cmd.Parameters.AddWithValue("@CourseCode", courseCode);
                        cmd.Parameters.AddWithValue("@Credits", credits);
                        cmd.Parameters.AddWithValue("@Description", description);
                        cmd.Parameters.AddWithValue("@DepartmentID", departmentID);

                        if (semesterID.HasValue)
                            cmd.Parameters.AddWithValue("@SemesterID", semesterID.Value);
                        else
                            cmd.Parameters.AddWithValue("@SemesterID", DBNull.Value);

                        cmd.ExecuteNonQuery();
                    }

               
                    string action = "Add Subject";
                    string logDescription = $"Added new subject: {courseName} ({courseCode})";
                    InsertLog(conn, transaction, action, logDescription);

                    transaction.Commit();

                    MessageBox.Show("New course successfully added and logged!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error inserting course: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }




        private void ClearForm()
        {
            txtCourseName.Clear();
            txtCourseCode.Clear();
            txtCredits.Clear();
            txtDescription.Clear();
            cmbDepartment.SelectedIndex = -1;
            cmbSemester.SelectedIndex = -1;
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
