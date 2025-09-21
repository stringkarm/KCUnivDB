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
                    WHERE U.RoleID = 3; -- RoleID 3 is for Students
                ";
                SqlCommand cmd = new SqlCommand(query, connection);

                try
                {
                    connection.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    
                    dtgApproval.Columns.Clear();

                    var statusColumn = new DataGridViewComboBoxColumn();
                    statusColumn.HeaderText = "Status";
                    statusColumn.Name = "Status_Dropdown";

                    statusColumn.DataSource = new string[] { "Active", "Pending", "Inactive" };
                    dtgApproval.Columns.Add(statusColumn);

 
                    dtgApproval.DataSource = dt;

                    dtgApproval.Columns["Status_Dropdown"].DataPropertyName = "Status";

                    dtgApproval.AutoResizeColumns();

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
              
                SqlCommand cmdActive = new SqlCommand("SELECT COUNT(*) FROM Profiles WHERE Status = 'Active' AND ProfileID IN (SELECT ProfileID FROM Users WHERE RoleID = 3)", connection);

                SqlCommand cmdInactive = new SqlCommand("SELECT COUNT(*) FROM Profiles WHERE Status = 'Inactive' AND ProfileID IN (SELECT ProfileID FROM Users WHERE RoleID = 3)", connection);
                SqlCommand cmdPending = new SqlCommand("SELECT COUNT(*) FROM Profiles WHERE Status = 'Pending' AND ProfileID IN (SELECT ProfileID FROM Users WHERE RoleID = 3)", connection);

                try
                {
                    connection.Open();
                    int activeCount = (int)cmdActive.ExecuteScalar();
                    int inactiveCount = (int)cmdInactive.ExecuteScalar();
                    int pendingCount = (int)cmdPending.ExecuteScalar();

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

       
        //private void UpdateStudentInfo(int profileId, string firstName, string lastName, int age, string gender, string email, string address)
        //{
        //    string updateQuery = @"
        //        UPDATE Profiles 
        //        SET FirstName = @FirstName, 
        //            LastName = @LastName, 
        //            Age = @Age, 
        //            Gender = @Gender, 
        //            Email = @Email, 
        //            Address = @Address 
        //        WHERE ProfileID = @ProfileID";

        //    using (SqlConnection connection = new SqlConnection(connectionString))
        //    {
        //        SqlCommand cmd = new SqlCommand(updateQuery, connection);
        //        cmd.Parameters.AddWithValue("@FirstName", firstName);
        //        cmd.Parameters.AddWithValue("@LastName", lastName);
        //        cmd.Parameters.AddWithValue("@Age", age);
        //        cmd.Parameters.AddWithValue("@Gender", gender);
        //        cmd.Parameters.AddWithValue("@Email", email);
        //        cmd.Parameters.AddWithValue("@Address", address);
        //        cmd.Parameters.AddWithValue("@ProfileID", profileId);

        //        try
        //        {
        //            connection.Open();
        //            int rowsAffected = cmd.ExecuteNonQuery();
        //            if (rowsAffected > 0)
        //            {
        //                MessageBox.Show("Student information updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //            }
        //            else
        //            {
        //                MessageBox.Show("No changes were made.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //            }
        //        }
        //        catch (SqlException ex)
        //        {
        //            MessageBox.Show("Failed to update student information: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //        }
        //    }
        //}

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

        //private void btnUpdate_Click_1(object sender, EventArgs e)
        //{
        //    if (dtgApproval.SelectedRows.Count > 0)
        //    {
        //        DialogResult dialogResult = MessageBox.Show("Are you sure you want to save the changes for this student?", "Confirm Update", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        //        if (dialogResult == DialogResult.Yes)
        //        {
        //            // Get the data from the selected row.
        //            int profileId = Convert.ToInt32(dtgApproval.SelectedRows[0].Cells["ProfileID"].Value);
        //            string firstName = dtgApproval.SelectedRows[0].Cells["FirstName"].Value.ToString();
        //            string lastName = dtgApproval.SelectedRows[0].Cells["LastName"].Value.ToString();
        //            int age = Convert.ToInt32(dtgApproval.SelectedRows[0].Cells["Age"].Value);
        //            string gender = dtgApproval.SelectedRows[0].Cells["Gender"].Value.ToString();
        //            string email = dtgApproval.SelectedRows[0].Cells["Email"].Value.ToString();
        //            string address = dtgApproval.SelectedRows[0].Cells["Address"].Value.ToString();

        //            // Now, call the new method to update the database.
        //            UpdateStudentInfo(profileId, firstName, lastName, age, gender, email, address);

        //            // Refresh the data to show the changes.
        //            LoadApprovalData();
        //            LoadStudentCounts();
        //        }
        //    }
        //}

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dtgApproval.SelectedRows.Count > 0)
            {
                DialogResult dialogResult = MessageBox.Show(
                    "Are you sure you want to change delete this student?", "Confirm Status Change",MessageBoxButtons.YesNo,MessageBoxIcon.Warning
                );

                if (dialogResult == DialogResult.Yes)
                {
                   
                    int profileId = Convert.ToInt32(dtgApproval.SelectedRows[0].Cells["ProfileID"].Value);
                    string updateQuery = "UPDATE Profiles SET Status = 'Inactive' WHERE ProfileID = @ProfileID";

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        SqlCommand cmd = new SqlCommand(updateQuery, connection);

                        cmd.Parameters.AddWithValue("@ProfileID", profileId);

                        try
                        {
                            connection.Open();
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Students data successfully removed.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show("No changes were made. Student not found.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            LoadApprovalData();
                            LoadStudentCounts();
                        }
                        catch (SqlException ex)
                        {
                            MessageBox.Show("Failed to update student status: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
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
