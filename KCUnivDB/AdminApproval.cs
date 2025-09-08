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
            adminRegister1.Hide();

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
                // Replaced the stored procedure with a direct SQL query and added the Address column.
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
                    WHERE U.RoleID = 3; -- RoleID 3 is for Students
                ";
                SqlCommand cmd = new SqlCommand(query, connection);

                try
                {
                    connection.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    // Clear existing columns to prevent duplicates.
                    dtgApproval.Columns.Clear();

                    // Add a new column to the DataGridView for the dropdown list.
                    var statusColumn = new DataGridViewComboBoxColumn();
                    statusColumn.HeaderText = "Status";
                    statusColumn.Name = "Status_Dropdown";
                    // Add the options for the dropdown.
                    statusColumn.DataSource = new string[] { "Active", "Pending", "Inactive" };
                    dtgApproval.Columns.Add(statusColumn);

                    // Bind the fetched data to the DataGridView.
                    dtgApproval.DataSource = dt;

                    // Set the DataPropertyName for the ComboBox column to the 'Status' column from the database.
                    dtgApproval.Columns["Status_Dropdown"].DataPropertyName = "Status";

                    // Adjust column widths automatically.
                    dtgApproval.AutoResizeColumns();

                    // Hide columns you don't need to show to the admin.
                    dtgApproval.Columns["UserID"].Visible = false;
                    dtgApproval.Columns["ProfileID"].Visible = false;

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
                // Direct query for active student count
                SqlCommand cmdActive = new SqlCommand("SELECT COUNT(*) FROM Profiles WHERE Status = 'Active' AND ProfileID IN (SELECT ProfileID FROM Users WHERE RoleID = 3)", connection);
                // Direct query for inactive student count
                SqlCommand cmdInactive = new SqlCommand("SELECT COUNT(*) FROM Profiles WHERE Status = 'Inactive' AND ProfileID IN (SELECT ProfileID FROM Users WHERE RoleID = 3)", connection);
                // New direct query for pending student count
                SqlCommand cmdPending = new SqlCommand("SELECT COUNT(*) FROM Profiles WHERE Status = 'Pending' AND ProfileID IN (SELECT ProfileID FROM Users WHERE RoleID = 3)", connection);

                try
                {
                    connection.Open();
                    int activeCount = (int)cmdActive.ExecuteScalar();
                    int inactiveCount = (int)cmdInactive.ExecuteScalar();
                    int pendingCount = (int)cmdPending.ExecuteScalar();

                    // Update the labels on your form.
                    lblTotalActive.Text = activeCount.ToString();
                    lblTotalInactive.Text = inactiveCount.ToString();
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
                (dtgApproval.DataSource as DataTable).DefaultView.RowFilter = string.Format("FirstName LIKE '%{0}%' OR LastName LIKE '%{0}%'", txtSearch.Text);
            }

           

        private void btnAddStudent_Click(object sender, EventArgs e)
        {
            // Create a new instance of the registration form for admins.
            AdminRegister registrationForm = new AdminRegister();
            registrationForm.Show(); // Use ShowDialog() to block the parent form

            // Reload the data after the new student is potentially added.
            LoadApprovalData();
            LoadStudentCounts();
        }

        private void dtgApproval_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // Make sure the change happened in the "Status" column.
            if (e.ColumnIndex == dtgApproval.Columns["Status_Dropdown"].Index && e.RowIndex >= 0)
            {
                // Get the new status and the student's ProfileID.
                string newStatus = dtgApproval.Rows[e.RowIndex].Cells["Status_Dropdown"].Value.ToString();
                int profileId = Convert.ToInt32(dtgApproval.Rows[e.RowIndex].Cells["ProfileID"].Value);

                // Ask for confirmation before updating the database.
                DialogResult dialogResult = MessageBox.Show($"Are you sure you want to change the status to '{newStatus}' for this student?",
                                                            "Confirm Status Change", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (dialogResult == DialogResult.Yes)
                {
                    // Replaced the stored procedure with a direct SQL query.
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
                            LoadStudentCounts(); // Refresh the counts.
                        }
                        catch (SqlException ex)
                        {
                            MessageBox.Show("Failed to update status: " + ex.Message);
                        }
                    }
                }
                else
                {
                    // If the admin cancels, reload the data to revert the change in the UI.
                    LoadApprovalData();
                }
            }
        }

       

        // New method to handle the database update.
        private void UpdateStudentInfo(int profileId, string firstName, string lastName, int age, string gender, string email, string address)
        {
            string updateQuery = @"
                UPDATE Profiles 
                SET FirstName = @FirstName, 
                    LastName = @LastName, 
                    Age = @Age, 
                    Gender = @Gender, 
                    Email = @Email, 
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
                cmd.Parameters.AddWithValue("@Address", address);
                cmd.Parameters.AddWithValue("@ProfileID", profileId);

                try
                {
                    connection.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Student information updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        // This method enables/disables the Delete/Update buttons based on row selection.
        private void SetButtonStates()
        {
            bool rowSelected = dtgApproval.SelectedRows.Count > 0;
            
            btnDelete.Enabled = rowSelected;
            btnUpdate.Enabled = rowSelected;
        }

        private void dtgApproval_SelectionChanged(object sender, EventArgs e)
        {
            SetButtonStates();
        }

        private void btnUpdate_Click_1(object sender, EventArgs e)
        {
            if (dtgApproval.SelectedRows.Count > 0)
            {
                DialogResult dialogResult = MessageBox.Show("Are you sure you want to save the changes for this student?", "Confirm Update", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (dialogResult == DialogResult.Yes)
                {
                    // Get the data from the selected row.
                    int profileId = Convert.ToInt32(dtgApproval.SelectedRows[0].Cells["ProfileID"].Value);
                    string firstName = dtgApproval.SelectedRows[0].Cells["FirstName"].Value.ToString();
                    string lastName = dtgApproval.SelectedRows[0].Cells["LastName"].Value.ToString();
                    int age = Convert.ToInt32(dtgApproval.SelectedRows[0].Cells["Age"].Value);
                    string gender = dtgApproval.SelectedRows[0].Cells["Gender"].Value.ToString();
                    string email = dtgApproval.SelectedRows[0].Cells["Email"].Value.ToString();
                    string address = dtgApproval.SelectedRows[0].Cells["Address"].Value.ToString();

                    // Now, call the new method to update the database.
                    UpdateStudentInfo(profileId, firstName, lastName, age, gender, email, address);

                    // Refresh the data to show the changes.
                    LoadApprovalData();
                    LoadStudentCounts();
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dtgApproval.SelectedRows.Count > 0)
            {
                DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete this student?", "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (dialogResult == DialogResult.Yes)
                {
                    int profileId = Convert.ToInt32(dtgApproval.SelectedRows[0].Cells["ProfileID"].Value);

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        SqlTransaction transaction = null;
                        try
                        {
                            connection.Open();
                            transaction = connection.BeginTransaction();

                            // Delete from Users table first due to foreign key constraints.
                            string deleteUserQuery = "DELETE FROM Users WHERE ProfileID = @ProfileID";
                            SqlCommand userCmd = new SqlCommand(deleteUserQuery, connection, transaction);
                            userCmd.Parameters.AddWithValue("@ProfileID", profileId);
                            userCmd.ExecuteNonQuery();

                            // Then, delete from Profiles table.
                            string deleteProfileQuery = "DELETE FROM Profiles WHERE ProfileID = @ProfileID";
                            SqlCommand profileCmd = new SqlCommand(deleteProfileQuery, connection, transaction);
                            profileCmd.Parameters.AddWithValue("@ProfileID", profileId);
                            profileCmd.ExecuteNonQuery();

                            transaction.Commit();
                            MessageBox.Show("Student deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // Refresh the data to show the changes.
                            LoadApprovalData();
                            LoadStudentCounts();
                        }
                        catch (SqlException ex)
                        {
                            transaction?.Rollback();
                            MessageBox.Show("Failed to delete student: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }


        private void btnAddstudent_Click_1(object sender, EventArgs e)
        {
            adminRegister1.Show();
        }
    }
}
