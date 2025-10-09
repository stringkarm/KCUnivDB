using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KCUnivDB
{
    public partial class AdminStudent : Form
    {
        public AdminStudent()
        {
            InitializeComponent();
            LoadStudentsData(); 
            SetButtonStates();
            LoadStudentCounts();
            adminRegister1.Hide();
            EditStudentPanel.Hide();

            EnrollPanel.Hide();
        }

        private string selectedProfileId;
        private string selectedStudentId = "";
        string mailPattern = @"^[\w\.-]+@gmail\.com$";
        string agePattern = @"^(1[0-9]{2}|[1-9]?[0-9])$";

        string connectionString = @"Data Source = canasa\SQLEXPRESS;
        Initial catalog = KCUnivDB; Integrated Security = true";

        public void RefreshStudentData()
        {
            LoadStudentsData();
            LoadStudentCounts();


        }

        public void LoadStudentsData()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {

                string query = @"
            SELECT
                U.UserID,
                S.StudentID,            -- *** NEW: Fetch StudentID from the Students table ***
                P.ProfileID,            -- Keep ProfileID for internal use (e.g., editing)
                P.FirstName,
                P.LastName,
                P.Age,
                P.Gender,
                P.Email,
                P.Phone,
                P.Address,
                P.Status
            FROM Profiles P
            INNER JOIN Users U ON P.ProfileID = U.ProfileID
            INNER JOIN Students S ON P.ProfileID = S.ProfileID -- *** NEW: Join the Students table ***
            -- RoleID 3 is for Student
            WHERE U.RoleID = 3 AND P.Status != 'Inactive'
            ORDER BY
                CASE P.Status
                    WHEN 'Active' THEN 1
                    WHEN 'Pending' THEN 2
                    ELSE 3
                END,
                P.ProfileID DESC;
        ";

                SqlCommand cmd = new SqlCommand(query, connection);

                try
                {
                    connection.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dtgStudentsList.Columns.Clear();
                    dtgStudentsList.DataSource = dt;
                    dtgStudentsList.AutoResizeColumns();

                    if (dtgStudentsList.Columns.Contains("UserID"))
                    {
                        dtgStudentsList.Columns["UserID"].Visible = false;
                    }
                    if (dtgStudentsList.Columns.Contains("ProfileID"))
                    {
                        dtgStudentsList.Columns["ProfileID"].Visible = false;
                    }

                    if (dtgStudentsList.Columns.Contains("StudentID"))
                    {
                        dtgStudentsList.Columns["StudentID"].HeaderText = "Student ID";
 
                        dtgStudentsList.Columns["StudentID"].DisplayIndex = 0;
                    }

                    DataGridViewButtonColumn detailsButton = new DataGridViewButtonColumn();
                    detailsButton.HeaderText = "Details";
                    detailsButton.Name = "Details";
                    detailsButton.Text = "View";
                    detailsButton.UseColumnTextForButtonValue = true;
                    dtgStudentsList.Columns.Add(detailsButton);


                    detailsButton.DisplayIndex = dtgStudentsList.Columns.Count - 1;

                }
                catch (SqlException ex)
                {
                    MessageBox.Show("Database error: " + ex.Message, "SQL Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An unexpected error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void LoadStudentCounts()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand cmdActive = new SqlCommand("SELECT COUNT(*) FROM Profiles WHERE Status = 'Active' AND ProfileID IN (SELECT ProfileID FROM Users WHERE RoleID = 3)", connection);
                try
                {
                    connection.Open();
                    int activeCount = (int)cmdActive.ExecuteScalar();
                    lblTotalActive.Text = activeCount.ToString();
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("Database error while counting students: " + ex.Message);
                }
            }
        }

        private void SetButtonStates()
        {
            bool rowSelected = dtgStudentsList.SelectedRows.Count > 0;
            btnDelete.Enabled = rowSelected;
            btnUpdate.Enabled = rowSelected;
            btnEnrollPage.Enabled = rowSelected;
        }



        private void btnSearch_Click(object sender, EventArgs e)
        {
            string searchText = txtSearch.Text;
            (dtgStudentsList.DataSource as DataTable).DefaultView.RowFilter =
                string.Format("FirstName LIKE '%{0}%' OR LastName LIKE '%{0}%' OR Gender LIKE '%{0}%'", searchText);
        }

        private void btnAddstudent_Click(object sender, EventArgs e)
        {
            adminRegister1.Show();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (dtgStudentsList.SelectedRows.Count > 0)
                {
                    DataGridViewRow selectedRow = dtgStudentsList.SelectedRows[0];

                    string profileId = selectedRow.Cells["ProfileID"].Value.ToString();

                    string currentStatus = string.Empty;
                    if (selectedRow.Cells["Status"].Value != null)
                    {
                        currentStatus = selectedRow.Cells["Status"].Value.ToString();
                    }

                    if (currentStatus.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show("This student is already inactive.", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    DialogResult confirmResult = MessageBox.Show($"Are you sure you want to deactivate Student {profileId}?", "Confirm Deactivation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (confirmResult == DialogResult.Yes)
                    {
                        string newStatus = "Inactive";
                        UpdateUserStatus(profileId, newStatus);

                        string logDescription = $"Deactivated a student";
                        AddLogEntry(Convert.ToInt32(profileId), "Delete Student", logDescription);

                        LoadStudentsData();
                        LoadStudentCounts();
                    }
                }
                else
                {
                    MessageBox.Show("Please select a student to deactivate.", "No Student Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateUserStatus(string profileId, string newStatus)
        {
            string updateQuery = "UPDATE Profiles SET Status = @Status WHERE ProfileID = @ProfileID";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(updateQuery, connection);
                cmd.Parameters.AddWithValue("@Status", newStatus);
                cmd.Parameters.AddWithValue("@ProfileID", profileId);

                try
                {
                    connection.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("Database error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public void UpdateStudentInfo(int profileId, string firstName, string lastName, int age, string gender, string email, string phone, string address)
        {
            string updateQuery = @"
                UPDATE Profiles 
                SET FirstName = @FirstName, 
                    LastName = @LastName, 
                    Age = @Age, 
                    Gender = @Gender, 
                    Email = @Email, 
                    Phone = @Phone,
                    Address = @Address 
                WHERE ProfileID = @ProfileID";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(updateQuery, connection);
                cmd.Parameters.AddWithValue("@FirstName", firstName);
                cmd.Parameters.AddWithValue("@LastName", lastName);
                cmd.Parameters.AddWithValue("@Age", age);
                cmd.Parameters.AddWithValue("@Gender", gender);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Phone", phone);
                cmd.Parameters.AddWithValue("@Address", address);
                cmd.Parameters.AddWithValue("@ProfileID", profileId);

                try
                {
                    connection.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        if (MessageBox.Show("Student information updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)==DialogResult.OK)
                        {
                            EditStudentPanel.Hide();
                        }
                     
                    }
                    else
                    {
                        MessageBox.Show("No changes were made.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("Failed to update student information: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
       


        private void btnUpdate_Click(object sender, EventArgs e)
        {
 
                    EditStudentPanel.Show();
              
        }

        private void AddLogEntry(int profileID, string action, string description)
        {

            string sqlQuery = "INSERT INTO Logs (ProfileID, Action, Description) VALUES (@profileId, @action, @description)";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@profileId", profileID);
                    cmd.Parameters.AddWithValue("@action", action);
                    cmd.Parameters.AddWithValue("@description", description);

                    try
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error logging action: " + ex.Message);
                    }
                }
            }
        }

        private void btnApproval_Click(object sender, EventArgs e)
        {
            AdminApproval adminApproval = new AdminApproval();
            adminApproval.Show();
            this.Hide();
        }

        private void adminRegister1_Load(object sender, EventArgs e)
        {

        }

        private void dtgStudentsList_SelectionChanged(object sender, EventArgs e)
        {
            SetButtonStates();
        }

        private void btnDashboard_Click(object sender, EventArgs e)
        {
            AdminDashboard dash = new AdminDashboard();
            dash.Show();
            this.Hide();
        }

        private void guna2Shapes4_Click(object sender, EventArgs e)
        {

        }

        private void btnTeachers_Click(object sender, EventArgs e)
        {
            AdminTeachers teachers = new AdminTeachers();
            this.Hide();
            teachers.Show();
        }

        private void btnLogs_Click(object sender, EventArgs e)
        {
            Logs logs = new Logs();
            logs.Show();
            this.Hide();
        }

        private void dtgStudentsList_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dtgStudentsList_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                btnUpdate_Click(sender, e);
            }
        }

        private void label22_Click(object sender, EventArgs e)
        {
            EditStudentPanel.Hide();
        }

        private void adminRegister1_Load_1(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void LoadData()
        {
            string connectionString = @"Data Source = canasa\SQLEXPRESS;
            Initial catalog = KCUnivDB; Integrated Security = true";

            string sqlQuery_TotalCount = "SELECT COUNT(p.ProfileID) " +
                                          "FROM Profiles AS p " +
                                          "INNER JOIN Users AS u ON p.ProfileID = u.ProfileID " +
                                          "INNER JOIN Roles AS r ON u.RoleID = r.RoleID " +
                                          "WHERE r.RoleName = 'Student' AND p.Status = 'Active'";

            string sqlQuery_LoadData = "SELECT p.ProfileID, p.FirstName, p.LastName, p.Age, p.Gender, p.Phone, p.Address, p.Email, ISNULL(p.Status, 'Unknown') AS Status " +
                                       "FROM Profiles AS p " +
                                       "INNER JOIN Users AS u ON p.ProfileID = u.ProfileID " +
                                       "INNER JOIN Roles AS r ON u.RoleID = r.RoleID " +
                                       "WHERE r.RoleName IN ('Student') AND p.Status <> 'Inactive' " + 
                                       "ORDER BY " +
                                       "CASE p.Status " +
                                       "WHEN 'Active' THEN 1 " +
                                       "WHEN 'Pending' THEN 2 " +
                                       "ELSE 3 END";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    SqlCommand countCmd = new SqlCommand(sqlQuery_TotalCount, conn);
                    int activeStudentCount = (int)countCmd.ExecuteScalar();
                    lblTotalActive.Text = activeStudentCount.ToString();

                    SqlDataAdapter dataAdapter = new SqlDataAdapter(sqlQuery_LoadData, conn);
                    DataTable dataTable = new DataTable();
                    dataAdapter.Fill(dataTable);

                    dtgStudentsList.AutoGenerateColumns = false;
                    dtgStudentsList.Columns.Clear();
                    dtgStudentsList.ReadOnly = true;

                    dtgStudentsList.Columns.Add("ProfileID", "Profile ID");
                    dtgStudentsList.Columns.Add("FirstName", "First Name");
                    dtgStudentsList.Columns.Add("LastName", "Last Name");
                    dtgStudentsList.Columns.Add("Age", "Age");
                    dtgStudentsList.Columns.Add("Gender", "Gender");
                    dtgStudentsList.Columns.Add("Phone", "Phone");
                    dtgStudentsList.Columns.Add("Address", "Address");
                    dtgStudentsList.Columns.Add("Email", "Email");


                    foreach (DataGridViewColumn col in dtgStudentsList.Columns)
                    {
                        if (dataTable.Columns.Contains(col.Name))
                        {
                            col.DataPropertyName = col.Name;
                        }
                    }
                    dtgStudentsList.DataSource = dataTable;
                    dtgStudentsList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private bool IsEmailTaken(string email, string currentProfileId)
        {

            string sqlQuery = "SELECT COUNT(*) FROM Profiles WHERE Email = @email AND ProfileID != @currentProfileId";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@currentProfileId", currentProfileId);
                    conn.Open();
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        public static bool IsValid(string input, string pattern)
        {
            return Regex.IsMatch(input, pattern);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            errorProvider1.Clear();
            errorProvider2.Clear();
            errorProvider3.Clear();


            if (string.IsNullOrEmpty(selectedProfileId))
            {
                MessageBox.Show("Please select a student to update.", "No Student Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string firstName = txtFirstname.Text;
                string lastName = txtLastname.Text;
                string gender = cmbGender.Text;
                string address = txtAddress.Text;
                string newEmail = txtEmail.Text;
                string age = txtAge.Text;
                string phone = txtPhone.Text;


                bool allValid = true;

                if (!IsValid(newEmail, mailPattern))
                {
                    errorProvider1.SetError(txtEmail, "Please enter a valid Email.");
                    allValid = false;
                }

                if (!IsValid(age, agePattern))
                {
                    errorProvider3.SetError(txtAge, "Age is in invalid format.");
                    allValid = false;
                }

                if (!allValid)
                {
                    return;

                }

                string originalEmail = dtgStudentsList.SelectedRows[0].Cells["Email"].Value.ToString();


                if (newEmail != originalEmail)
                {
                    if (IsEmailTaken(newEmail, selectedProfileId))
                    {
                        MessageBox.Show("This email address is already in use by another user.", "Email Invalid", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }


                string sqlQuery = "UPDATE Profiles SET " +
                                  "FirstName = @firstName, " +
                                  "LastName = @lastName, " +
                                  "Age = @age, " +
                                  "Gender = @gender, " +
                                  "Phone = @phone, " +
                                  "Address = @address, " +
                                  "Email = @email " +
                                  "WHERE ProfileID = @profileId";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@firstName", firstName);
                        cmd.Parameters.AddWithValue("@lastName", lastName);
                        cmd.Parameters.AddWithValue("@age", Convert.ToInt32(age));
                        cmd.Parameters.AddWithValue("@gender", gender);
                        cmd.Parameters.AddWithValue("@phone", phone);
                        cmd.Parameters.AddWithValue("@address", address);
                        cmd.Parameters.AddWithValue("@email", newEmail);
                        cmd.Parameters.AddWithValue("@profileId", selectedProfileId);

                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Student profile updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadData();
                            EditStudentPanel.Visible = false;

                            string logDescription = $"Updated a student";
                            AddLogEntry(Convert.ToInt32(selectedProfileId), "Update Student", logDescription);
                        }
                        else
                        {
                            MessageBox.Show("No records were updated. Profile not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred during the update: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    
        private void dtgStudentsList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
           

            if (e.RowIndex >= 0)
            {
                selectedStudentId = dtgStudentsList.Rows[e.RowIndex].Cells["StudentID"].Value.ToString();
                errorProvider1.SetError(dtgStudentsList, "");
            }

            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dtgStudentsList.Rows[e.RowIndex];

                selectedProfileId = row.Cells["ProfileID"].Value.ToString();

                string firstName = row.Cells["FirstName"].Value.ToString();
                string lastName = row.Cells["LastName"].Value.ToString();
                string age = row.Cells["Age"].Value.ToString();
                string gender = row.Cells["Gender"].Value.ToString();
                string phone = row.Cells["Phone"].Value.ToString();
                string address = row.Cells["Address"].Value.ToString();
                string email = row.Cells["Email"].Value.ToString();

                txtFirstname.Text = firstName;
                txtLastname.Text = lastName;
                txtAge.Text = age;
                txtPhone.Text = phone;
                txtAddress.Text = address;
                txtEmail.Text = email;
                cmbGender.Text = gender;
            }

            if (e.RowIndex >= 0)
            {
                
                selectedStudentId = dtgStudentsList.Rows[e.RowIndex].Cells["StudentID"].Value.ToString();
              
            }

            if (dtgStudentsList.Columns[e.ColumnIndex].Name == "Details" && e.RowIndex >= 0)
            {

                string studentId = dtgStudentsList.Rows[e.RowIndex].Cells["StudentID"].Value.ToString();
                ShowStudentEnrollmentDetails(studentId);
            }

        }

       


        private void LoadSemesters()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT SemesterID, TermName FROM Semesters";
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                cmbSemester.DataSource = dt;
                cmbSemester.DisplayMember = "TermName";
                cmbSemester.ValueMember = "SemesterID";
                cmbSemester.SelectedIndex = -1;
            }
        }

        private void LoadPrograms()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT ProgramID, ProgramName FROM Programs";
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                cmbProgram.DataSource = dt;
                cmbProgram.DisplayMember = "ProgramName";
                cmbProgram.ValueMember = "ProgramID";
                cmbProgram.SelectedIndex = -1;
            }
        }

        private void EnrollStudentInCourse(string studentId, int courseId, int semesterId)
        {
            string query = @"
        INSERT INTO StudentEnrollments (StudentID, CourseID, SemesterID)
        VALUES (@StudentID, @CourseID, @SemesterID)";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@StudentID", studentId);
                    cmd.Parameters.AddWithValue("@CourseID", courseId);
                    cmd.Parameters.AddWithValue("@SemesterID", semesterId);

                    try
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                       
                        throw new Exception($"Failed to enroll student {studentId} in Course ID {courseId}: {ex.Message}");
                    }
                }
            }
        }

        private void LoadSubjectsForProgram(int programId, int semesterId)
        {
            clbSubjects.Items.Clear();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                string query = @"
            SELECT 
                c.CourseID, 
                c.CourseName, 
                c.CourseCode, 
                c.Credits
            FROM Courses c
            INNER JOIN Programs p ON c.DepartmentID = p.DepartmentID
            WHERE p.ProgramID = @ProgramID 
              AND c.SemesterID = @SemesterID
              AND c.Status = 'Active';";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@ProgramID", programId);
                    cmd.Parameters.AddWithValue("@SemesterID", semesterId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int courseId = Convert.ToInt32(reader["CourseID"]);
                            string courseName = reader["CourseName"].ToString();
                            string courseCode = reader["CourseCode"].ToString();
                            decimal credits = reader["Credits"] != DBNull.Value
                                ? Convert.ToDecimal(reader["Credits"])
                                : 0;

                 
                            string itemText = $"{courseCode} - {courseName} ({credits:F1})";

                            clbSubjects.Items.Add(new KeyValuePair<int, string>(courseId, itemText), false);
                        }
                    }
                }
            }

            clbSubjects.DisplayMember = "Value";

            if (clbSubjects.Items.Count == 0)
            {
                errorProvider1.SetError(clbSubjects, "No active subjects found for this program and semester.");
            }
            else
            {
                errorProvider1.SetError(clbSubjects, "");
            }
        }

        private bool IsStudentAlreadyEnrolled(string studentId, int courseId, int semesterId)
        {
           
            string query = @"
        SELECT COUNT(*) 
        FROM Enrollment 
        WHERE StudentID = @StudentID 
        AND CourseID = @CourseID 
        AND SemesterID = @SemesterID";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@StudentID", studentId);
                    cmd.Parameters.AddWithValue("@CourseID", courseId);
                    cmd.Parameters.AddWithValue("@SemesterID", semesterId);

                    try
                    {
                        conn.Open();
                        int count = (int)cmd.ExecuteScalar();
                        return count > 0;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error checking enrollment status: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return true;
                    }
                }
            }
        }

        private void btnEnrollPage_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedStudentId))
            {
                MessageBox.Show("Please select a student first.", "No Student Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            EnrollPanel.Show();
            LoadSemesters();
            LoadPrograms();
            clbSubjects.Items.Clear();
        }

        private void label11_Click(object sender, EventArgs e)
        {
            EnrollPanel.Hide();
        }

        private void btnEnrollStudent_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedStudentId) || cmbSemester.SelectedValue == null)
            {
                MessageBox.Show("Please ensure a student and a semester are selected.", "Missing Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int semesterId = Convert.ToInt32(cmbSemester.SelectedValue);
            int enrolledCount = 0;
            int duplicateCount = 0;
            string duplicateCourses = "";

           
            foreach (var item in clbSubjects.CheckedItems)
            {
                
                if (item is KeyValuePair<int, string> coursePair)
                {
                    int courseId = coursePair.Key;
                    string courseName = coursePair.Value; 

                   
                    if (IsStudentAlreadyEnrolled(selectedStudentId, courseId, semesterId))
                    {
                        duplicateCount++;
                        duplicateCourses += $"\n- {courseName}";
                        continue; 
                    }

                    try
                    {
                        EnrollStudentInCourse(selectedStudentId, courseId, semesterId);
                        enrolledCount++;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Enrollment failed for {courseName}: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                      
                    }
                }
            }

            string message = $"Enrollment process complete. {enrolledCount} new course(s) successfully added.";
            if (duplicateCount > 0)
            {
                message += $"\n\n🚨 Warning: {duplicateCount} course(s) were skipped because the student is already enrolled in them this semester:{duplicateCourses}";
            }

            MessageBox.Show(message, "Enrollment Status", MessageBoxButtons.OK, MessageBoxIcon.Information);

            RefreshStudentData();
            EnrollPanel.Hide();
        }

     

        private void cmbProgram_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbProgram.SelectedValue != null && cmbSemester.SelectedValue != null)
            {
                if (int.TryParse(cmbProgram.SelectedValue.ToString(), out int programId) &&
                    int.TryParse(cmbSemester.SelectedValue.ToString(), out int semesterId))
                {
                    LoadSubjectsForProgram(programId, semesterId);
                }
            }
            else
            {
                clbSubjects.Items.Clear();
                errorProvider1.SetError(clbSubjects, "");
            }
        }

        private void cmbSemester_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbProgram.SelectedValue != null && cmbSemester.SelectedValue != null)
            {
                int programId = Convert.ToInt32(cmbProgram.SelectedValue);
                int semesterId = Convert.ToInt32(cmbSemester.SelectedValue);
                LoadSubjectsForProgram(programId, semesterId);
            }
            else
            {
                clbSubjects.Items.Clear();
                errorProvider1.SetError(clbSubjects, "");
            }
        }

        private void ShowStudentEnrollmentDetails(string studentId)
        {
       
            string query = @"
        SELECT 
            se.StudentID,
            c.CourseName,
            c.CourseCode,
            sem.TermName + ' ' + sem.AcademicYear AS SemesterTerm,
            d.DepartmentName,
            ISNULL(pr.FirstName + ' ' + pr.LastName, 'Not Assigned') AS InstructorName
        FROM StudentEnrollments se
        -- FROM Enrollment se  <-- Use this line if you fully switched your enrollment logic
        INNER JOIN Courses c ON se.CourseID = c.CourseID
        INNER JOIN Semesters sem ON se.SemesterID = sem.SemesterID
        INNER JOIN Departments d ON c.DepartmentID = d.DepartmentID
        LEFT JOIN InstructorSubjects ins ON ins.CourseID = c.CourseID AND ins.SemesterID = se.SemesterID
        LEFT JOIN Instructors i ON ins.InstructorID = i.InstructorID
        LEFT JOIN Profiles pr ON i.ProfileID = pr.ProfileID
        WHERE se.StudentID = @StudentID
        ORDER BY sem.TermName, c.CourseCode;
    ";

        
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@StudentID", studentId);

                try
                {
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine($"📘 Enrollment Details for Student ID: {studentId}\n");

                        while (reader.Read())
                        {
                            sb.AppendLine($"Course: {reader["CourseCode"]} - {reader["CourseName"]}");
                            sb.AppendLine($"Term: {reader["SemesterTerm"]}");
                          
                            sb.AppendLine($"Department: {reader["DepartmentName"]}");
                            sb.AppendLine($"Instructor: {reader["InstructorName"]}");
                            sb.AppendLine(new string('-', 50));
                        }

                        reader.Close();

                        MessageBox.Show(sb.ToString(),
                                        $"Subjects Enrolled by Student {studentId}",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Information);
                    }
                  
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}


