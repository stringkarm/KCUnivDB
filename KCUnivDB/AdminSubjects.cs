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
            LoadCourses();
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

        private void LoadCourses()
        {

            string sqlQuery = "SELECT c.CourseID, c.CourseName, c.CourseCode, c.Description, c.Credits, " +
                     "p.FirstName, p.LastName, d.DepartmentName, c.Status " +
                     "FROM Courses AS c " +
                     "INNER JOIN Instructors AS i ON c.InstructorID = i.InstructorID " +
                     "INNER JOIN Profiles AS p ON i.ProfileID = p.ProfileID " +
                     "INNER JOIN Departments AS d ON c.DepartmentID = d.DepartmentID " +
                     "WHERE c.Status = 'Active' " +
                     "ORDER BY c.CourseID DESC"; 

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(sqlQuery, conn);
                    DataTable dataTable = new DataTable();
                    dataAdapter.Fill(dataTable);

                    dtgSubjectsList.DataSource = dataTable;
                    SetupCoursesDataGridView();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while loading courses: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        public void RefreshDataGrid()
        {
            LoadSubjectsData();
        }

        private void LoadSubjectsData()
        {
            string sqlQuery = @"
                SELECT
                    C.CourseID,
                    C.CourseCode AS [Code],
                    C.CourseName AS [Name],
                    C.Description,
                    C.Credits,
                    ISNULL((P.FirstName + ' ' + P.LastName), 'Unassigned') AS [Instructor], -- Use ISNULL for unassigned
                    D.DepartmentName AS [Department],
                    C.Status
                FROM Courses C
                -- Use LEFT JOINs to allow for courses without assigned instructors or departments (if applicable)
                LEFT JOIN Instructors I ON C.InstructorID = I.InstructorID
                LEFT JOIN Profiles P ON I.ProfileID = P.ProfileID
                INNER JOIN Departments D ON C.DepartmentID = D.DepartmentID
                ORDER BY C.CourseID DESC, C.Status DESC, C.CourseCode ASC"; 

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

                    if (dtgSubjectsList.Columns.Contains("CourseID"))
                    {
                        dtgSubjectsList.Columns["CourseID"].Visible = true;
                    }

                    activeSubjectCount = dataTable.AsEnumerable()
                                  .Count(row => row.Field<string>("Status") == "Active");

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
                LoadCourses();
                return;
            }

            string sqlQuery = "SELECT c.CourseID, c.CourseName, c.CourseCode, c.Description, c.Credits, " +
                     "p.FirstName, p.LastName, d.DepartmentName, c.Status " +
                     "FROM Courses AS c " +
                     "INNER JOIN Instructors AS i ON c.InstructorID = i.InstructorID " +
                     "INNER JOIN Profiles AS p ON i.ProfileID = p.ProfileID " +
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

            DataTable dataTable = (DataTable)dtgSubjectsList.DataSource;
            if (dataTable != null && !dataTable.Columns.Contains("InstructorName"))
            {
                // This line seems incorrect as it assumes FirstName and LastName exist in the DataTable directly 
                // when the table is populated by LoadCourses/LoadSubjectsData/btnSearch. 
                // The query is responsible for creating the combined Instructor name. 
                // I will comment it out as it is usually not needed when the SQL query handles the column.
                // dataTable.Columns.Add("InstructorName", typeof(string), "FirstName + ' ' + LastName");
            }

            foreach (DataGridViewColumn col in dtgSubjectsList.Columns)
            {
                // The column names in the DataGridView must match the column names in the SQL query result set.
                // Let's ensure the DataPropertyName is correctly mapped based on the query result:
                string dataFieldName = "";
                switch (col.Name)
                {
                    case "CourseID": dataFieldName = "CourseID"; break;
                    case "CourseName": dataFieldName = "CourseName"; break;
                    case "CourseCode": dataFieldName = "CourseCode"; break;
                    case "Description": dataFieldName = "Description"; break;
                    case "Credits": dataFieldName = "Credits"; break;
                    case "InstructorName":
                       
                        dataFieldName = "FirstName"; 
                        break;
                    case "DepartmentName": dataFieldName = "DepartmentName"; break;
                    case "Status": dataFieldName = "Status"; break;
                }

                if (dataTable != null && dataTable.Columns.Contains(dataFieldName))
                {
                   
                    if (dataTable.Columns.Contains(col.Name))
                    {
                        col.DataPropertyName = col.Name;
                    }
                }
            }

            
            dtgSubjectsList.Columns["CourseName"].DataPropertyName = "Name";
            dtgSubjectsList.Columns["CourseCode"].DataPropertyName = "Code";
            dtgSubjectsList.Columns["InstructorName"].DataPropertyName = "Instructor";
            dtgSubjectsList.Columns["DepartmentName"].DataPropertyName = "Department";
            dtgSubjectsList.Columns["CourseID"].DataPropertyName = "CourseID";
            dtgSubjectsList.Columns["Credits"].DataPropertyName = "Credits";
            dtgSubjectsList.Columns["Description"].DataPropertyName = "Description";
            dtgSubjectsList.Columns["Status"].DataPropertyName = "Status";

            dtgSubjectsList.Columns["CourseID"].Visible = false;
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
            string sqlCommand = "UPDATE Courses SET Status = 'Inactive' WHERE CourseID = @CourseID";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(sqlCommand, conn);
                    cmd.Parameters.AddWithValue("@CourseID", courseID);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    // You could add a success message here if desired
                    if (rowsAffected > 0)
                    {
                        // Logger.LogAction($"Course ID {courseID} soft deleted.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred during deletion: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                DialogResult result = MessageBox.Show("Are you sure you want to deactivate this course?", "Confirm Deactivation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    DeleteCourse(courseID);

                   
                    LoadCourses();

                    MessageBox.Show("Course successfully deactivated and removed from the list.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("Please select a course to delete.", "No Course Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            EditSubjectPanel.Visible = true;
        }


        private void btnSave_Click(object sender, EventArgs e)
        {

            if (dtgSubjectsList.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a course.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int courseID = Convert.ToInt32(dtgSubjectsList.SelectedRows[0].Cells["CourseID"].Value);

            string selectedDepartmentName = cmbDepartment.Text;
            string selectedInstructorName = cmbTeacherAssigned.Text;


            if (string.IsNullOrEmpty(txtCourseName.Text) || string.IsNullOrEmpty(txtCourseCode.Text) || string.IsNullOrEmpty(txtCredits.Text) || string.IsNullOrEmpty(txtDescription.Text))
            {
                MessageBox.Show("Please fill in all course detail fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(selectedDepartmentName) || string.IsNullOrEmpty(selectedInstructorName))
            {
                MessageBox.Show("Please select both a Department and an Instructor.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbDepartment.SelectedValue == null || cmbTeacherAssigned.SelectedValue == null)
            {
                
                MessageBox.Show("Selected Department or Instructor is not a valid item. Please re-select from the list.", "Data Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            
            if (!int.TryParse(txtCredits.Text, out int credits))
            {
                MessageBox.Show("Credits must be a valid number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            int departmentID = Convert.ToInt32(cmbDepartment.SelectedValue);
            int instructorID = Convert.ToInt32(cmbTeacherAssigned.SelectedValue);

            string sqlQuery = "UPDATE Courses SET " +
                              "CourseName = @CourseName, " +
                              "CourseCode = @CourseCode, " +
                              "Credits = @Credits, " +
                              "Description = @Description, " +
                              "DepartmentID = @DepartmentID, " +
                              "InstructorID = @InstructorID " +
                              "WHERE CourseID = @CourseID;";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(sqlQuery, conn);

                    cmd.Parameters.AddWithValue("@CourseID", courseID);
                    cmd.Parameters.AddWithValue("@CourseName", txtCourseName.Text.Trim());
                    cmd.Parameters.AddWithValue("@CourseCode", txtCourseCode.Text.Trim());
                    cmd.Parameters.AddWithValue("@Credits", credits); 
                    cmd.Parameters.AddWithValue("@Description", txtDescription.Text.Trim());

                    cmd.Parameters.AddWithValue("@DepartmentID", departmentID);
                    cmd.Parameters.AddWithValue("@InstructorID", instructorID);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Course details updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadSubjectsData(); 
                    }
                    else
                    {
                        MessageBox.Show("Update failed. Course not found or no changes were made.", "Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred during the update: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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

                int departmentIndex = cmbDepartment.FindStringExact(department);
                if (departmentIndex != ListBox.NoMatches)
                {
                    cmbDepartment.SelectedIndex = departmentIndex;

                    if (cmbDepartment.SelectedValue != null)
                    {
                        int selectedDepartmentID = Convert.ToInt32(cmbDepartment.SelectedValue);

                        DataTable instructorsData = DatabaseManager.GetInstructorsByDepartment(selectedDepartmentID);
                        cmbTeacherAssigned.DataSource = instructorsData;
                        cmbTeacherAssigned.DisplayMember = "FullName";
                        cmbTeacherAssigned.ValueMember = "InstructorID";

                        cmbTeacherAssigned.SelectedIndex = cmbTeacherAssigned.FindStringExact(instructor);
                    }
                }

                cmbDepartment.SelectedIndexChanged += cmbDepartment_SelectedIndexChanged;

              
                SetButtonStates();
            }

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

    }
}
