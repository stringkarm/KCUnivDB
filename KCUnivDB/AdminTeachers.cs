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
        }

        string connectionString = @"Data Source = canasa\SQLEXPRESS; Initial catalog = KCUnivDB; Integrated Security = true";

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

        public void LoadTeachersData()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlDataAdapter dataAdapter = new SqlDataAdapter("SELECT TeacherID, FirstName, LastName, Gender, Status FROM Teachers", connection);
                    DataTable dataTable = new DataTable();
                    dataAdapter.Fill(dataTable);

                    dtgTeacherList.DataSource = dataTable;

                    // Count active teachers
                    int activeTeachers = dataTable.AsEnumerable().Count(row => row.Field<string>("Status") == "Active");

                    // Assuming you have a Label control named lblActiveTeachersCount on your form.
                    lblTotalActive.Text = "Active Teachers: " + activeTeachers.ToString();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while loading teacher data: " + ex.Message);
                }
            }
        }
    }
}

