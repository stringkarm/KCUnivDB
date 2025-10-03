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
            this.cmbDepartment.SelectedIndexChanged += new EventHandler(cmbDepartment_SelectedIndexChanged);
        }

      
        private string connectionString = @"Data Source = canasa\SQLEXPRESS;
        Initial catalog = KCUnivDB; Integrated Security = true";

        private void LoadDepartments()
        {
            string query = "SELECT DepartmentID, DepartmentName FROM Departments ORDER BY DepartmentName";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    cmbDepartment.DataSource = dt;
                    cmbDepartment.DisplayMember = "DepartmentName";
                    cmbDepartment.ValueMember = "DepartmentID";
                    cmbDepartment.SelectedIndex = -1;
                    cmbDepartment.Text = "-- Select Department --";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading departments: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void cmbDepartment_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbDepartment.SelectedValue != null && cmbDepartment.SelectedValue is int)
            {
                int departmentId = (int)cmbDepartment.SelectedValue;
                LoadTeachersByDepartment(departmentId);
            }
            else
            {
                
                cmbTeacher.DataSource = null;
                cmbTeacher.Items.Clear();
                cmbTeacher.Text = "-- Select Teacher --";
            }
        }

        private void LoadTeachersByDepartment(int departmentId)
        {

            string query = @"
                SELECT 
                    I.InstructorID, 
                    P.FirstName + ' ' + P.LastName AS FullName
                FROM Instructors I
                INNER JOIN Profiles P ON I.ProfileID = P.ProfileID
                WHERE I.DepartmentID = @DepartmentID -- Filter by the selected department
                ORDER BY FullName";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    da.SelectCommand.Parameters.AddWithValue("@DepartmentID", departmentId);

                    DataTable dt = new DataTable();
                    da.Fill(dt);


                    cmbTeacher.DataSource = dt;
                    cmbTeacher.DisplayMember = "FullName";
                    cmbTeacher.ValueMember = "InstructorID";
                    cmbTeacher.SelectedIndex = -1;
                    cmbTeacher.Text = (dt.Rows.Count > 0)
                                            ? "-- Select Teacher --"
                                            : "-- No Teachers in this Dept --";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading teachers: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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


            if (cmbTeacher.SelectedValue == null || cmbTeacher.SelectedIndex == -1 || cmbTeacher.Text.Contains("No Teachers"))
            {
                errorProvider1.SetError(cmbTeacher, "A Teacher selection is required.");
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

  

        private void InsertLog(SqlConnection conn, string action, string description)
        {
            string query = @"
        INSERT INTO Logs (ProfileID, Action, Date, Time, Description)
        VALUES (@ProfileID, @Action, CAST(GETDATE() AS DATE), CONVERT(VARCHAR(8), GETDATE(), 108), @Description)";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                // If you already have currentProfileID in your project, use it here.
                // If not, you can set ProfileID to NULL or a default admin ID.
                cmd.Parameters.AddWithValue("@ProfileID", DBNull.Value);

                cmd.Parameters.AddWithValue("@Action", action);
                cmd.Parameters.AddWithValue("@Description", description);

                cmd.ExecuteNonQuery();
            }
        }


        private void btnSubmit_Click(object sender, EventArgs e)
        {

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    if (!ValidateForm(conn))
                    {

                        return;
                    }

                    int credits = int.Parse(txtCredits.Text);
                    int departmentId = (int)cmbDepartment.SelectedValue;
                    int instructorId = (int)cmbTeacher.SelectedValue;
                    string courseName = txtCourseName.Text.Trim();
                    string courseCode = txtCourseCode.Text.Trim();
                    string courseDescription = txtDescription.Text.Trim();

                    string action = "Add Subject";
                    string description = "Added a new subject";

                    string departmentName = cmbDepartment.Text;
                    string teacherName = cmbTeacher.Text;
                    string status = "Active";

                    string insertQuery = @"
                        INSERT INTO Courses 
                            (CourseName, CourseCode, Credits, InstructorID, DepartmentID, Status, Description) 
                        VALUES 
                            (@CourseName, @CourseCode, @Credits, @InstructorID, @DepartmentID, 'Active', @Description)";

                    using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@CourseName", courseName);
                        cmd.Parameters.AddWithValue("@CourseCode", courseCode);
                        cmd.Parameters.AddWithValue("@Credits", credits);
                        cmd.Parameters.AddWithValue("@InstructorID", instructorId);
                        cmd.Parameters.AddWithValue("@DepartmentID", departmentId);
                        cmd.Parameters.AddWithValue("@Status", status);
                        cmd.Parameters.AddWithValue("@Action", action);
                        cmd.Parameters.AddWithValue("@AddDescription", description);

                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Added Subject Successful!" + "\n CourseCode: " + courseCode,
                                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                }
                catch (SqlException sqlEx)
                {
                    MessageBox.Show("Database Error: " + sqlEx.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An unexpected error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ClearFormFields()
        {
            txtCourseName.Clear();
            txtCourseCode.Clear();
            txtCredits.Clear();
            cmbDepartment.SelectedIndex = -1;
            cmbDepartment.Text = "-- Select Department --";
            txtDescription.Clear(); 
            cmbTeacher.DataSource = null;
            cmbTeacher.Items.Clear();
            cmbTeacher.Text = "-- Select Teacher --";
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
