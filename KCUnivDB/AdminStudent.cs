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

        private void btnStudents_Click(object sender, EventArgs e)
        {
            AdminStudent stud = new AdminStudent();
            stud.Show();
            this.Hide();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            (dtgApproval.DataSource as DataTable).DefaultView.RowFilter = string.Format("FirstName LIKE '%{0}%' OR LastName LIKE '%{0}%'", txtSearch.Text);
        }

        private void btnAddstudent_Click(object sender, EventArgs e)
        {
            AdminRegister registrationForm = new AdminRegister();
            registrationForm.Show();

            LoadApprovalData();
            LoadStudentCounts();
        }
    }
}
