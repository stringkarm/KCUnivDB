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
using System.Windows.Forms.DataVisualization.Charting;

namespace KCUnivDB
{
    public partial class AdminDashboard : Form
    {
        string connectionString = Database.ConnectionString;
        private Timer clockTimer;

        public AdminDashboard()
        {
            InitializeComponent();
            LoadStudentChart();
            LoadTeacherChart();
            LoadCount();
            adminLogout1.Hide();
            InitializeClock();
            LoadSubjectChart();
        }

        private void LoadSubjectChart()
        {
            string sqlQuery = @"
        SELECT d.DepartmentName, COUNT(c.CourseID) AS TotalSubjects
        FROM Courses c
        INNER JOIN Departments d ON c.DepartmentID = d.DepartmentID
        WHERE c.Status = 'Active'
        GROUP BY d.DepartmentName
        ORDER BY d.DepartmentName";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(sqlQuery, conn);
                    DataTable dataTable = new DataTable();
                    dataAdapter.Fill(dataTable);

                    chartSubject.Series.Clear();
                    chartSubject.ChartAreas.Clear();

                    chartSubject.ChartAreas.Add(new ChartArea("MainChart"));

                    Series series = new Series("Subjects");
                    series.ChartType = SeriesChartType.Column;
                    series.IsValueShownAsLabel = true;
                    series.Color = Color.FromArgb(200, 50, 50);

                    foreach (DataRow row in dataTable.Rows)
                    {
                        string deptName = row["DepartmentName"].ToString();
                        int subjectCount = Convert.ToInt32(row["TotalSubjects"]);
                        series.Points.AddXY(deptName, subjectCount);
                    }

                    chartSubject.Series.Add(series);

                    chartSubject.Titles.Clear();
                    chartSubject.Titles.Add("Subjects by Department");
                    chartSubject.ChartAreas["MainChart"].AxisX.Title = "Department";
                    chartSubject.ChartAreas["MainChart"].AxisY.Title = "Number of Subjects";
                    chartSubject.ChartAreas["MainChart"].AxisX.Interval = 1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while loading the subject chart: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        private void LoadTeacherChart()
        {
            string sqlQuery = "SELECT Status, COUNT(*) AS TotalCount " +
                      "FROM Profiles " +
                      "WHERE ProfileID IN (SELECT ProfileID FROM Users WHERE RoleID = (SELECT RoleID FROM Roles WHERE RoleName = 'Instructor')) " +
                      "GROUP BY Status";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(sqlQuery, conn);
                    DataTable dataTable = new DataTable();
                    dataAdapter.Fill(dataTable);

                    chartTeacher.Series.Clear();
                    chartTeacher.ChartAreas.Clear();

                    chartTeacher.ChartAreas.Add(new ChartArea("MainChart"));

                    Series series = new Series("TeacherStatus");
                    series.ChartType = SeriesChartType.Column;
                    series.IsValueShownAsLabel = true;

                    foreach (DataRow row in dataTable.Rows)
                    {
                        string status = row["Status"].ToString();
                        int count = Convert.ToInt32(row["TotalCount"]);
                        series.Points.AddXY(status, count);
                    }

                    chartTeacher.Series.Add(series);

                    chartTeacher.Titles.Clear();
                    chartTeacher.Titles.Add("Teacher");
                    chartTeacher.ChartAreas["MainChart"].AxisX.Title = "Status";
                    chartTeacher.ChartAreas["MainChart"].AxisY.Title = "Number of Teacher";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while loading the chart: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void LoadCount()
        {
            string sqlQuery_TotalStudentCount = "SELECT COUNT(p.ProfileID) " +
                                          "FROM Profiles AS p " +
                                          "INNER JOIN Users AS u ON p.ProfileID = u.ProfileID " +
                                          "INNER JOIN Roles AS r ON u.RoleID = r.RoleID " +
                                          "WHERE r.RoleName = 'Student' AND p.Status = 'Active'";

            string sqlQuery_TotalTeacherCount = "SELECT COUNT(p.ProfileID) " +
                                          "FROM Profiles AS p " +
                                          "INNER JOIN Users AS u ON p.ProfileID = u.ProfileID " +
                                          "INNER JOIN Roles AS r ON u.RoleID = r.RoleID " +
                                          "WHERE r.RoleName = 'Instructor' AND p.Status = 'Active'";

            string sqlQuery_TotalSubjectCount = "SELECT COUNT(CourseID) FROM Courses WHERE Status = 'Active'";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                SqlCommand countCmd = new SqlCommand(sqlQuery_TotalStudentCount, conn);
                int StudentCount = (int)countCmd.ExecuteScalar();
                lblStudentTotal.Text = StudentCount.ToString();

                SqlCommand countCMD = new SqlCommand(sqlQuery_TotalTeacherCount, conn);
                int TeacherCount = (int)countCMD.ExecuteScalar();
                lblTeacherTotal.Text = TeacherCount.ToString();

                SqlCommand countSub = new SqlCommand(sqlQuery_TotalSubjectCount, conn);
                int SubjectCount = (int)countSub.ExecuteScalar();
                lblSubjectTotal.Text = SubjectCount.ToString();   
            }
        }


        private void LoadStudentChart()
        {
            string sqlQuery = "SELECT Status, COUNT(*) AS TotalCount " +
                      "FROM Profiles " +
                      "WHERE ProfileID IN (SELECT ProfileID FROM Users WHERE RoleID = (SELECT RoleID FROM Roles WHERE RoleName = 'Student')) " +
                      "GROUP BY Status";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(sqlQuery, conn);
                    DataTable dataTable = new DataTable();
                    dataAdapter.Fill(dataTable);

                    chartStudentStatus.Series.Clear();
                    chartStudentStatus.ChartAreas.Clear();

                    chartStudentStatus.ChartAreas.Add(new ChartArea("MainChart"));

                    Series series = new Series("StudentStatus");
                    series.ChartType = SeriesChartType.Column;
                    series.IsValueShownAsLabel = true;

                    foreach (DataRow row in dataTable.Rows)
                    {
                        string status = row["Status"].ToString();
                        int count = Convert.ToInt32(row["TotalCount"]);
                        series.Points.AddXY(status, count);
                    }

                    chartStudentStatus.Series.Add(series);

                    chartStudentStatus.Titles.Clear();
                    chartStudentStatus.Titles.Add("Student");
                    chartStudentStatus.ChartAreas["MainChart"].AxisX.Title = "Status";
                    chartStudentStatus.ChartAreas["MainChart"].AxisY.Title = "Number of Students";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while loading the chart: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void InitializeClock()
        {
            UpdateDateTimeLabels(null, null);
            clockTimer = new Timer();
            clockTimer.Interval = 1000; 
            clockTimer.Tick += UpdateDateTimeLabels;
            clockTimer.Start();
        }

        private void UpdateDateTimeLabels(object sender, EventArgs e)
        {
            lblDate.Text = DateTime.Now.ToLongDateString();
            lblTime.Text = DateTime.Now.ToShortTimeString();
        }

        public void LoadData()
        {
            try
            {
                UpdateTotalCountLabels();
                LoadCharts();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading dashboard data: {ex.Message}", "Dashboard Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateTotalCountLabels()
        {
            lblStudentTotal.Text = DashboardManager.GetTotalCount("Students").ToString();
            lblTeacherTotal.Text = DashboardManager.GetTotalCount("Instructors").ToString();
            lblSubjectTotal.Text = DashboardManager.GetTotalCount("Courses").ToString();
        }

        private void ChartStudentsStatus()
        {

            DataTable dt = DashboardManager.GetStudentStatusCounts();

            chartTeacher.Series.Clear();
            chartTeacher.Titles.Clear();

            chartTeacher.Titles.Add("Student Active Status");

            Series series = new Series("Status")
            {
                ChartType = SeriesChartType.Pie,   
                IsValueShownAsLabel = true,         
                LegendText = "#VALX",             
                Label = "#VALY (#PERCENT)",        
            };

            series.Points.DataBind(dt.DefaultView,
                                   "Status",         
                                   "Count",        
                                   "");

            chartTeacher.Series.Add(series);

            chartTeacher.Legends[0].Enabled = true;
            chartTeacher.Legends[0].Title = "Status";
        }

        private void LoadCharts()
        {
            ChartTeachersByDepartment();
            ChartStudentsStatus();
        }

        private void ChartTeachersByDepartment()
        {
         
            DataTable dt = DashboardManager.GetTeachersByDepartment();
            chartSubject.Series.Clear();
            chartSubject.Titles.Clear();
            chartSubject.Titles.Add("Faculty Count by Department");

            Series series = new Series("Teachers")
            {
                ChartType = SeriesChartType.Column, 
                IsValueShownAsLabel = true,     
                Color = Color.FromArgb(170, 0, 0) 
            };
            series.Points.DataBind(dt.DefaultView,
                                   "DepartmentName",
                                   "TeacherCount",   
                                   "");

            chartSubject.Series.Add(series);

            chartSubject.ChartAreas[0].AxisX.Title = "Department";
            chartSubject.ChartAreas[0].AxisY.Title = "Number of Faculty";
            chartSubject.ChartAreas[0].AxisX.Interval = 1;
        }

        

        private void btnApproval_Click(object sender, EventArgs e)
        {
            AdminApproval approve = new AdminApproval();
            approve.Show();
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
            AdminReports reports = new AdminReports();
            reports.Show();
            this.Hide();
        }

        private void btnLogs_Click(object sender, EventArgs e)
        {
            Logs logs = new Logs();
            logs.Show();
            this.Hide();
        }

        private void guna2CirclePictureBox1_Click(object sender, EventArgs e)
        {
            adminLogout1.Show();
        }

        private void btnStudents_Click(object sender, EventArgs e)
        {
            AdminStudent stud = new AdminStudent();
            stud.Show();
            this.Hide();
        }

        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void lblStudentTotal_Click(object sender, EventArgs e)
        {

        }

        private void AdminDashboard_Load(object sender, EventArgs e)
        {

        }

        private void chartTeacher_Click(object sender, EventArgs e)
        {

        }
    }
}
