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
    public partial class AdminTeachers : Form
    {
        public AdminTeachers()
        {
            InitializeComponent();
            addTeacher1.Hide();
            LoadDepartmentsToComboBox();
            LoadTeachersData();
            SetButtonStates();
            LoadTeacherCounts();
            EditTeacherPanel.Hide();
            LoadData();
            SubjectHandledPanel.Hide();
        }

        private string selectedProfileId;
        private string selectedInstructorId;
        string mailPattern = @"^[\w\.-]+@gmail\.com$";
        string agePattern = @"^(1[0-9]{2}|[1-9]?[0-9])$";


        string connectionString = @"Data Source = canasa\SQLEXPRESS; Initial catalog = KCUnivDB; Integrated Security = true";

        private void LoadSemesters()
        {

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    string query = "SELECT SemesterID, TermName FROM Semesters ORDER BY SemesterID";

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    cmbSemester.DisplayMember = "TermName";
                    cmbSemester.ValueMember = "SemesterID";
                    cmbSemester.DataSource = dt;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading semesters:\n" + ex.Message,
                                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        private void LoadDepartmentsToComboBox()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT DepartmentName FROM Departments";
                SqlCommand cmd = new SqlCommand(query, connection);

                try
                {
                    connection.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    cmbDepartment.Items.Clear(); 

                    while (reader.Read())
                    {
                        cmbDepartment.Items.Add(reader["DepartmentName"].ToString());
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("Database error while loading departments: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public void LoadTeachersData()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT
                        I.InstructorID,
                        P.ProfileID,
                        P.FirstName,
                        P.LastName,
                        P.Age,
                        P.Gender,
                        P.Email,
                        P.Phone,
                        P.Address,
                        D.DepartmentName,
                        P.Status
                    FROM Profiles P
                    INNER JOIN Users U ON P.ProfileID = U.ProfileID
                    INNER JOIN Instructors I ON P.ProfileID = I.ProfileID
                    INNER JOIN Departments D ON I.DepartmentID = D.DepartmentID
                    WHERE U.RoleID = 2 AND P.Status != 'Inactive'
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

                    dtgTeacherList.Columns.Clear();
                    dtgTeacherList.DataSource = dt;
                    dtgTeacherList.AutoResizeColumns();

                    if (dtgTeacherList.Columns.Contains("InstructorID"))
                    {
                        dtgTeacherList.Columns["InstructorID"].Visible = true;
                    }
                    if (dtgTeacherList.Columns.Contains("ProfileID"))
                    {
                        dtgTeacherList.Columns["ProfileID"].Visible = false;
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("Database error: " + ex.Message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An unexpected error occurred: " + ex.Message);
                }
            }
        }

        private void LoadTeacherCounts()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand cmdActive = new SqlCommand("SELECT COUNT(*) FROM Profiles WHERE Status = 'Active' AND ProfileID IN (SELECT ProfileID FROM Users WHERE RoleID = 2)", connection);
                try
                {
                    connection.Open();
                    int activeCount = (int)cmdActive.ExecuteScalar();
                    lblTotalActive.Text = activeCount.ToString();
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("Database error while counting teachers: " + ex.Message);
                }
            }
        }

        private void SetButtonStates()
        {
            bool rowSelected = dtgTeacherList.SelectedRows.Count > 0;
            btnDelete.Enabled = rowSelected;
            btnUpdate.Enabled = rowSelected;
            btnApplySubject.Enabled = rowSelected;
        }

        private void btnDashboard_Click(object sender, EventArgs e)
        {
            AdminDashboard adminDashboard = new AdminDashboard();
            adminDashboard.Show();
            this.Hide();
        }

        private void btnApproval_Click(object sender, EventArgs e)
        {
            AdminApproval adminApproval = new AdminApproval();
            adminApproval.Show();
            this.Hide();
        }

        private void btnSubjects_Click(object sender, EventArgs e)
        {
            AdminSubjects adminSubjects = new AdminSubjects();
            adminSubjects.Show();
            this.Hide();
        }

        private void btnReports_Click(object sender, EventArgs e)
        {
            AdminReports adminReports = new AdminReports();
            adminReports.Show();    
            this.Hide();
        }

        private void btnLogs_Click(object sender, EventArgs e)
        {
            Logs logs = new Logs();
            logs.Show();
            this.Hide();
        }

        private void btnAddstudent_Click(object sender, EventArgs e)
        {
            addTeacher1.Show();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string searchText = txtSearch.Text;
            (dtgTeacherList.DataSource as DataTable).DefaultView.RowFilter =
                string.Format("FirstName LIKE '%{0}%' OR LastName LIKE '%{0}%' OR Gender LIKE '%{0}%'", searchText);
        }

        private void btnLogs_Click_1(object sender, EventArgs e)
        {
            Logs logs = new Logs();
            logs.Show();
            this.Hide();
        }

        private void btnApproval_Click_1(object sender, EventArgs e)
        {
            AdminApproval app = new AdminApproval();
            app.Show();
            this.Hide();
        }

        private void btnDashboard_Click_1(object sender, EventArgs e)
        {
            AdminDashboard dash = new AdminDashboard();
            dash.Show();
            this.Hide();
        }

        private void btnStudents_Click(object sender, EventArgs e)
        {
            AdminStudent stu = new AdminStudent();
            stu.Show();
            this.Hide();
        }

        private void btnReports_Click_1(object sender, EventArgs e)
        {
            AdminReports adminReports = new AdminReports();
            adminReports.Show();
            this.Hide();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (dtgTeacherList.SelectedRows.Count > 0)
                {
                    DataGridViewRow selectedRow = dtgTeacherList.SelectedRows[0];

                    string profileId = selectedRow.Cells["ProfileID"].Value.ToString();
                    string firstname = selectedRow.Cells["FirstName"].Value.ToString();

                    string currentStatus = string.Empty;
                    if (selectedRow.Cells["Status"].Value != null)
                    {
                        currentStatus = selectedRow.Cells["Status"].Value.ToString();
                    }

                    if (currentStatus.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show("This teacher is already inactive.", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    DialogResult confirmResult = MessageBox.Show($"Are you sure you want to deactivate Teacher {firstname}?", "Confirm Deactivation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (confirmResult == DialogResult.Yes)
                    {
                        string newStatus = "Inactive";
                        UpdateUserStatus(profileId, newStatus);

                        string logDescription = $"Deactivated a teacher";
                        AddLogEntry(Convert.ToInt32(profileId), "Delete Teacher", logDescription);
                    }
                }
                else
                {
                    MessageBox.Show("Please select a teacher to deactivate.", "No Teacher Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            EditTeacherPanel.Show();
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

        private void ShowSubjectsHandled(int instructorId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                string query = @"
            SELECT 
                c.CourseName,
                s.TermName,
                p.FirstName + ' ' + p.LastName AS InstructorName
            FROM InstructorSubjects isb
            INNER JOIN Courses c ON isb.CourseID = c.CourseID
            INNER JOIN Semesters s ON isb.SemesterID = s.SemesterID
            INNER JOIN Instructors i ON isb.InstructorID = i.InstructorID
            INNER JOIN Profiles p ON i.ProfileID = p.ProfileID
            WHERE isb.InstructorID = @InstructorID";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@InstructorID", instructorId);

                SqlDataReader reader = cmd.ExecuteReader();

                StringBuilder message = new StringBuilder();
                string instructorName = "";
                int count = 0;

                while (reader.Read())
                {
                    if (string.IsNullOrEmpty(instructorName))
                        instructorName = reader["InstructorName"].ToString();

                    count++;
                    message.AppendLine($"{count}. {reader["CourseName"]} – {reader["TermName"]}");
                }

                reader.Close();

                if (count == 0)
                {
                    MessageBox.Show("This instructor has no subjects assigned yet.", "Subjects Handled");
                }
                else
                {
                    MessageBox.Show(
                        $"Subjects handled by: {instructorName}\n\n{message.ToString()}",
                        "Subjects Handled",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
            }
        }


        private void dtgTeacherList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dtgTeacherList.Rows[e.RowIndex];

                selectedProfileId = row.Cells["ProfileID"].Value.ToString();

                
                selectedInstructorId = row.Cells["InstructorID"].Value.ToString();

                string firstName = row.Cells["FirstName"].Value.ToString();
                string lastName = row.Cells["LastName"].Value.ToString();
                string age = row.Cells["Age"].Value.ToString();
                string gender = row.Cells["Gender"].Value.ToString();
                string phone = row.Cells["Phone"].Value.ToString();
                string address = row.Cells["Address"].Value.ToString();
                string email = row.Cells["Email"].Value.ToString();

                object departmentValue = row.Cells["DepartmentName"].Value;
                string department = (departmentValue != DBNull.Value && departmentValue != null) ? departmentValue.ToString() : string.Empty;

                

                txtFirstname.Text = firstName;
                txtLastname.Text = lastName;
                txtAge.Text = age;
                txtPhone.Text = phone;
                txtAddress.Text = address;
                txtEmail.Text = email;
                cmbGender.Text = gender;
                cmbDepartment.Text = department;
            }

            SetButtonStates();
        }

        private void label22_Click(object sender, EventArgs e)
        {
            EditTeacherPanel.Hide();
        }

       

        private int GetDepartmentID(string departmentName)
        {
            int departmentID = -1;
            string sqlQuery = "SELECT DepartmentID FROM Departments WHERE DepartmentName = @DepartmentName";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(sqlQuery, conn);
                    cmd.Parameters.AddWithValue("@DepartmentName", departmentName);
                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        departmentID = Convert.ToInt32(result);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while getting DepartmentID: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return departmentID;
        }


        public void LoadData()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT
                        I.InstructorID,
                        P.ProfileID,
                        P.FirstName,
                        P.LastName,
                        P.Age,
                        P.Gender,
                        P.Email,
                        P.Phone,
                        P.Address,
                        D.DepartmentName,
                        P.Status
                    FROM Profiles P
                    INNER JOIN Users U ON P.ProfileID = U.ProfileID
                    INNER JOIN Instructors I ON P.ProfileID = I.ProfileID
                    INNER JOIN Departments D ON I.DepartmentID = D.DepartmentID
                    WHERE U.RoleID = 2 AND P.Status != 'Inactive'
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

                    dtgTeacherList.Columns.Clear();
                    dtgTeacherList.DataSource = dt;
                    dtgTeacherList.AutoResizeColumns();

                    if (dtgTeacherList.Columns.Contains("InstructorID"))
                    {
                        dtgTeacherList.Columns["InstructorID"].Visible = true;
                    }
                    if (dtgTeacherList.Columns.Contains("ProfileID"))
                    {
                        dtgTeacherList.Columns["ProfileID"].Visible = false;
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("Database error: " + ex.Message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An unexpected error occurred: " + ex.Message);
                }
            }

        }

        public static bool IsValid(string input, string pattern)
        {
            return Regex.IsMatch(input, pattern);
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

     
        private void btnSave_Click(object sender, EventArgs e)
        {

            errorProvider1.Clear();
            errorProvider2.Clear();
            errorProvider3.Clear();


            if (string.IsNullOrEmpty(selectedProfileId))
            {
                MessageBox.Show("Please select a teacher to update.", "No Student Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

                string originalEmail = dtgTeacherList.SelectedRows[0].Cells["Email"].Value.ToString();


                if (newEmail != originalEmail)
                {
                    if (IsEmailTaken(newEmail, selectedProfileId))
                    {
                        MessageBox.Show("This email address is already in use by another user.", "Email Invalid", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                string selectedDepartmentName = cmbDepartment.SelectedItem.ToString();

                int departmentID = GetDepartmentID(selectedDepartmentName);

                if (departmentID == -1)
                {
                    MessageBox.Show("Selected department not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                int selectedProfileID = (int)dtgTeacherList.SelectedRows[0].Cells["ProfileID"].Value;

                string sqlQuery = "UPDATE Profiles SET " +
                                  "FirstName = @FirstName, " +
                                  "LastName = @LastName, " +
                                  "Age = @Age, " +
                                  "Gender = @Gender, " +
                                  "Phone = @Phone, " +
                                  "Address = @Address, " +
                                  "Email = @Email " +
                                  "WHERE ProfileID = @profileId; " +
                                  "UPDATE Instructors SET DepartmentID = @DepartmentID WHERE ProfileID = @profileId;";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand(sqlQuery, conn);

                        cmd.Parameters.AddWithValue("@FirstName", txtFirstname.Text);
                        cmd.Parameters.AddWithValue("@LastName", txtLastname.Text);
                        cmd.Parameters.AddWithValue("@Age", txtAge.Text);
                        cmd.Parameters.AddWithValue("@Gender", cmbGender.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@Phone", txtPhone.Text);
                        cmd.Parameters.AddWithValue("@Address", txtAddress.Text);
                        cmd.Parameters.AddWithValue("@Email", txtEmail.Text);
                        cmd.Parameters.AddWithValue("@profileId", selectedProfileID);

                        cmd.Parameters.AddWithValue("@DepartmentID", departmentID);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {

                            string logDescription = $"Updated Teacher: {txtFirstname.Text} {txtLastname.Text}";

                            AddLogEntry(selectedProfileID, "Update Teacher", logDescription);

                            MessageBox.Show("Teacher updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadData();
                        }
                        else
                        {
                            MessageBox.Show("No records were updated. Profile not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An error occurred during the update: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show("An error occurred during the update: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void btnSubjects_Click_1(object sender, EventArgs e)
        {
            AdminSubjects sub = new AdminSubjects();
            sub.Show();
            this.Hide();
        }

        private void label25_Click(object sender, EventArgs e)
        {
            SubjectHandledPanel.Hide();
        }

        private void btnApplySubject_Click(object sender, EventArgs e)
        {
            SubjectHandledPanel.Visible = true;
            LoadSemesters();
        }


        private void LoadAvailableSubjects(int semesterID)
        {
           
                string connectionString = @"Data Source = canasa\SQLEXPRESS;
                Initial catalog = KCUnivDB; Integrated Security = true";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();

                        string query = @"
                                        SELECT 
                                            c.CourseID, 
                                            c.CourseCode, 
                                            c.CourseName, 
                                            ISNULL(d.DepartmentName, 'No Department') AS DepartmentName
                                        FROM Courses c
                                        LEFT JOIN Departments d ON c.DepartmentID = d.DepartmentID
                                        WHERE c.SemesterID = @SemesterID AND c.Status = 'Active'
                                        ORDER BY c.CourseName";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@SemesterID", semesterID);

                            SqlDataAdapter da = new SqlDataAdapter(cmd);
                            DataTable dt = new DataTable();
                            da.Fill(dt);

                          
                            dtgSubjectAvailable.Columns.Clear();
                            dtgSubjectAvailable.DataSource = dt;

                         
                            DataGridViewButtonColumn btnHandle = new DataGridViewButtonColumn();
                            btnHandle.HeaderText = "Action";
                            btnHandle.Name = "btnHandle";
                            btnHandle.Text = "Handle";
                            btnHandle.UseColumnTextForButtonValue = true;
                            dtgSubjectAvailable.Columns.Add(btnHandle);

                        
                            dtgSubjectAvailable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                            dtgSubjectAvailable.AllowUserToAddRows = false;
                            dtgSubjectAvailable.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                            dtgSubjectAvailable.ReadOnly = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading available subjects:\n" + ex.Message,
                                        "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
        }




        private void cmbSemester_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void btnApplySub_Click(object sender, EventArgs e)
        {

            if (cmbSemester.SelectedIndex == -1)
            {
                errorProvider1.SetError(cmbSemester, "Please select a semester first.");
                return;
            }

          
            if (string.IsNullOrEmpty(selectedInstructorId))
            {
                MessageBox.Show("Please select a teacher first from the list.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dtgSubjectAvailable.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a subject to assign.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int semesterID = Convert.ToInt32(cmbSemester.SelectedValue);
            int courseID = Convert.ToInt32(dtgSubjectAvailable.SelectedRows[0].Cells["CourseID"].Value);

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
            INSERT INTO InstructorSubjects (InstructorID, CourseID, SemesterID)
VALUES (@InstructorID, @CourseID, @SemesterID);
";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@SemesterID", semesterID);
                cmd.Parameters.AddWithValue("@CourseID", courseID);
                cmd.Parameters.AddWithValue("@InstructorID", selectedInstructorId);

                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("Subject successfully applied to the instructor!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            SubjectHandledPanel.Visible = false;
        }

        private void cmbSemester_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (cmbSemester.SelectedValue == null || cmbSemester.SelectedValue is DataRowView)
                return;

            int semesterID = Convert.ToInt32(cmbSemester.SelectedValue);
            LoadAvailableSubjects(semesterID);
        }

        private void dtgSubjectAvailable_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            if (dtgSubjectAvailable.Columns[e.ColumnIndex].Name == "btnHandle")
            {
                if (string.IsNullOrEmpty(selectedInstructorId))
                {
                    MessageBox.Show("Please select a teacher first from the list.",
                                    "No Instructor Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (cmbSemester.SelectedValue == null)
                {
                    MessageBox.Show("Please select a semester first.",
                                    "No Semester Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int semesterID = Convert.ToInt32(cmbSemester.SelectedValue);
                int courseID = Convert.ToInt32(dtgSubjectAvailable.Rows[e.RowIndex].Cells["CourseID"].Value);

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string checkQuery = @"
                SELECT COUNT(*) 
                FROM InstructorSubjects 
                WHERE InstructorID = @InstructorID AND CourseID = @CourseID AND SemesterID = @SemesterID";

                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@InstructorID", selectedInstructorId);
                        checkCmd.Parameters.AddWithValue("@CourseID", courseID);
                        checkCmd.Parameters.AddWithValue("@SemesterID", semesterID);

                        int exists = (int)checkCmd.ExecuteScalar();

                        if (exists > 0)
                        {
                            MessageBox.Show("This instructor already handles this subject.",
                                            "Duplicate Entry", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    string insertQuery = @"
                INSERT INTO InstructorSubjects (InstructorID, CourseID, SemesterID)
                VALUES (@InstructorID, @CourseID, @SemesterID)";
                    using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
                    {
                        insertCmd.Parameters.AddWithValue("@InstructorID", selectedInstructorId);
                        insertCmd.Parameters.AddWithValue("@CourseID", courseID);
                        insertCmd.Parameters.AddWithValue("@SemesterID", semesterID);
                        insertCmd.ExecuteNonQuery();
                    }

                    string teacherName = "";
                    string subjectName = "";

                    string fetchQuery = @"
                SELECT p.FirstName + ' ' + p.LastName AS TeacherName, c.CourseName
                FROM Instructors i
                INNER JOIN Profiles p ON i.ProfileID = p.ProfileID
                INNER JOIN Courses c ON c.CourseID = @CourseID
                WHERE i.InstructorID = @InstructorID";
                    using (SqlCommand cmdFetch = new SqlCommand(fetchQuery, conn))
                    {
                        cmdFetch.Parameters.AddWithValue("@InstructorID", selectedInstructorId);
                        cmdFetch.Parameters.AddWithValue("@CourseID", courseID);

                        using (SqlDataReader reader = cmdFetch.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                teacherName = reader["TeacherName"].ToString();
                                subjectName = reader["CourseName"].ToString();
                            }
                        }
                    }

                    try
                    {
                  
                        int adminProfileId = 1;
                        string logAction = "Assign Subject";
                        string logDescription = $"Assigned teacher {teacherName} to handle {subjectName}.";

                        string logQuery = @"
                    INSERT INTO Logs (ProfileID, Action, Description)
                    VALUES (@ProfileID, @Action, @Description)";
                        using (SqlCommand logCmd = new SqlCommand(logQuery, conn))
                        {
                            logCmd.Parameters.AddWithValue("@ProfileID", adminProfileId);
                            logCmd.Parameters.AddWithValue("@Action", logAction);
                            logCmd.Parameters.AddWithValue("@Description", logDescription);
                            logCmd.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error inserting log: " + ex.Message);
                    }

                    MessageBox.Show("Subject successfully assigned and logged!",
                                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void AdminTeachers_Load(object sender, EventArgs e)
        {
            LoadTeachersData();

            
            if (!dtgTeacherList.Columns.Contains("Details"))
            {
                DataGridViewButtonColumn btn = new DataGridViewButtonColumn();
                btn.HeaderText = "Details";
                btn.Text = "View";
                btn.Name = "Details";
                btn.UseColumnTextForButtonValue = true;
                dtgTeacherList.Columns.Add(btn);
            }


        }

        private void dtgTeacherList_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dtgTeacherList.Columns[e.ColumnIndex].Name == "Details" && e.RowIndex >= 0)
            {
                int instructorId = Convert.ToInt32(dtgTeacherList.Rows[e.RowIndex].Cells["InstructorID"].Value);
                ShowSubjectsHandled(instructorId);
            }
        }

        private void AssignSubjectToTeacher(int instructorId, int courseId, int semesterId, int adminProfileId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                SqlTransaction transaction = con.BeginTransaction();

                try
                {
                    string insertQuery = @"
                INSERT INTO InstructorSubjects (InstructorID, CourseID, SemesterID, DateAssigned)
                VALUES (@InstructorID, @CourseID, @SemesterID, GETDATE());
            ";

                    using (SqlCommand cmd = new SqlCommand(insertQuery, con, transaction))
                    {
                        cmd.Parameters.AddWithValue("@InstructorID", instructorId);
                        cmd.Parameters.AddWithValue("@CourseID", courseId);
                        cmd.Parameters.AddWithValue("@SemesterID", semesterId);
                        cmd.ExecuteNonQuery();
                    }

 
                    string teacherName = "";
                    string subjectName = "";

                    string fetchQuery = @"
                SELECT p.FirstName + ' ' + p.LastName AS TeacherName, c.CourseName
                FROM Instructors i
                INNER JOIN Profiles p ON i.ProfileID = p.ProfileID
                INNER JOIN Courses c ON c.CourseID = @CourseID
                WHERE i.InstructorID = @InstructorID;
            ";

                    using (SqlCommand cmdFetch = new SqlCommand(fetchQuery, con, transaction))
                    {
                        cmdFetch.Parameters.AddWithValue("@InstructorID", instructorId);
                        cmdFetch.Parameters.AddWithValue("@CourseID", courseId);

                        using (SqlDataReader reader = cmdFetch.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                teacherName = reader["TeacherName"].ToString();
                                subjectName = reader["CourseName"].ToString();
                            }
                        }
                    }

                   
                    string logAction = "Assign Subject";
                    string logDescription = $"Assigned teacher {teacherName} to handle {subjectName}.";

                    string logQuery = @"
                INSERT INTO Logs (ProfileID, Action, Description)
                VALUES (@ProfileID, @Action, @Description);
            ";

                    using (SqlCommand cmdLog = new SqlCommand(logQuery, con, transaction))
                    {
                        cmdLog.Parameters.AddWithValue("@ProfileID", adminProfileId); 
                        cmdLog.Parameters.AddWithValue("@Action", logAction);
                        cmdLog.Parameters.AddWithValue("@Description", logDescription);
                        cmdLog.ExecuteNonQuery();
                    }

                 
                    transaction.Commit();

                    MessageBox.Show("Teacher successfully assigned and log recorded.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Error assigning teacher: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

    }
}

