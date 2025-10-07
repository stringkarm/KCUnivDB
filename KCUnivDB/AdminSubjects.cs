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
    public partial class AdminSubjects : Form
    {
        public AdminSubjects()
        {
            InitializeComponent();
            dtgSubjectsList.ReadOnly = true;
            EditSubjectPanel.Hide();
            addSubject1.Hide();
            dtgSubjectsList.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            LoadSubjectsData();

            DataTable departmentsData = DatabaseManager.GetDepartments();
            cmbDepartment.DataSource = departmentsData;
            cmbDepartment.DisplayMember = "DepartmentName";
            cmbDepartment.ValueMember = "DepartmentID";

            DataTable semestersData = DatabaseManager.GetSemesters();
            cmbSemester.DataSource = semestersData;
            cmbSemester.DisplayMember = "TermName";
            cmbSemester.ValueMember = "SemesterID";

            LoadSubjectsData();
            SetButtonStates();

        }

        string connectionString = Database.ConnectionString;
        private int activeSubjectCount = 0;
        private string selectedCourseId;


        private void UpdateActiveCountLabel()
        {

             lblTotalActive.Text = $"{activeSubjectCount}";
        }

     
        public void RefreshDataGrid()
        {
            LoadSubjectsData();
        }

        private void LoadSubjectsData()
        {
            string sqlQuery = @"
                SELECT
                    C.CourseID AS [Course ID],
                    C.CourseCode AS [Code],
                    C.CourseName AS [Name],
                    C.Description,
                    C.Credits,
                    D.DepartmentName AS [Department],
                    ISNULL(S.TermName, 'N/A') AS [Semester],
                    C.Status
                FROM Courses C
                INNER JOIN Departments D ON C.DepartmentID = D.DepartmentID
                LEFT JOIN Semesters S ON C.SemesterID = S.SemesterID
                WHERE C.Status = 'Active'
                ORDER BY C.CourseID DESC";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(sqlQuery, conn);
                    DataTable dataTable = new DataTable();
                    dataAdapter.Fill(dataTable);

                    SetupCoursesDataGridView();
                    dtgSubjectsList.DataSource = dataTable;
                    dtgSubjectsList.AutoResizeColumns();
                    activeSubjectCount = dataTable.Rows.Count;
                    UpdateActiveCountLabel();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while loading subjects: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnStudents_Click(object sender, EventArgs e)
        {
            AdminStudent stud = new AdminStudent();
            stud.Show();
            this.Hide();
        }

        private void btnLogs_Click(object sender, EventArgs e)
        {
            Logs logs = new Logs();
            logs.Show();
            this.Hide();
        }

        private void dtgLogs_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string searchTerm = txtSearch.Text.Trim();

            if (string.IsNullOrEmpty(searchTerm))
            {
                LoadSubjectsData();
                return;
            }
            string sqlQuery = "SELECT c.CourseID AS [Course ID], c.CourseCode AS [Code], c.CourseName AS [Name], c.Description, c.Credits, " +
                "d.DepartmentName AS [Department], S.TermName AS [Semester], c.Status " +
                "FROM Courses AS c " +
                "INNER JOIN Departments AS d ON c.DepartmentID = d.DepartmentID " +
                "INNER JOIN Semesters AS S ON c.SemesterID = S.SemesterID " +
                "WHERE c.Status = 'Active' AND " +
                "(c.CourseName LIKE @searchTerm OR c.CourseCode LIKE @searchTerm OR d.DepartmentName LIKE @searchTerm OR S.TermName LIKE @searchTerm) " +
                "ORDER BY c.CourseID DESC";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(sqlQuery, conn);
                    dataAdapter.SelectCommand.Parameters.AddWithValue("@searchTerm", "%" + searchTerm + "%");

                    DataTable dataTable = new DataTable();
                    dataAdapter.Fill(dataTable);

                    SetupCoursesDataGridView();

                    dtgSubjectsList.DataSource = dataTable;

                    if (dataTable.Rows.Count == 0)
                    {
                        MessageBox.Show("No courses found matching your search criteria.", "No Results", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred during search: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void SetupCoursesDataGridView()
        {
            dtgSubjectsList.AutoGenerateColumns = false;
            dtgSubjectsList.Columns.Clear();
            dtgSubjectsList.ReadOnly = true;


            dtgSubjectsList.Columns.Add("CourseID", "Course ID");
            dtgSubjectsList.Columns.Add("CourseName", "Course Name");
            dtgSubjectsList.Columns.Add("CourseCode", "Course Code");
            dtgSubjectsList.Columns.Add("Description", "Description");
            dtgSubjectsList.Columns.Add("Credits", "Credits");
            dtgSubjectsList.Columns.Add("DepartmentName", "Department Name");
            dtgSubjectsList.Columns.Add("Semester", "Semester");
            dtgSubjectsList.Columns.Add("Status", "Status");

            dtgSubjectsList.Columns["CourseID"].DataPropertyName = "Course ID";
            dtgSubjectsList.Columns["CourseName"].DataPropertyName = "Name";
            dtgSubjectsList.Columns["CourseCode"].DataPropertyName = "Code";
            dtgSubjectsList.Columns["DepartmentName"].DataPropertyName = "Department";
            dtgSubjectsList.Columns["Credits"].DataPropertyName = "Credits";
            dtgSubjectsList.Columns["Description"].DataPropertyName = "Description";
            dtgSubjectsList.Columns["Semester"].DataPropertyName = "Semester";
            dtgSubjectsList.Columns["Status"].DataPropertyName = "Status";

            dtgSubjectsList.Columns["CourseID"].Visible = true;
            dtgSubjectsList.Columns["Status"].Visible = false;
        }

        private void AdminSubjects_Load(object sender, EventArgs e)
        {
            LoadSubjectsData();
        }

        private void btnSubjects_Click(object sender, EventArgs e)
        {
            LoadSubjectsData();
        }

        private void btnAddstudent_Click(object sender, EventArgs e)
        {
            addSubject1.Show();
        }

        private void DeleteCourse(int courseID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE Courses SET Status = 'Inactive' WHERE CourseID = @CourseID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CourseID", courseID);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                     
                        InsertLog(conn, null, null, "Delete Subject", $"Deactivated subject → ID: {courseID}");
                        MessageBox.Show("Subject deactivated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadSubjectsData();
                    }
                    else
                    {
                        MessageBox.Show("Failed to deactivate subject.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
            {
                DataGridViewRow selectedRow = null;
                if (dtgSubjectsList.SelectedRows.Count > 0)
                {
                    selectedRow = dtgSubjectsList.SelectedRows[0];
                }
                else if (dtgSubjectsList.CurrentRow != null)
                {
                    selectedRow = dtgSubjectsList.CurrentRow;
                }

                if (selectedRow == null)
                {
                    MessageBox.Show("Please select a course to delete.", "No Course Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }


                object idObj = selectedRow.Cells["CourseID"].Value;
                if (idObj == null || idObj == DBNull.Value || !int.TryParse(idObj.ToString(), out int courseID))
                {
                    MessageBox.Show("Invalid Course ID selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string courseName = selectedRow.Cells["CourseName"].Value?.ToString() ?? "(Unnamed)";


                DialogResult result = MessageBox.Show(
                  $"Are you sure you want to deactivate this course?\n\nCourse: {courseName}",
                  "Confirm Deactivation",
                  MessageBoxButtons.YesNo,
                  MessageBoxIcon.Warning);

                if (result != DialogResult.Yes) return;

                try
                {
                    DeleteCourse(courseID);

                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while deactivating course: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            EditSubjectPanel.Visible = true;
        }

        private void InsertLog(SqlConnection conn, SqlTransaction transaction, int? profileID, string action, string description)
        {
            string query = @"
        INSERT INTO Logs (ProfileID, Action, Date, Time, Description)
        VALUES (@ProfileID, @Action,
                CAST(GETDATE() AS DATE), CONVERT(VARCHAR(8), GETDATE(), 108), @Description)";

            using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@ProfileID", (object)profileID ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Action", action);
                cmd.Parameters.AddWithValue("@Description", description);
                cmd.ExecuteNonQuery();
            }
        }

        private bool PerformUpdateCourse(int courseID, string courseName, string courseCode, int credits, string description, int departmentID, int semesterID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                    UPDATE Courses
                    SET CourseName = @CourseName, 
                        CourseCode = @CourseCode,
                        Credits = @Credits,
                        Description = @Description,
                        DepartmentID = @DepartmentID,
                        SemesterID = @SemesterID
                    WHERE CourseID = @CourseID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CourseName", courseName);
                    cmd.Parameters.AddWithValue("@CourseCode", courseCode);
                    cmd.Parameters.AddWithValue("@Credits", credits);
                    cmd.Parameters.AddWithValue("@Description", description);
                    cmd.Parameters.AddWithValue("@DepartmentID", departmentID);
                    cmd.Parameters.AddWithValue("@SemesterID", semesterID);
                    cmd.Parameters.AddWithValue("@CourseID", courseID);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        
                        InsertLog(conn, null, null, "Update Subject", $"Updated subject → ID: {courseID}, Name: {courseName}, Code: {courseCode}");
                        return true;
                    }
                    else return false;
                }
            }
        }



        private bool PerformInsertCourse(string courseName, string courseCode, int credits, string description, int departmentID, int semesterID, string semesterName)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    string query = @"
                        INSERT INTO Courses (CourseName, CourseCode, Credits, Description, DepartmentID, SemesterID, Status)
                        VALUES (@CourseName, @CourseCode, @Credits, @Description, @DepartmentID, @SemesterID, 'Active')";

                    using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@CourseName", courseName);
                        cmd.Parameters.AddWithValue("@CourseCode", courseCode);
                        cmd.Parameters.AddWithValue("@Credits", credits);
                        cmd.Parameters.AddWithValue("@Description", description);
                        cmd.Parameters.AddWithValue("@DepartmentID", departmentID);
                        cmd.Parameters.AddWithValue("@SemesterID", semesterID);
                        cmd.ExecuteNonQuery();
                    }

                   
                    InsertLog(conn, transaction, null, "Add Subject", $"Added a new subject: {courseName} (Code: {courseCode}) in {semesterName}");
                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Error adding subject: " + ex.Message);
                    return false;
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            errorProvider1.Clear();
            bool isValid = true;

            string courseName = txtCourseName.Text.Trim();
            string courseCode = txtCourseCode.Text.Trim();
            string description = txtDescription.Text.Trim();
            int credits = 0;
            int departmentID = 0;
            int semesterID = 0;
            string semesterName = string.Empty;

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
            else departmentID = Convert.ToInt32(cmbDepartment.SelectedValue);

            if (cmbSemester.SelectedValue == null)
            {
                errorProvider1.SetError(cmbSemester, "Please select a semester.");
                isValid = false;
            }
            else
            {
                semesterID = Convert.ToInt32(cmbSemester.SelectedValue);
                semesterName = cmbSemester.Text;
            }

            if (!isValid)
            {
                MessageBox.Show("Please correct the highlighted errors.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool success;
            if (!string.IsNullOrEmpty(selectedCourseId) && int.TryParse(selectedCourseId, out int courseID))
            {
                success = PerformUpdateCourse(courseID, courseName, courseCode, credits, description, departmentID, semesterID);
                if (success) MessageBox.Show("Course updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                success = PerformInsertCourse(courseName, courseCode, credits, description, departmentID, semesterID, semesterName);
                if (success) MessageBox.Show("Course added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            if (success)
            {
                LoadSubjectsData();
                EditSubjectPanel.Hide();
            }
        }


        private void label22_Click(object sender, EventArgs e)
        {

            EditSubjectPanel.Hide();
        }

        

        private void dtgSubjectsList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dtgSubjectsList.Rows[e.RowIndex];

              
                object courseIdValue = row.Cells["CourseID"].Value;
                if (courseIdValue != null && courseIdValue != DBNull.Value)
                {
                    selectedCourseId = courseIdValue.ToString();
                }
                else
                {
                    selectedCourseId = string.Empty;
                }

                string courseName = row.Cells["CourseName"].Value.ToString();
                string courseCode = row.Cells["CourseCode"].Value.ToString();
                string credits = row.Cells["Credits"].Value.ToString();
                string description = row.Cells["Description"].Value.ToString();
                string department = row.Cells["DepartmentName"].Value.ToString();
                string semester = row.Cells["Semester"].Value.ToString();

                txtCourseName.Text = courseName;
                txtCourseCode.Text = courseCode;
                txtCredits.Text = credits;
                txtDescription.Text = description;


                cmbDepartment.SelectedIndex = cmbDepartment.FindStringExact(department);

                cmbSemester.SelectedIndex = cmbSemester.FindStringExact(semester);
            }

            SetButtonStates();
        }

        public static class DatabaseManager
        {
            public static DataTable GetDepartments()
            {
                DataTable dt = new DataTable();
                string query = "SELECT DepartmentID, DepartmentName FROM Departments";
                using (SqlConnection conn = new SqlConnection(Database.ConnectionString))
                {
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    da.Fill(dt);
                }
                return dt;
            }

            public static DataTable GetSemesters()
            {
                DataTable dt = new DataTable();
                string query = "SELECT SemesterID, TermName FROM Semesters ORDER BY SemesterID";
                using (SqlConnection conn = new SqlConnection(Database.ConnectionString))
                {
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    da.Fill(dt);
                }
                return dt;
            }
        }

        private void cmbDepartment_SelectedIndexChanged(object sender, EventArgs e)
            {
            
            }

        private void SetButtonStates()
        {
            bool isRowSelected = dtgSubjectsList.SelectedRows.Count > 0;
            btnUpdate.Enabled = isRowSelected;
            btnDelete.Enabled = isRowSelected;
        }

        private void btnTeachers_Click(object sender, EventArgs e)
        {
            AdminTeachers teacher= new AdminTeachers();
            teacher.Show();
            this.Hide();
        }
    }
}
