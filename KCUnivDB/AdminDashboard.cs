using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
        private string connectionString = @"Data Source=canasa\SQLEXPRESS; Initial catalog=KCUnivDB; Integrated Security=true";


        public AdminDashboard()
        {
            InitializeComponent();

            adminLogout1.Hide();
        }

       
        private void btnApproval_Click(object sender, EventArgs e)
        {
            AdminApproval approve = new AdminApproval();
            approve.Show();
            this.Hide();
        }

        private void btnTeachers_Click(object sender, EventArgs e)
        {
            AdminTeachers approve = new AdminTeachers();
            approve.Show();
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
    }
}
