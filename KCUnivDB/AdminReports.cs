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

using System.IO;

using iTextSharp.text;
using iTextSharp.text.pdf;

namespace KCUnivDB
{
    public partial class AdminReports : Form
    {
        public AdminReports()
        {
            InitializeComponent();

            this.Load += AdminReports_Load;
            cmbReportType.SelectedIndexChanged += cmbReportType_SelectedIndexChanged;
            btnGenerateReport.Click += btnGenerateReport_Click;
            btnExportPdf.Click += btnExportPdf_Click;
        }

        private const string connectionString = @"Data Source=canasa\SQLEXPRESS; Initial Catalog=KCUnivDB; Integrated Security=true";
        private DataTable parameterData = new DataTable();

        private void ExecuteReportQuery(string sqlQuery, SqlParameter[] parameters = null)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(sqlQuery, connection))
            {
                if (parameters != null)
                    command.Parameters.AddRange(parameters);

                try
                {
                    connection.Open();
                    SqlDataAdapter da = new SqlDataAdapter(command);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dtgReportOutput.DataSource = dt;
                    dtgReportOutput.AutoResizeColumns();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error generating report: " + ex.Message, "Database Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
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

        private void btnTeachers_Click(object sender, EventArgs e)
        {
            AdminTeachers teachers = new AdminTeachers();
            teachers.Show();
            this.Hide();
        }

        private void btnSubjects_Click(object sender, EventArgs e)
        {
            AdminSubjects teachersSubjects = new AdminSubjects();
            teachersSubjects.Show();
            this.Hide();
        }

        private void btnLogs_Click(object sender, EventArgs e)
        {
            Logs logs = new Logs();
            logs.Show();
            this.Hide();
        }

        private void btnApproval_Click_1(object sender, EventArgs e)
        {
            AdminStudent stud = new AdminStudent();
            stud.Show();
            this.Hide();
        }

        private void btnDashboard_Click_1(object sender, EventArgs e)
        {
            AdminDashboard dash = new AdminDashboard();
            dash.Show();
            this.Hide();
        }

        private void btnSubjects_Click_1(object sender, EventArgs e)
        {
            AdminSubjects sub = new AdminSubjects();
            sub.Show();
            this.Hide();
        }

       

        private void btnGenerateReport_Click(object sender, EventArgs e)
        {
            if (cmbReportType.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a report type first.", "Selection Missing",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedReport = cmbReportType.SelectedItem.ToString();
            string sqlQuery = "";
            SqlParameter[] parameters = null;

            // ✅ Report 1: All Active Students (no enrollment date, no duplicates)
            if (selectedReport.StartsWith("1."))
            {
                sqlQuery = @"
                    SELECT DISTINCT 
                        P.LastName AS [Last Name],
                        P.FirstName AS [First Name],
                        P.Gender,
                        PR.ProgramName AS [Program],
                        D.DepartmentName AS [Department]
                    FROM Students S
                    INNER JOIN Profiles P ON S.ProfileID = P.ProfileID
                    INNER JOIN StudentEnrollments SE ON S.StudentID = SE.StudentID
                    INNER JOIN Programs PR ON SE.ProgramID = PR.ProgramID
                    INNER JOIN Departments D ON PR.DepartmentID = D.DepartmentID
                    WHERE P.Status = 'Active'
                    ORDER BY P.LastName, P.FirstName;";
            }

            // ✅ Report 2: All Active Teachers
            else if (selectedReport.StartsWith("2."))
            {
                sqlQuery = @"
                    SELECT 
                        P.LastName AS [Last Name], 
                        P.FirstName AS [First Name], 
                        P.Gender, 
                        D.DepartmentName AS [Department]
                    FROM Profiles P
                    INNER JOIN Instructors I ON P.ProfileID = I.ProfileID
                    LEFT JOIN Departments D ON I.DepartmentID = D.DepartmentID
                    WHERE P.Status = 'Active'
                    ORDER BY P.LastName, P.FirstName;";
            }

            // ✅ Report 3: Active Students per Subject
            else if (selectedReport.StartsWith("3."))
            {
                if (cmbParameter.SelectedValue == null)
                {
                    MessageBox.Show("Please select a Subject.", "Parameter Missing",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!int.TryParse(cmbParameter.SelectedValue.ToString(), out int courseId))
                {
                    MessageBox.Show("Invalid Subject selection.", "Parameter Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                sqlQuery = @"
                    SELECT DISTINCT
                        P.LastName AS [Last Name],
                        P.FirstName AS [First Name],
                        P.Gender,
                        PR.ProgramName AS [Program],
                        D.DepartmentName AS [Department]
                    FROM StudentEnrollments SE
                    INNER JOIN Students S ON SE.StudentID = S.StudentID
                    INNER JOIN Profiles P ON S.ProfileID = P.ProfileID
                    INNER JOIN Programs PR ON SE.ProgramID = PR.ProgramID
                    INNER JOIN Departments D ON PR.DepartmentID = D.DepartmentID
                    WHERE SE.CourseID = @CourseID AND P.Status = 'Active'
                    ORDER BY P.LastName, P.FirstName;";
                parameters = new SqlParameter[] { new SqlParameter("@CourseID", courseId) };
            }

            // ✅ Report 4: Students per Teacher
            else if (selectedReport.StartsWith("4."))
            {
                if (cmbParameter.SelectedValue == null)
                {
                    MessageBox.Show("Please select a Teacher/Instructor.", "Parameter Missing",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!int.TryParse(cmbParameter.SelectedValue.ToString(), out int instructorId))
                {
                    MessageBox.Show("Invalid Teacher selection.", "Parameter Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                sqlQuery = @"
                    SELECT DISTINCT
                        P.LastName AS [Last Name], 
                        P.FirstName AS [First Name], 
                        P.Gender, 
                        PR.ProgramName AS [Program],
                        D.DepartmentName AS [Department]
                    FROM InstructorSubjects ISUB
                    INNER JOIN Courses C ON ISUB.CourseID = C.CourseID
                    INNER JOIN StudentEnrollments SE ON SE.CourseID = C.CourseID AND SE.SemesterID = ISUB.SemesterID
                    INNER JOIN Students S ON SE.StudentID = S.StudentID
                    INNER JOIN Profiles P ON S.ProfileID = P.ProfileID
                    INNER JOIN Programs PR ON SE.ProgramID = PR.ProgramID
                    INNER JOIN Departments D ON PR.DepartmentID = D.DepartmentID
                    WHERE ISUB.InstructorID = @InstructorID AND P.Status = 'Active'
                    ORDER BY P.LastName, P.FirstName;";
                parameters = new SqlParameter[] { new SqlParameter("@InstructorID", instructorId) };
            }

            // ✅ Report 5: All Active Subjects
            else if (selectedReport.StartsWith("5."))
            {
                sqlQuery = @"
                    SELECT 
                        C.CourseCode AS [Course Code], 
                        C.CourseName AS [Course Name], 
                        C.Credits AS [Units], 
                        S.TermName AS [Semester],
                        D.DepartmentName AS [Department]
                    FROM Courses C
                    LEFT JOIN Semesters S ON C.SemesterID = S.SemesterID
                    LEFT JOIN Departments D ON C.DepartmentID = D.DepartmentID
                    WHERE C.Status = 'Active'
                    ORDER BY D.DepartmentName, C.CourseName;";
            }

            if (!string.IsNullOrEmpty(sqlQuery))
                ExecuteReportQuery(sqlQuery, parameters);
        }

        private void AdminReports_Load(object sender, EventArgs e)
        {
            cmbReportType.Items.Clear();
            cmbReportType.Items.Add("1. Print active students");
            cmbReportType.Items.Add("2. Print active teachers");
            cmbReportType.Items.Add("3. Print subjects per student");
            cmbReportType.Items.Add("4. Print students per teacher");
            cmbReportType.Items.Add("5. Print subjects");

            cmbParameter.Visible = false;
            lblParameter.Visible = false;

            if (cmbReportType.Items.Count > 0)
                cmbReportType.SelectedIndex = 0;
        }

        public void LoadCoursesIntoComboBox(ComboBox cmb)
        {
            string query = "SELECT CourseID, CourseName FROM Courses WHERE Status = 'Active' ORDER BY CourseName";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                try
                {
                    connection.Open();
                    SqlDataAdapter da = new SqlDataAdapter(command);
                    parameterData.Clear();
                    da.Fill(parameterData);

                    cmb.DataSource = parameterData;
                    cmb.DisplayMember = "CourseName";
                    cmb.ValueMember = "CourseID";
                    if (cmb.Items.Count > 0) cmb.SelectedIndex = 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading courses: " + ex.Message);
                }
            }
        }

        public void LoadTeachersIntoComboBox(ComboBox cmb)
        {
            string query = @"
                SELECT I.InstructorID, P.FirstName + ' ' + P.LastName AS FullName 
                FROM Instructors I
                JOIN Profiles P ON I.ProfileID = P.ProfileID
                WHERE P.Status = 'Active'
                ORDER BY P.LastName";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                try
                {
                    connection.Open();
                    SqlDataAdapter da = new SqlDataAdapter(command);
                    parameterData.Clear();
                    da.Fill(parameterData);

                    cmb.DataSource = parameterData;
                    cmb.DisplayMember = "FullName";
                    cmb.ValueMember = "InstructorID";
                    if (cmb.Items.Count > 0) cmb.SelectedIndex = 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading teachers: " + ex.Message);
                }
            }
        }

        private void cmbReportType_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedReport = cmbReportType.SelectedItem.ToString();
            cmbParameter.DataSource = null;
            dtgReportOutput.DataSource = null;
            cmbParameter.Visible = false;
            lblParameter.Visible = false;

            if (selectedReport.StartsWith("3."))
            {
                lblParameter.Text = "Select a Subject";
                lblParameter.Visible = true;
                cmbParameter.Visible = true;
                LoadCoursesIntoComboBox(cmbParameter);
            }
            else if (selectedReport.StartsWith("4."))
            {
                lblParameter.Text = "Select a Teacher";
                lblParameter.Visible = true;
                cmbParameter.Visible = true;
                LoadTeachersIntoComboBox(cmbParameter);
            }
        }

        private void btnExportPdf_Click(object sender, EventArgs e)
        {
            if (dtgReportOutput.Rows.Count == 0 || dtgReportOutput.DataSource == null)
            {
                MessageBox.Show("Please generate a report first before exporting.", "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                FileName = $"{cmbReportType.Text.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Document pdfDoc = new Document(PageSize.A4.Rotate(), 10f, 10f, 10f, 0f);
                    PdfWriter.GetInstance(pdfDoc, new FileStream(sfd.FileName, FileMode.Create));
                    pdfDoc.Open();

                    string reportTitle = cmbReportType.Text +
                        (cmbParameter.Visible ? " (" + cmbParameter.Text + ")" : "");

                    Paragraph title = new Paragraph(reportTitle, FontFactory.GetFont("Arial", 16, iTextSharp.text.Font.BOLD))
                    {
                        Alignment = Element.ALIGN_CENTER
                    };
                    pdfDoc.Add(title);
                    pdfDoc.Add(new Paragraph("Generated on: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                    pdfDoc.Add(Chunk.NEWLINE);

                    int visibleColumnCount = dtgReportOutput.Columns.Cast<DataGridViewColumn>()
                        .Count(col => col.Visible);
                    PdfPTable pdfTable = new PdfPTable(visibleColumnCount)
                    {
                        WidthPercentage = 100,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    pdfTable.DefaultCell.Padding = 3;

                    foreach (DataGridViewColumn column in dtgReportOutput.Columns)
                    {
                        if (column.Visible)
                        {
                            PdfPCell cell = new PdfPCell(new Phrase(column.HeaderText,
                                FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.BOLD)))
                            {
                                BackgroundColor = new BaseColor(System.Drawing.Color.LightGray)
                            };
                            pdfTable.AddCell(cell);
                        }
                    }

                    foreach (DataGridViewRow row in dtgReportOutput.Rows)
                    {
                        if (row.IsNewRow) continue;
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            if (dtgReportOutput.Columns[cell.ColumnIndex].Visible)
                                pdfTable.AddCell(new Phrase(cell.Value?.ToString() ?? "",
                                    FontFactory.GetFont("Arial", 9)));
                        }
                    }

                    pdfDoc.Add(pdfTable);
                    pdfDoc.Close();
                    MessageBox.Show("Report exported successfully to PDF!", "Export Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred during PDF export:\n" + ex.Message, "Export Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
