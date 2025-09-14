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
    public partial class AdminStudent : Form
    {
        public AdminStudent()
        {
            InitializeComponent();
            LoadStudentsData();
            SetButtonStates();
            LoadStudentCounts();
            adminRegister1.Hide();
        }

        string connectionString = @"Data Source = canasa\SQLEXPRESS;
        Initial catalog = KCUnivDB; Integrated Security = true";

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

    
        private void LoadStudentsData()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
                SELECT 
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
                WHERE U.RoleID = 3 AND P.Status = 'Active'
                ORDER BY P.ProfileID DESC; -- Adjust this to order by a different column if you wish
 ";

                SqlCommand cmd = new SqlCommand(query, connection);

                try
                {
                    connection.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dtgStudentsList.DataSource = dt;
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("Database error: " + ex.Message);
                }
            }
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
            if (dtgStudentsList.SelectedRows.Count > 0)
            {
                DialogResult dialogResult = MessageBox.Show(
                    "Are you sure you want to delete this student?",
                    "Confirm Status Change",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (dialogResult == DialogResult.Yes)
                {
                    int profileId = Convert.ToInt32(dtgStudentsList.SelectedRows[0].Cells["ProfileID"].Value);
                    string updateQuery = "UPDATE Profiles SET Status = 'Inactive' WHERE ProfileID = @ProfileID";

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        SqlCommand cmd = new SqlCommand(updateQuery, connection);
                        cmd.Parameters.AddWithValue("@ProfileID", profileId);

                        try
                        {
                            connection.Open();
                            cmd.ExecuteNonQuery();
                            MessageBox.Show("You successfully deleted this student.");

                            // Refresh the DataGridView to show the updated data
                            LoadStudentsData();
                        }
                        catch (SqlException ex)
                        {
                            MessageBox.Show("Failed to update student status: " + ex.Message);
                        }
                    }
                }
            }
        }

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


                    dtgStudentsList.Columns.Clear();

                    dtgStudentsList.DataSource = dt;
                    dtgStudentsList.AutoResizeColumns();

                    dtgStudentsList.Columns["UserID"].Visible = false;
                    dtgStudentsList.Columns["ProfileID"].Visible = false;

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

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (dtgStudentsList.SelectedRows.Count > 0)
            {
                DialogResult dialogResult = MessageBox.Show("Are you sure you want to save the changes for this student?", "Confirm Update", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (dialogResult == DialogResult.Yes)
                {

                    int profileId = Convert.ToInt32(dtgStudentsList.SelectedRows[0].Cells["ProfileID"].Value);
                    string firstName = dtgStudentsList.SelectedRows[0].Cells["FirstName"].Value.ToString();
                    string lastName = dtgStudentsList.SelectedRows[0].Cells["LastName"].Value.ToString();
                    int age = Convert.ToInt32(dtgStudentsList.SelectedRows[0].Cells["Age"].Value);
                    string gender = dtgStudentsList.SelectedRows[0].Cells["Gender"].Value.ToString();
                    string email = dtgStudentsList.SelectedRows[0].Cells["Email"].Value.ToString();
                    string address = dtgStudentsList.SelectedRows[0].Cells["Address"].Value.ToString();


                    UpdateStudentInfo(profileId, firstName, lastName, age, gender, email, address);
                    LoadStudentCounts();
                    LoadApprovalData();
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
    }
    
}
