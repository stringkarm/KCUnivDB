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

                    // Assuming you have a ComboBox named cbDepartment on your form
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


            SqlTransaction transaction = null;
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();

                    // Generate a new unique teacher username (e.g., TC000001, TC000002)
                    string getNextIdQuery = "SELECT ISNULL(MAX(SUBSTRING(Username, 3, 6)), 0) FROM Users WHERE Username LIKE 'TC%'";
                    SqlCommand getNextIdCmd = new SqlCommand(getNextIdQuery, connection, transaction);
                    int nextId = Convert.ToInt32(getNextIdCmd.ExecuteScalar()) + 1;
                    string generatedUsername = $"TC{nextId:D6}";
                    string generatedPassword = generatedUsername; // Password is the same as the username
                    string hashedPassword = HashPassword(generatedPassword);

                    // 1. Insert into Profiles table
                    string insertProfileQuery = "INSERT INTO Profiles (FirstName, LastName, Age, Gender, Phone, Email, Address, Status) VALUES (@FirstName, @LastName, @Age, @Gender, @Phone, @Email, @Address, 'Active'); SELECT SCOPE_IDENTITY();";
                    SqlCommand insertProfileCmd = new SqlCommand(insertProfileQuery, connection, transaction);
                    insertProfileCmd.Parameters.AddWithValue("@FirstName", txtFirstname.Text.Trim());
                    insertProfileCmd.Parameters.AddWithValue("@LastName", txtLastname.Text.Trim());
                    insertProfileCmd.Parameters.AddWithValue("@Age", int.Parse(txtAge.Text.Trim()));
                    insertProfileCmd.Parameters.AddWithValue("@Gender", cmbGender.Text);
                    insertProfileCmd.Parameters.AddWithValue("@Phone", txtPhone.Text.Trim());
                    insertProfileCmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                    insertProfileCmd.Parameters.AddWithValue("@Address", txtAddress.Text.Trim());

                    int profileId = Convert.ToInt32(insertProfileCmd.ExecuteScalar());


                    string insertUserQuery = "INSERT INTO Users (Username, Password, RoleID, ProfileID) VALUES (@Username, @Password, @RoleID, @ProfileID)";
                    SqlCommand insertUserCmd = new SqlCommand(insertUserQuery, connection, transaction);
                    insertUserCmd.Parameters.AddWithValue("@Username", generatedUsername);
                    insertUserCmd.Parameters.AddWithValue("@Password", hashedPassword);
                    insertUserCmd.Parameters.AddWithValue("@RoleID", 2); 
                    insertUserCmd.Parameters.AddWithValue("@ProfileID", profileId);

                    insertUserCmd.ExecuteNonQuery();

                    string getDeptIdQuery = "SELECT DepartmentID FROM Departments WHERE DepartmentName = @DepartmentName";
                    SqlCommand getDeptIdCmd = new SqlCommand(getDeptIdQuery, connection, transaction);
                    getDeptIdCmd.Parameters.AddWithValue("@DepartmentName", cmbDepartment.Text);
                    int departmentId = Convert.ToInt32(getDeptIdCmd.ExecuteScalar());

                    string insertInstructorQuery = "INSERT INTO Instructors (ProfileID, HireDate, DepartmentID) VALUES (@ProfileID, GETDATE(), @DepartmentID)";
                    SqlCommand insertInstructorCmd = new SqlCommand(insertInstructorQuery, connection, transaction);
                    insertInstructorCmd.Parameters.AddWithValue("@ProfileID", profileId);
                    insertInstructorCmd.Parameters.AddWithValue("@DepartmentID", departmentId);

                    insertInstructorCmd.ExecuteNonQuery();

                    transaction.Commit();
                    MessageBox.Show($"Teacher registered successfully!\nUsername: {generatedUsername}\nPassword: {generatedPassword}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    ClearFields();
                }
            }
            catch (SqlException ex)
            {
                transaction?.Rollback();
                MessageBox.Show("Database error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                MessageBox.Show("An unexpected error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private bool IsEmailExist(string email)
        {

            string query = "SELECT COUNT(*) FROM Users WHERE Email = @Email";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Email", email);

                try
                {
                    connection.Open();
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
                catch (SqlException ex)
                {

                    Console.WriteLine("Database error in IsEmailExist: " + ex.Message);
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An unexpected error occurred in IsEmailExist: " + ex.Message);
                    return false;
                }
            }
        }

        private void AddTeacher_Load(object sender, EventArgs e)
        {
            dateTimePicker1.Value = DateTime.Now;
        }

        public static bool IsValid(string input, string pattern)
        {
            return Regex.IsMatch(input, pattern);
        }

    }

}
