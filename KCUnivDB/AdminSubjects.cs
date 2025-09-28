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

            SetButtonStates();

        }

        string connectionString = Database.ConnectionString;

        private int activeSubjectCount = 0;
        private int currentCourseID = -1;

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
                    C.CourseID AS [Course ID] ,
                    C.CourseCode AS [Code],
                    C.CourseName AS [Name],
                    C.Description,
                    C.Credits,
                    ISNULL((P.FirstName + ' ' + P.LastName), 'Unassigned') AS [Instructor], 
                    D.DepartmentName AS [Department],
                    C.Status
                FROM Courses C
                -- Use LEFT JOINs to allow for courses without assigned instructors or departments (if applicable)
                LEFT JOIN Instructors I ON C.InstructorID = I.InstructorID
                LEFT JOIN Profiles P ON I.ProfileID = P.ProfileID
                INNER JOIN Departments D ON C.DepartmentID = D.DepartmentID
                WHERE C.Status = 'Active' 
                ORDER BY C.CourseID DESC, C.CourseCode DESC";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(sqlQuery, conn);
                    DataTable dataTable = new DataTable();
                    dataAdapter.Fill(dataTable);

                    dtgSubjectsList.DataSource = dataTable;
                    dtgSubjectsList.AutoResizeColumns();

                    SetupCoursesDataGridView();
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

            // FIX 1: Standardize the SELECT statement to use the same aliases as LoadSubjectsData()
            // This ensures the DataGridView column mapping in SetupCoursesDataGridView works for both loading and searching.
            string sqlQuery = "SELECT c.CourseID AS [Course ID], c.CourseCode AS [Code], c.CourseName AS [Name], c.Description, c.Credits, " +
                             "ISNULL((p.FirstName + ' ' + p.LastName), 'Unassigned') AS [Instructor], " +
                             "d.DepartmentName AS [Department], c.Status " +
                             "FROM Courses AS c " +
                             "LEFT JOIN Instructors AS i ON c.InstructorID = i.InstructorID " + // Use LEFT JOIN for consistency
                             "LEFT JOIN Profiles AS p ON i.ProfileID = p.ProfileID " + // Use LEFT JOIN for consistency
                             "INNER JOIN Departments AS d ON c.DepartmentID = d.DepartmentID " +
                             "WHERE c.Status = 'Active' AND " +
                             "(c.CourseName LIKE @searchTerm OR c.CourseCode LIKE @searchTerm OR p.FirstName LIKE @searchTerm OR p.LastName LIKE @searchTerm OR d.DepartmentName LIKE @searchTerm) " +
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

                    dtgSubjectsList.DataSource = dataTable;

                    SetupCoursesDataGridView();

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
            dtgSubjectsList.Columns.Add("InstructorName", "Instructor Name");
            dtgSubjectsList.Columns.Add("DepartmentName", "Department Name");
            dtgSubjectsList.Columns.Add("Status", "Status");

            
            dtgSubjectsList.Columns["CourseID"].DataPropertyName = "Course ID";

          
            dtgSubjectsList.Columns["CourseName"].DataPropertyName = "Name";
            dtgSubjectsList.Columns["CourseCode"].DataPropertyName = "Code";
            dtgSubjectsList.Columns["InstructorName"].DataPropertyName = "Instructor";
            dtgSubjectsList.Columns["DepartmentName"].DataPropertyName = "Department";
            dtgSubjectsList.Columns["Credits"].DataPropertyName = "Credits";
            dtgSubjectsList.Columns["Description"].DataPropertyName = "Description";
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

        private void DeleteCourse(int courseID, string courseName)
        {
            string sqlCommand = "UPDATE Courses SET Status = 'Inactive' WHERE CourseID = @CourseID";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(sqlCommand, conn);
                    cmd.Parameters.AddWithValue("@CourseID", courseID);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
             
                        AddLogEntry($"Course soft deleted → ID: {courseID}, Name: {courseName}");
                    }
                    else
                    {
                        MessageBox.Show("No rows were updated. Please check if the CourseID exists.",
                            "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred during deletion: " + ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    AddLogEntry($"Error deleting course → {ex.Message}");
                }
            }
        }


        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dtgSubjectsList.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dtgSubjectsList.SelectedRows[0];

                if (selectedRow.Cells["CourseID"].Value == DBNull.Value)
                {
                    MessageBox.Show("Invalid Course ID selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                int courseID = Convert.ToInt32(selectedRow.Cells["CourseID"].Value);
                string courseName = selectedRow.Cells["CourseName"].Value.ToString();

                DialogResult result = MessageBox.Show(
                    $"Are you sure you want to deactivate this course?\n\nCourse: {courseName}",
                    "Confirm Deactivation",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    DeleteCourse(courseID, courseName);
                    LoadSubjectsData();

                    MessageBox.Show("Course successfully deactivated and removed from the list.",
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("Please select a course to delete.", "No Course Selected",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            EditSubjectPanel.Visible = true;
        }

        private void AddLogEntry(string logMessage)
        {
            string sqlQuery = "INSERT INTO Logs (LogMessage, LogTime) VALUES (@Message, @Time)";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                    {

                        cmd.Parameters.AddWithValue("@Message", logMessage);
                        cmd.Parameters.AddWithValue("@Time", DateTime.Now);

                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {

                    Console.WriteLine($"[LOGGING FAILED] Could not add log entry: '{logMessage}'. Error: {ex.Message}");
                }
            }
        }

        private bool PerformUpdateCourse(int courseID, string courseName, string courseCode, int credits, string description, int departmentID, int instructorID)
        {
            string sqlQuery = "UPDATE Courses SET " +
                              "CourseName = @CourseName, " +
                              "CourseCode = @CourseCode, " +
                              "Credits = @Credits, " +
                              "Description = @Description, " +
                              "DepartmentID = @DepartmentID, " +
                              "InstructorID = @InstructorID " +
                              "WHERE CourseID = @CourseID";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(sqlQuery, conn);

                    cmd.Parameters.AddWithValue("@CourseID", courseID); 
                    cmd.Parameters.AddWithValue("@CourseName", courseName);
                    cmd.Parameters.AddWithValue("@CourseCode", courseCode);
                    cmd.Parameters.AddWithValue("@Credits", credits);
                    cmd.Parameters.AddWithValue("@Description", description);
                    cmd.Parameters.AddWithValue("@DepartmentID", departmentID);
                    cmd.Parameters.AddWithValue("@InstructorID", instructorID);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                       
                        AddLogEntry($"Course updated → ID: {courseID}, Name: {courseName}, Code: {courseCode}");
                        return true;
                    }

                    else
                    {
                       
                        MessageBox.Show("No changes were made or Course ID not found.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while updating course: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    AddLogEntry($"Error updating course → ID: {courseID}. Error: {ex.Message}");
                    return false;
                }
            }
        }

        private bool PerformInsertCourse(string courseName, string courseCode, int credits, string description, int departmentID, int instructorID)
        {
            string sqlQuery = "INSERT INTO Courses (CourseName, CourseCode, Credits, Description, DepartmentID, InstructorID, Status) " +
                              "VALUES (@CourseName, @CourseCode, @Credits, @Description, @DepartmentID, @InstructorID, 'Active')";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(sqlQuery, conn);

                    cmd.Parameters.AddWithValue("@CourseName", courseName);
                    cmd.Parameters.AddWithValue("@CourseCode", courseCode);
                    cmd.Parameters.AddWithValue("@Credits", credits);
                    cmd.Parameters.AddWithValue("@Description", description);
                    cmd.Parameters.AddWithValue("@DepartmentID", departmentID);
                    cmd.Parameters.AddWithValue("@InstructorID", instructorID);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        AddLogEntry($"Course added → Name: {courseName}, Code: {courseCode}, Credits: {credits}, DeptID: {departmentID}, InstructorID: {instructorID}");
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("No rows were inserted. Please try again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        AddLogEntry($"Failed attempt to add course → Name: {courseName}, Code: {courseCode}");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while adding course: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    AddLogEntry($"Error adding course → {ex.Message}");
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
            int instructorID = 0;

          
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

            if (cmbTeacherAssigned.SelectedValue == null)
            {
                errorProvider1.SetError(cmbTeacherAssigned, "Please select an instructor.");
                isValid = false;
            }
            else
            {
                instructorID = Convert.ToInt32(cmbTeacherAssigned.SelectedValue);
            }

            if (!isValid)
            {
                MessageBox.Show("Please correct the highlighted errors.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                AddLogEntry("Failed attempt to save a course due to validation errors.");
                return;
            }
            // --- End Validation Block ---

            bool success = false;

            // 🌟 CRITICAL FIX: Check the selectedCourseId to decide between UPDATE and INSERT.
            if (!string.IsNullOrEmpty(selectedCourseId) && int.TryParse(selectedCourseId, out int courseID) && courseID > 0)
            {
                // Logic for UPDATE
                success = PerformUpdateCourse(courseID, courseName, courseCode, credits, description, departmentID, instructorID);

                if (success)
                {
                    MessageBox.Show("Course updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                // Logic for INSERT (Original logic)
                success = PerformInsertCourse(courseName, courseCode, credits, description, departmentID, instructorID);

                if (success)
                {
                    MessageBox.Show("Course added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            // Common clean-up after successful operation
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

        


        private string selectedCourseId;

        private void dtgSubjectsList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dtgSubjectsList.Rows[e.RowIndex];

                selectedCourseId = row.Cells["CourseID"].Value.ToString();

                string courseName = row.Cells["CourseName"].Value.ToString();
                string courseCode = row.Cells["CourseCode"].Value.ToString();
                string credits = row.Cells["Credits"].Value.ToString();
                string description = row.Cells["Description"].Value.ToString();
                string department = row.Cells["DepartmentName"].Value.ToString();
                string instructor = row.Cells["InstructorName"].Value.ToString();

                txtCourseName.Text = courseName;
                txtCourseCode.Text = courseCode;
                txtCredits.Text = credits;
                txtDescription.Text = description;

                cmbDepartment.SelectedIndexChanged -= cmbDepartment_SelectedIndexChanged;
                cmbDepartment.SelectedIndex = cmbDepartment.FindStringExact(department);
                cmbDepartment.SelectedIndexChanged += cmbDepartment_SelectedIndexChanged;

                if (cmbDepartment.SelectedValue != null)
                {
                    int selectedDepartmentID = Convert.ToInt32(cmbDepartment.SelectedValue);

                    DataTable instructorsData = DatabaseManager.GetInstructorsByDepartment(selectedDepartmentID);
                    cmbTeacherAssigned.DataSource = instructorsData;
                    cmbTeacherAssigned.DisplayMember = "FullName";
                    cmbTeacherAssigned.ValueMember = "InstructorID";
                }

                cmbTeacherAssigned.SelectedIndex = cmbTeacherAssigned.FindStringExact(instructor);
            }

            SetButtonStates();
        }

        public static class DatabaseManager
        {
            public static DataTable GetDepartments()
            {
                DataTable dataTable = new DataTable();
                string sqlQuery = "SELECT DepartmentID, DepartmentName FROM Departments";
                using (SqlConnection connection = new SqlConnection(Database.ConnectionString))
                {
                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        connection.Open();
                        SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                        dataAdapter.Fill(dataTable);
                    }
                }
                return dataTable;
            }


            public static DataTable GetInstructorsByDepartment(int departmentID)
            {
                DataTable dataTable = new DataTable();

                dataTable.Columns.Add("InstructorID", typeof(int));
                dataTable.Columns.Add("FullName", typeof(string));

                DataRow unassignedRow = dataTable.NewRow();
                unassignedRow["InstructorID"] = 0;
                unassignedRow["FullName"] = "Unassigned";
                dataTable.Rows.Add(unassignedRow);


                string sqlQuery = @"
                                            SELECT 
                                            i.InstructorID, 
                                            p.FirstName + ' ' + p.LastName AS FullName
                                            FROM 
                                            Instructors i
                                            INNER JOIN 
                                            Profiles p ON i.ProfileID = p.ProfileID
                                            WHERE 
                                            i.DepartmentID = @DepartmentID
                                            AND
                                            p.Status = 'Active';
                                            ";

                using (SqlConnection connection = new SqlConnection(Database.ConnectionString))
                {
                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@DepartmentID", departmentID);
                        try
                        {
                            connection.Open();
                            SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                            
                            dataAdapter.Fill(dataTable);
                        }
                        catch (Exception ex)
                        {



                            Console.WriteLine("Error in GetInstructorsByDepartment: " + ex.Message);

                        }
                    }
                }
                return dataTable;
            }
        }



            private void cmbDepartment_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbDepartment.SelectedValue != null && cmbDepartment.SelectedValue.ToString() != "")
            {
                try
                {
                    int selectedDepartmentID = Convert.ToInt32(cmbDepartment.SelectedValue);

                    DataTable instructorsData = DatabaseManager.GetInstructorsByDepartment(selectedDepartmentID);

                    cmbTeacherAssigned.DataSource = instructorsData;
                    cmbTeacherAssigned.DisplayMember = "FullName";
                    cmbTeacherAssigned.ValueMember = "InstructorID";
                }
                catch (InvalidCastException ex)
                {
                    Console.WriteLine("InvalidCastException: " + ex.Message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }
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
