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
    public partial class AdminTeachers : Form
    {
        public AdminTeachers()
        {
            InitializeComponent();
            addTeacher1.Hide();
            LoadTeachersData();
            SetButtonStates();
            LoadTeacherCounts();
            EditTeacherPanel.Hide();
        }
        private string selectedProfileId;
        string mailPattern = @"^[\w\.-]+@gmail\.com$";
        string agePattern = @"^(1[0-9]{2}|[1-9]?[0-9])$";

        string connectionString = @"Data Source = canasa\SQLEXPRESS; Initial catalog = KCUnivDB; Integrated Security = true";


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

                    // Hide the InstructorID and ProfileID columns for security
                    if (dtgTeacherList.Columns.Contains("InstructorID"))
                    {
                        dtgTeacherList.Columns["InstructorID"].Visible = false;
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
                    string currentStatus = selectedRow.Cells["Status"].Value.ToString();

                    if (currentStatus.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show("This teacher is already inactive.", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    DialogResult confirmResult = MessageBox.Show($"Are you sure you want to deactivate Teacher {profileId}?", "Confirm Deactivation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (confirmResult == DialogResult.Yes)
                    {
                        string newStatus = "Inactive";
                        UpdateUserStatus(profileId, newStatus);
                        AddLogEntry(Convert.ToInt32(profileId), "Delete Teacher", "Deactivated a teacher");

                        LoadTeachersData();
                        LoadTeacherCounts();
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

        private void dtgTeacherList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
           
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dtgTeacherList.Rows[e.RowIndex];

                    selectedProfileId = row.Cells["ProfileID"].Value.ToString();
                    string firstName = row.Cells["FirstName"].Value.ToString();
                    string lastName = row.Cells["LastName"].Value.ToString();
                    string age = row.Cells["Age"].Value.ToString();
                    string gender = row.Cells["Gender"].Value.ToString();
                    string phone = row.Cells["Phone"].Value.ToString();
                    string address = row.Cells["Address"].Value.ToString();
                    string email = row.Cells["Email"].Value.ToString();

                  
                }
            
        }

        private void label22_Click(object sender, EventArgs e)
        {
            EditTeacherPanel.Hide();
        }
    }
}

