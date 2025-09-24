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
    public partial class Logs : Form
    {
        public Logs()
        {
            InitializeComponent();
            dtgLogs.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dtgLogs.ReadOnly = true;
            Load();
        }

        string connectionString = @"Data Source = canasa\SQLEXPRESS;
        Initial catalog = KCUnivDB; Integrated Security = true";

        private void Load()
        {

            string sqlQuery = "SELECT l.LogID, p.FirstName, p.LastName, l.Action, l.Description, l.Date, " +
                              "CONVERT(VARCHAR(8), l.Time, 100) AS Time " +
                              "FROM Logs l " +
                              "INNER JOIN Profiles p ON l.ProfileID = p.ProfileID " +
                              "INNER JOIN Users u ON p.ProfileID = u.ProfileID " +
                              "INNER JOIN Roles r ON u.RoleID = r.RoleID " +
                              "WHERE r.RoleName IN ('Student', 'Instructor') " +
                              "ORDER BY " +
                              "CASE l.Action " +
                              "WHEN 'Add Student' THEN 1 " +
                              "WHEN 'Add Teacher' THEN 2 " +
                              "WHEN 'Delete Student' THEN 3 " +
                              "WHEN 'Delete Teacher' THEN 4 " +
                              "WHEN 'Update Student' THEN 5 " +
                              "WHEN 'Update Teacher' THEN 6 " +
                              "ELSE 7 END, " +
                              "l.Date DESC, l.Time DESC";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(sqlQuery, conn);
                    DataTable dataTable = new DataTable();
                    dataAdapter.Fill(dataTable);

                    dtgLogs.DataSource = dataTable;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while loading logs: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            string searchTerm = txtSearch.Text.Trim();

            if (string.IsNullOrEmpty(searchTerm))
            {
                Load();
                return;
            }

            string sqlQuery = "SELECT l.LogID, p.FirstName, p.LastName, l.Action, l.Description, l.Date, " +
                              "CONVERT(VARCHAR(8), l.Time, 100) AS Time " +
                              "FROM Logs l " +
                              "INNER JOIN Profiles p ON l.ProfileID = p.ProfileID ";

            if (int.TryParse(searchTerm, out int numericSearchTerm))
            {
                sqlQuery += "WHERE l.LogID = @searchTerm";
            }
            else if (DateTime.TryParse(searchTerm, out DateTime dateValue))
            {
                sqlQuery += "WHERE l.Date = @searchTerm";
            }
            else
            {
                sqlQuery += "WHERE p.FirstName LIKE @searchTerm OR p.LastName LIKE @searchTerm OR l.Action LIKE @searchTerm OR l.Description LIKE @searchTerm";
            }

            sqlQuery += " ORDER BY " +
                        "CASE l.Action " +
                        "WHEN 'Add Student' THEN 1 " +
                        "WHEN 'Delete Student' THEN 2 " +
                        "WHEN 'Update Student' THEN 3 " +
                        "WHEN 'Add Teacher' THEN 4 " +
                        "WHEN 'Delete Teacher' THEN 5 " +
                        "WHEN 'Update Teacher' THEN 6 " +
                        "ELSE 7 END, " +
                        "l.Date DESC, l.Time DESC";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(sqlQuery, conn);

                    if (int.TryParse(searchTerm, out int numericValue))
                    {
                        dataAdapter.SelectCommand.Parameters.AddWithValue("@searchTerm", numericValue);
                    }
                    else if (DateTime.TryParse(searchTerm, out DateTime date))
                    {
                        dataAdapter.SelectCommand.Parameters.AddWithValue("@searchTerm", date.Date);
                    }
                    else
                    {
                        dataAdapter.SelectCommand.Parameters.AddWithValue("@searchTerm", "%" + searchTerm + "%");
                    }

                    DataTable dataTable = new DataTable();
                    dataAdapter.Fill(dataTable);
                    dtgLogs.DataSource = dataTable;

                    if (dataTable.Rows.Count == 0)
                    {
                        MessageBox.Show("No logs found matching your search criteria.", "No Results", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred during search: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnDashboard_Click(object sender, EventArgs e)
        {
            AdminDashboard dash = new AdminDashboard();
            dash.Show();
            this.Hide();
        }

        private void btnApproval_Click(object sender, EventArgs e)
        {
            AdminApproval approval = new AdminApproval();
            approval.Show();
            this.Hide();
        }

        private void btnStudents_Click_1(object sender, EventArgs e)
        {
            AdminStudent student = new AdminStudent();
            student.Show();
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
            AdminSubjects sub = new AdminSubjects();
            sub.Show();
            this.Hide();
        }

        private void btnReports_Click(object sender, EventArgs e)
        {
            AdminReports rep = new AdminReports();
            rep.Show();
            this.Hide();
        }
    }
}
