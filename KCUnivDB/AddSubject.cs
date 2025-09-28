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

        private void btnSubmit_Click(object sender, EventArgs e)
        {

            if (string.IsNullOrWhiteSpace(txtCourseName.Text) ||
                string.IsNullOrWhiteSpace(txtCourseCode.Text) ||
                string.IsNullOrWhiteSpace(txtCredits.Text) ||
                cmbDepartment.SelectedValue == null ||
                cmbTeacher.SelectedValue == null)
            {
                MessageBox.Show("All required fields must be filled and selections made.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtCredits.Text, out int credits) || credits <= 0)
            {
                MessageBox.Show("Credits must be a positive whole number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int departmentId = (int)cmbDepartment.SelectedValue;
            int instructorId = (int)cmbTeacher.SelectedValue;
            string courseName = txtCourseName.Text.Trim();
            string courseCode = txtCourseCode.Text.Trim();
           
            string courseDescription = txtDescription.Text.Trim(); 

          
            string insertQuery = @"
                INSERT INTO Courses 
                    (CourseName, CourseCode, Credits, InstructorID, DepartmentID, Status, Description) 
                VALUES 
                    (@CourseName, @CourseCode, @Credits, @InstructorID, @DepartmentID, 'Active', @Description)";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@CourseName", courseName);
                        cmd.Parameters.AddWithValue("@CourseCode", courseCode);
                        cmd.Parameters.AddWithValue("@Credits", credits);
                        cmd.Parameters.AddWithValue("@InstructorID", instructorId);
                        cmd.Parameters.AddWithValue("@DepartmentID", departmentId);
                      
                        if (string.IsNullOrEmpty(courseDescription))
                        {
                            cmd.Parameters.AddWithValue("@Description", DBNull.Value);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@Description", courseDescription);
                        }

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Course added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            ClearFormFields();

                        }
                        else
                        {
                            MessageBox.Show("Course was not added. Please check the data.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
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
