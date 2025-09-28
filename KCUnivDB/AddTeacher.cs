using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KCUnivDB
{
    public partial class AddTeacher : UserControl
    {

        public delegate void RegistrationCompletedEventHandler(object sender, EventArgs e);
        public event RegistrationCompletedEventHandler RegistrationCompleted;


        public AddTeacher()
        {
            InitializeComponent();
            LoadDepartments();
        }

        private string connectionString = @"Data Source=canasa\SQLEXPRESS; Initial catalog=KCUnivDB; Integrated Security=true";

        string cmbG;

        string mailPattern = @"^[\w\.-]+@gmail\.com$";

        string agePattern = @"^(1[0-9]{2}|[1-9]?[0-9])$";

        private void LoadDepartments()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT DepartmentName FROM Departments";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    SqlDataReader reader = cmd.ExecuteReader();

                    cmbDepartment.Items.Clear();
                    while (reader.Read())
                    {
                        cmbDepartment.Items.Add(reader["DepartmentName"].ToString());
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Database error while loading departments: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An unexpected error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string HashPassword(string plainPassword)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(plainPassword);
                byte[] hash = sha256.ComputeHash(bytes);
                StringBuilder builder = new StringBuilder();
                foreach (byte b in hash)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private bool IsEmailExist(string email, string currentProfileId)
        {

            string sqlQuery = "SELECT COUNT(*) FROM Profiles WHERE Email = @email AND ProfileID != @currentProfileId";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@currentProfileId", currentProfileId);
                    conn.Open();
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }


        private void btnRegister_Click(object sender, EventArgs e)
        {


            errorProvider1.Clear();
            errorProvider2.Clear();
            errorProvider3.Clear();
            errorProvider4.Clear();
            errorProvider5.Clear();
            errorProvider6.Clear();
            errorProvider7.Clear();
            errorProvider8.Clear();


            string age = txtAge.Text;
            string phone = txtPhone.Text;
            string email = txtEmail.Text;
            DateTime hireDate;
            hireDate = dateTimePicker1.Value;

            string action = "Add Teacher";
            string description = "Added a new teacher";



            if (string.IsNullOrEmpty(txtFirstname.Text) || string.IsNullOrEmpty(txtLastname.Text) || string.IsNullOrEmpty(cmbGender.Text) || string.IsNullOrEmpty(txtAge.Text)
                 || string.IsNullOrEmpty(txtPhone.Text) || string.IsNullOrEmpty(txtAddress.Text) || string.IsNullOrEmpty(txtEmail.Text) || string.IsNullOrEmpty(cmbDepartment.Text))
            {


                if (string.IsNullOrWhiteSpace(txtFirstname.Text))
                {
                    errorProvider1.SetError(txtFirstname, "First name is required.");

                }

                if (string.IsNullOrWhiteSpace(txtLastname.Text))
                {
                    errorProvider2.SetError(txtLastname, "Last name is required.");

                }

                if (string.IsNullOrWhiteSpace(cmbGender.Text))
                {
                    errorProvider3.SetError(cmbGender, "Gender is required.");

                }

                if (string.IsNullOrWhiteSpace(txtAge.Text))
                {
                    errorProvider4.SetError(txtAge, "Age is required.");

                }

                if (string.IsNullOrWhiteSpace(txtPhone.Text))
                {
                    errorProvider5.SetError(txtPhone, "Phone number is required.");
                }

                if (string.IsNullOrWhiteSpace(txtAddress.Text))
                {
                    errorProvider6.SetError(txtAddress, "Address is required.");

                }

                if (string.IsNullOrWhiteSpace(txtEmail.Text))
                {
                    errorProvider7.SetError(txtEmail, "Email is required.");
                }

                if (string.IsNullOrWhiteSpace(cmbDepartment.Text))
                {
                    errorProvider8.SetError(cmbDepartment, "Department is required.");
                }

            }

            bool allValid = true;

            if (!IsValid(email, mailPattern))
            {
                errorProvider7.SetError(txtEmail, "Please enter a valid Email.");
                allValid = false;
            }


            if (!IsValid(age, agePattern))
            {
                errorProvider4.SetError(txtAge, "Age is in invalid format.");
                allValid = false;
            }

            if (!allValid)
            {
                return;
            }


            if (cmbGender.SelectedIndex == 0)
            {
                cmbG += cmbGender.Text;
            }
            if (cmbGender.SelectedIndex == 1)
            {
                cmbG += cmbGender.Text;
            }
            if (cmbGender.SelectedIndex == 2)
            {
                cmbG += cmbGender.Text;
            }




            using (SqlConnection conn = new SqlConnection(connectionString))
            {

                conn.Open();

                SqlCommand Checkcmd = new SqlCommand("SELECT COUNT(*) FROM Profiles WHERE Email = @email", conn);
                Checkcmd.Parameters.AddWithValue("@email", txtEmail.Text);

                int userCount = (int)Checkcmd.ExecuteScalar();

                if (userCount > 0)
                {
                    MessageBox.Show("This email address is already in use by another user.", "Email Conflict", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Random rnd = new Random();
                string generatedUserID = "TE" + rnd.Next(100000, 999999).ToString();
                string generatedPassword = generatedUserID;

                string hashedPassword = HashPassword(generatedPassword);

                SqlCommand cmd = new SqlCommand("AddTeacher_SP", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@firstname", txtFirstname.Text);
                cmd.Parameters.AddWithValue("@lastname", txtLastname.Text);
                cmd.Parameters.AddWithValue("@age", txtAge.Text);
                cmd.Parameters.AddWithValue("@gender", cmbG);
                cmd.Parameters.AddWithValue("@phone", txtPhone.Text);
                cmd.Parameters.AddWithValue("@address", txtAddress.Text);
                cmd.Parameters.AddWithValue("@email", txtEmail.Text);
                cmd.Parameters.AddWithValue("@Username", generatedUserID);
                cmd.Parameters.AddWithValue("@HashedPassword", hashedPassword);
                cmd.Parameters.AddWithValue("@Department", cmbDepartment.Text);
                cmd.Parameters.AddWithValue("@HiredDate", hireDate);
                cmd.Parameters.AddWithValue("@Action", action);
                cmd.Parameters.AddWithValue("@Description", description);


                cmd.ExecuteNonQuery();
                MessageBox.Show("Added Teacher Successful!" + "\n Username: " + generatedUserID +
                                "\n Password: " + generatedPassword,
                                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                AdminTeachers adminteacher = new AdminTeachers();
                adminteacher.Show();
                this.Hide();
            }



        }


        private void ClearFields()
        {
            txtFirstname.Clear();
            txtLastname.Clear();
            txtAge.Clear();
            cmbGender.SelectedIndex = -1;
            txtPhone.Clear();
            txtEmail.Clear();
            txtAddress.Clear();
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
                this.Hide();
        }

       
     
        private void AddTeacher_Load(object sender, EventArgs e)
        {
           
            if (dateTimePicker1 != null)
            {
               
                dateTimePicker1.Value = DateTime.Now;

               
                dateTimePicker1.Visible = false;
            }
        }

        public static bool IsValid(string input, string pattern)
        {
            return Regex.IsMatch(input, pattern);
        }

    }

}
