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
        }

        private string selectedProfileId;
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
                        P.ProfileID,
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
                    
                    dtgStudentsList.Columns["UserID"].Visible = false;


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

                        // After successfully updating the status and adding the log, reload the data
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
            string connectionString = "Data Source=DESKTOP-5QHCE6M; Initial Catalog=NAVASCA_DB; Integrated Security=true";

            // SQL query to count students with 'Active' status
            string sqlQuery_TotalCount = "SELECT COUNT(p.ProfileID) " +
                                          "FROM Profiles AS p " +
                                          "INNER JOIN Users AS u ON p.ProfileID = u.ProfileID " +
                                          "INNER JOIN Roles AS r ON u.RoleID = r.RoleID " +
                                          "WHERE r.RoleName = 'Student' AND p.Status = 'Active'";

            // SQL query to load all student data for the DataGridView, sorted by status
            string sqlQuery_LoadData = "SELECT p.ProfileID, p.FirstName, p.LastName, p.Age, p.Gender, p.Phone, p.Address, p.Email, ISNULL(p.Status, 'Unknown') AS Status " +
                                       "FROM Profiles AS p " +
                                       "INNER JOIN Users AS u ON p.ProfileID = u.ProfileID " +
                                       "INNER JOIN Roles AS r ON u.RoleID = r.RoleID " +
                                       "WHERE r.RoleName IN ('Student') AND p.Status <> 'Inactive' " + // Exclude inactive users
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
                    dtgStudentsList.Columns.Add("Status", "Status");

                    DataGridViewButtonColumn btnColumn = new DataGridViewButtonColumn();
                    btnColumn.Name = "StatusActionButton";
                    btnColumn.HeaderText = "Change Status";
                    btnColumn.Text = "Approve";
                    btnColumn.UseColumnTextForButtonValue = true;
                    dtgStudentsList.Columns.Insert(9, btnColumn);

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
        }
    }
    }
    

