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
    public partial class AdminApproval : Form
    {

        public AdminApproval()
        {
            InitializeComponent();
            LoadApprovalData();
            LoadStudentCounts();
            SetButtonStates();
        }

        string connectionString = @"Data Source = canasa\SQLEXPRESS;
        Initial catalog = KCUnivDB; Integrated Security = true";

        private void LoadApprovalData()
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
                        P.Address,
                        P.Status
                    FROM Users U
                    INNER JOIN Profiles P ON U.ProfileID = P.ProfileID
                    WHERE U.RoleID = 3 AND (P.Status = 'Active' OR P.Status = 'Pending'); -- Only show students with Active or Pending status
                ";
                SqlCommand cmd = new SqlCommand(query, connection);

                try
                {
                    connection.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    // Clear the DataGridView before adding columns and rows
                    dtgApproval.Columns.Clear();
                    dtgApproval.Rows.Clear();

                    // Manually add the columns
                    dtgApproval.Columns.Add("ProfileID", "Profile ID");
                    dtgApproval.Columns.Add("FirstName", "First Name");
                    dtgApproval.Columns.Add("LastName", "Last Name");
                    dtgApproval.Columns.Add("Age", "Age");
                    dtgApproval.Columns.Add("Gender", "Gender");
                    dtgApproval.Columns.Add("Email", "Email");
                    dtgApproval.Columns.Add("Address", "Address");

                    // Add the custom ComboBoxColumn with only "Active" and "Pending"
                    var statusColumn = new DataGridViewComboBoxColumn();
                    statusColumn.HeaderText = "Status";
                    statusColumn.Name = "Status_Dropdown";
                    statusColumn.DataSource = new string[] { "Active", "Pending" };
                    dtgApproval.Columns.Add(statusColumn);

                    // Add data from the DataTable to the DataGridView rows
                    foreach (DataRow row in dt.Rows)
                    {
                        dtgApproval.Rows.Add(
                            row["ProfileID"],
                            row["FirstName"],
                            row["LastName"],
                            row["Age"],
                            row["Gender"],
                            row["Email"],
                            row["Address"],
                            row["Status"]
                        );
                    }

                    // Hide the ProfileID column
                    dtgApproval.Columns["ProfileID"].Visible = false;
                    dtgApproval.AutoResizeColumns();
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
                SqlCommand cmdPending = new SqlCommand("SELECT COUNT(*) FROM Profiles WHERE Status = 'Pending' AND ProfileID IN (SELECT ProfileID FROM Users WHERE RoleID = 3)", connection);

                try
                {
                    connection.Open();
                    int activeCount = (int)cmdActive.ExecuteScalar();
                    int pendingCount = (int)cmdPending.ExecuteScalar();

                    lblTotalActive.Text = activeCount.ToString();
                    lblTotalPending.Text = pendingCount.ToString();
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("Database error while counting students: " + ex.Message);
                }
            }

        }
            private void btnDashboard_Click(object sender, EventArgs e)
            {
            AdminDashboard admin = new AdminDashboard();
            admin.Show();
            this.Hide();
            }
 
            private void btnTeachers_Click(object sender, EventArgs e)
            {
                AdminTeachers teachers = new AdminTeachers();
                teachers.Show();
                this.Hide();    
            }

            private void btnSubjects_Click(object sender, EventArgs e)
            {
                AdminSubjects sub= new AdminSubjects();
                sub.Show();
                this.Hide();
            }

            private void btnReports_Click(object sender, EventArgs e)
            {
                AdminReports reports = new AdminReports();
                reports.Show();
                this.Hide();
            }

            private void btnLogs_Click(object sender, EventArgs e)
            {
                Logs logs= new Logs();
                logs.Show();    
                this.Hide();
            }

            private void btnSearch_Click(object sender, EventArgs e)
            {
            string searchText = txtSearch.Text;
            (dtgApproval.DataSource as DataTable).DefaultView.RowFilter =
                string.Format("FirstName LIKE '%{0}%' OR LastName LIKE '%{0}%' OR Gender LIKE '%{0}%'", searchText);
            }

           

        private void btnAddStudent_Click(object sender, EventArgs e)
        {
            AdminRegister registrationForm = new AdminRegister();
            registrationForm.Show();

            LoadApprovalData();
            LoadStudentCounts();
        }

        private void dtgApproval_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {


            if (e.ColumnIndex == dtgApproval.Columns["Status_Dropdown"].Index && e.RowIndex >= 0)
            {
                string newStatus = dtgApproval.Rows[e.RowIndex].Cells["Status_Dropdown"].Value.ToString();
                int profileId = Convert.ToInt32(dtgApproval.Rows[e.RowIndex].Cells["ProfileID"].Value);

                DialogResult dialogResult = MessageBox.Show($"Are you sure you want to change the status to '{newStatus}' for this student?",
                                                            "Confirm Status Change", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (dialogResult == DialogResult.Yes)
                {
                    string updateQuery = "UPDATE Profiles SET Status = @NewStatus WHERE ProfileID = @ProfileID";
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        SqlCommand cmd = new SqlCommand(updateQuery, connection);
                        cmd.Parameters.AddWithValue("@NewStatus", newStatus);
                        cmd.Parameters.AddWithValue("@ProfileID", profileId);

                        try
                        {
                            connection.Open();
                            cmd.ExecuteNonQuery();
                            MessageBox.Show("Student status updated successfully.");
                            AddLogEntry(profileId, "Updated Status", $"Changed student status to {newStatus}");
                            LoadStudentCounts();
                        }
                        catch (SqlException ex)
                        {
                            MessageBox.Show("Failed to update status: " + ex.Message);
                        }
                    }
                }
                else
                {
                    LoadApprovalData();
                }
            }
        }

     
        private void SetButtonStates()
        {
            bool rowSelected = dtgApproval.SelectedRows.Count > 0;
            
            btnDelete.Enabled = rowSelected;
            //btnUpdate.Enabled = rowSelected;
        }

        private void dtgApproval_SelectionChanged(object sender, EventArgs e)
        {
            SetButtonStates();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (dtgApproval.SelectedRows.Count > 0)
                {
                    DataGridViewRow selectedRow = dtgApproval.SelectedRows[0];
                    string profileId = selectedRow.Cells["ProfileID"].Value.ToString();

                    DialogResult confirmResult = MessageBox.Show($"Are you sure you want to deactivate Student {profileId}?", "Confirm Deactivation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (confirmResult == DialogResult.Yes)
                    {
                        string newStatus = "Inactive";
                        UpdateUserStatus(profileId, newStatus);

                        string logDescription = $"Deactivated a student";
                        AddLogEntry(Convert.ToInt32(profileId), "Delete Student", logDescription);

                        // After successfully updating the status and adding the log, reload the data
                        // This will refresh the datagridview and remove the deactivated student.
                        LoadApprovalData();
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


        private void btnAddstudent_Click_1(object sender, EventArgs e)
        {
           
        }

        private void btnApproval_Click(object sender, EventArgs e)
        {
           
        }

        private void btnStudents_Click(object sender, EventArgs e)
        {
            AdminStudent stud = new AdminStudent();
            stud.Show();
            this.Hide();
        }

        private void AdminApproval_Load(object sender, EventArgs e)
        {

        }

        private void btnDashboard_Click_1(object sender, EventArgs e)
        {
            AdminDashboard dash = new AdminDashboard();
            dash.Show();
            this.Hide();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnTeachers_Click_1(object sender, EventArgs e)
        {
            AdminTeachers teachers= new AdminTeachers();
            this.Hide();
            teachers.Show();
        }
    }
}
