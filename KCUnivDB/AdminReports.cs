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

        private const string connectionString = @"Data Source=canasa\SQLEXPRESS; Initial catalog=KCUnivDB; Integrated Security=true";

        private DataTable parameterData = new DataTable();

        private void ExecuteReportQuery(string sqlQuery)
        {
            ExecuteReportQuery(sqlQuery, null);
        }


        private void ExecuteReportQuery(string sqlQuery, SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    try
                    {
                        connection.Open();
                        SqlDataAdapter da = new SqlDataAdapter(command);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        dtgReportOutput.DataSource = null;

                        dtgReportOutput.DataSource = dt;
                        dtgReportOutput.AutoResizeColumns();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error generating report: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
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
                MessageBox.Show("Please select a report type first.", "Selection Missing", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedReport = cmbReportType.SelectedItem.ToString();
            string sqlQuery = "";
            SqlParameter[] parameters = null;

            // --- Report 1: Active Students ---
            if (selectedReport.StartsWith("1."))
            {
                sqlQuery = "SELECT P.FirstName, P.LastName, P.Gender, P.Email, S.EnrollmentDate FROM Profiles P INNER JOIN Students S ON P.ProfileID = S.ProfileID WHERE P.Status = 'Active' ORDER BY P.LastName, P.FirstName;";
            }

            // --- Report 2: Active Teachers ---
            else if (selectedReport.StartsWith("2."))
            {
                sqlQuery = "SELECT P.FirstName, P.LastName, P.Email, P.Phone, D.DepartmentName, I.HireDate FROM Profiles P INNER JOIN Instructors I ON P.ProfileID = I.ProfileID INNER JOIN Departments D ON I.DepartmentID = D.DepartmentID WHERE P.Status = 'Active' ORDER BY P.LastName, P.FirstName;";
            }

            // **MODIFIED** Report 3: Subjects per Student (Requires Student Parameter)
            else if (selectedReport.StartsWith("3."))
            {
                if (cmbParameter.SelectedValue == null)
                {
                    MessageBox.Show("Please select a Student.", "Parameter Missing", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                // NOTE: The cmbParameter ValueMember is 'StudentID' from LoadStudentsIntoComboBox
                int studentId = Convert.ToInt32(cmbParameter.SelectedValue);

                sqlQuery = @"
                    SELECT P.FirstName, P.LastName, C.CourseCode, C.CourseName, SE.EnrollDate, SE.Grade 
                    FROM StudentEnrollments SE
                    INNER JOIN Students S ON SE.StudentID = S.StudentID
                    INNER JOIN Profiles P ON S.ProfileID = P.ProfileID
                    INNER JOIN Courses C ON SE.CourseID = C.CourseID
                    WHERE S.StudentID = @StudentID AND C.Status = 'Active' 
                    ORDER BY C.CourseName;";

                parameters = new SqlParameter[] { new SqlParameter("@StudentID", studentId) };
            }

            // --- Report 4: Students Per Teacher (Requires Teacher Parameter) ---
            else if (selectedReport.StartsWith("4."))
            {
                if (cmbParameter.SelectedValue == null)
                {
                    MessageBox.Show("Please select a Teacher/Instructor.", "Parameter Missing", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                int instructorId = Convert.ToInt32(cmbParameter.SelectedValue);

                sqlQuery = @"
                            SELECT T_P.FirstName AS TeacherFirstName, T_P.LastName AS TeacherLastName, 
                                   C.CourseName, S_P.FirstName AS StudentFirstName, S_P.LastName AS StudentLastName, SE.Grade
                            FROM InstructorSubjects ISUB
                            INNER JOIN Instructors I ON ISUB.InstructorID = I.InstructorID
                            INNER JOIN Profiles T_P ON I.ProfileID = T_P.ProfileID
                            INNER JOIN Courses C ON ISUB.CourseID = C.CourseID
                            INNER JOIN StudentEnrollments SE ON C.CourseID = SE.CourseID AND ISUB.SemesterID = SE.SemesterID
                            INNER JOIN Students S ON SE.StudentID = S.StudentID
                            INNER JOIN Profiles S_P ON S.ProfileID = S_P.ProfileID
                            WHERE ISUB.InstructorID = @InstructorID AND T_P.Status = 'Active' AND S_P.Status = 'Active' 
                            ORDER BY S_P.LastName, C.CourseName;";

                parameters = new SqlParameter[] { new SqlParameter("@InstructorID", instructorId) };
            }

            // --- Report 5: All Subjects ---
            else if (selectedReport.StartsWith("5."))
            {
                sqlQuery = "SELECT C.CourseCode, C.CourseName, C.Credits, D.DepartmentName, S.TermName AS Semester, C.Status FROM Courses C LEFT JOIN Departments D ON C.DepartmentID = D.DepartmentID LEFT JOIN Semesters S ON C.SemesterID = S.SemesterID ORDER BY S.TermName, C.CourseName;";
            }

            if (!string.IsNullOrEmpty(sqlQuery))
            {
                ExecuteReportQuery(sqlQuery, parameters);
            }
        }

        private void AdminReports_Load(object sender, EventArgs e)
        {
            cmbReportType.Items.Clear();

            cmbReportType.Items.Add("1. Print all active students");
            cmbReportType.Items.Add("2. Print all active teachers");
         
            cmbReportType.Items.Add("3. Print all subjects per student");
            cmbReportType.Items.Add("4. Print all students per teacher");
            cmbReportType.Items.Add("5. Print all subjects");
    
            cmbParameter.Visible = false;
            lblParameter.Visible = false;

            if (cmbReportType.Items.Count > 0)
            {
                cmbReportType.SelectedIndex = 0;
            }
        }

        public void LoadStudentsIntoComboBox(ComboBox cmb)
        {
            string query = @"
                            SELECT S.StudentID, P.FirstName + ' ' + P.LastName AS FullName 
                            FROM Students S
                            INNER JOIN Profiles P ON S.ProfileID = P.ProfileID
                            WHERE P.Status = 'Active'
                            ORDER BY P.LastName";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
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
                        cmb.ValueMember = "StudentID";

                        if (cmb.Items.Count > 0) cmb.SelectedIndex = 0;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading active students: " + ex.Message);
                    }
                }
            }
        }

     
        public void LoadCoursesIntoComboBox(ComboBox cmb)
        {
            string query = "SELECT CourseID, CourseName FROM Courses C WHERE C.Status = 'Active' ORDER BY CourseName";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
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
                        MessageBox.Show("Error loading active courses: " + ex.Message);
                    }
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
            {
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
                lblParameter.Text = "Select Student:";
                lblParameter.Visible = true;
                cmbParameter.Visible = true;
                LoadStudentsIntoComboBox(cmbParameter); 
            }
            else if (selectedReport.StartsWith("4."))
            {
                lblParameter.Text = "Select Teacher/Instructor:";
                lblParameter.Visible = true;
                cmbParameter.Visible = true;
                LoadTeachersIntoComboBox(cmbParameter);
            }
        }

        private void btnExportPdf_Click(object sender, EventArgs e)
        {
            if (dtgReportOutput.Rows.Count == 0 || dtgReportOutput.DataSource == null)
            {
                MessageBox.Show("Please generate a report first before exporting.", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PDF files (*.pdf)|*.pdf";
            sfd.FileName = $"{cmbReportType.Text.Replace(" ", "_")}_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.pdf";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Document pdfDoc = new Document(PageSize.A4.Rotate(), 10f, 10f, 10f, 0f);
                    PdfWriter.GetInstance(pdfDoc, new FileStream(sfd.FileName, FileMode.Create));
                    pdfDoc.Open();

                    string reportTitle = cmbReportType.Text +
                                         (cmbParameter.Visible ? " (" + cmbParameter.Text + ")" : "");

                    Paragraph title = new Paragraph(reportTitle, FontFactory.GetFont("Arial", 16, iTextSharp.text.Font.BOLD));
                    title.Alignment = Element.ALIGN_CENTER;
                    pdfDoc.Add(title);
                    pdfDoc.Add(new Paragraph("Generated on: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                    pdfDoc.Add(Chunk.NEWLINE);

                    // --- Create PDF Table ---
                    int visibleColumnCount = 0;
                    foreach (DataGridViewColumn col in dtgReportOutput.Columns)
                    {
                        if (col.Visible) visibleColumnCount++;
                    }

                    PdfPTable pdfTable = new PdfPTable(visibleColumnCount);
                    pdfTable.DefaultCell.Padding = 3;
                    pdfTable.WidthPercentage = 100;
                    pdfTable.HorizontalAlignment = Element.ALIGN_LEFT;

                    // --- Add Headers ---
                    foreach (DataGridViewColumn column in dtgReportOutput.Columns)
                    {
                        if (column.Visible)
                        {
                            PdfPCell cell = new PdfPCell(new Phrase(column.HeaderText, FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.BOLD)));
                            cell.BackgroundColor = new BaseColor(System.Drawing.Color.LightGray);
                            pdfTable.AddCell(cell);
                        }
                    }

                    // --- Add Data Rows ---
                    foreach (DataGridViewRow row in dtgReportOutput.Rows)
                    {
                        if (row.IsNewRow) continue;

                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            if (dtgReportOutput.Columns[cell.ColumnIndex].Visible)
                            {
                                string cellValue = cell.Value?.ToString() ?? "";
                                pdfTable.AddCell(new Phrase(cellValue, FontFactory.GetFont("Arial", 9)));
                            }
                        }
                    }

                    // --- Finalize Document ---
                    pdfDoc.Add(pdfTable);
                    pdfDoc.Close();

                    MessageBox.Show("Report exported successfully to PDF!", "Export Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred during PDF export:\n" + ex.Message, "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
