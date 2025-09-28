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
            LoadLogs();
        }

        string connectionString = @"Data Source = canasa\SQLEXPRESS;
        Initial catalog = KCUnivDB; Integrated Security = true";

        private void LoadLogs()
        {
            // Updated SQL Query: Now prioritizes sorting by LogID in descending order
            string sqlQuery = @"
                SELECT
                    l.LogID AS [LogID], 
                    p.FirstName AS [First Name], 
                    p.LastName AS [Last Name], 
                    l.Action AS [Action], 
                    l.Description AS [Description], 
                    l.Date AS [Date], 
                    CONVERT(VARCHAR(8), l.Time, 100) AS [Time]
                FROM Logs l 
                INNER JOIN Profiles p ON l.ProfileID = p.ProfileID 
                INNER JOIN Users u ON p.ProfileID = u.ProfileID 
                INNER JOIN Roles r ON u.RoleID = r.RoleID 
                WHERE r.RoleName IN ('Student', 'Instructor') 
                ORDER BY
                    l.LogID DESC,  -- Primary sort: show newest logs first
                    l.Date DESC, 
                    l.Time DESC;";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(sqlQuery, conn);
                    DataTable dataTable = new DataTable();
                    dataAdapter.Fill(dataTable);

                    dtgLogs.DataSource = dataTable;
                    if (dtgLogs.Columns.Contains("LogID"))
                    {
                        dtgLogs.Columns["LogID"].DisplayIndex = 0;
                    }
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
            string searchValue = txtSearch.Text.Trim();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT
                            l.LogID AS [LogID],
                            p.FirstName AS [FirstName],
                            p.LastName AS [LastName],
                            l.Action AS [Action],
                            l.Description AS [Description],
                            l.Date AS [Date],
                            CONVERT(VARCHAR(8), l.Time, 100) AS [Time]
                        FROM Logs l
                        INNER JOIN Profiles p ON l.ProfileID = p.ProfileID
                        INNER JOIN Users u ON p.ProfileID = u.ProfileID
                        INNER JOIN Roles r ON u.RoleID = r.RoleID
                        WHERE r.RoleName IN ('Student', 'Instructor')
                          AND (
                                l.LogID LIKE @search OR
                                p.FirstName LIKE @search OR
                                p.LastName LIKE @search OR
                                l.Action LIKE @search OR
                                l.Description LIKE @search OR
                                CONVERT(VARCHAR, l.Date, 23) LIKE @search OR
                                CONVERT(VARCHAR(8), l.Time, 100) LIKE @search
                              )
                        -- Search already uses LogID DESC
                        ORDER BY l.LogID DESC;";

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    da.SelectCommand.Parameters.AddWithValue("@search", "%" + searchValue + "%");

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dtgLogs.DataSource = dt;
                    dtgLogs.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

                    // force LogID to be the first column
                    if (dtgLogs.Columns.Contains("LogID"))
                    {
                        dtgLogs.Columns["LogID"].DisplayIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while searching logs: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
