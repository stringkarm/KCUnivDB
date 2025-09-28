using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static KCUnivDB.AdminSubjects;

namespace KCUnivDB
{
    public static class DashboardManager
    {

        private static readonly string connectionString = @"Data Source = canasa\SQLEXPRESS; Initial catalog = KCUnivDB; Integrated Security = true";

        public static int GetTotalCount(string tableName)
        {
            string query = $"SELECT COUNT(*) FROM dbo.{tableName} WHERE Status = 'Active'";
            int count = 0;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                       
                        object result = command.ExecuteScalar();

                        if (result != null && result != DBNull.Value)
                        {
                            count = Convert.ToInt32(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database Error in GetTotalCount for {tableName}: {ex.Message}");
                return 0;
            }

            return count;
        }

        public static DataTable GetTeachersByDepartment()
        {
            DataTable dt = new DataTable();
            string query = @"
                SELECT 
                    D.DepartmentName, 
                    COUNT(I.InstructorID) AS TeacherCount
                FROM 
                    Instructors I
                INNER JOIN 
                    Departments D ON I.DepartmentID = D.DepartmentID
                WHERE 
                    I.Status = 'Active' -- Only count active instructors
                GROUP BY 
                    D.DepartmentName
                ORDER BY
                    TeacherCount DESC";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    adapter.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database Error in GetTeachersByDepartment: {ex.Message}");
            }

            return dt;
        }
        public static DataTable GetStudentStatusCounts()
        {
            DataTable dt = new DataTable();
            
            string query = @"
                SELECT 
                    Status, 
                    COUNT(StudentID) AS Count
                FROM 
                    Students
                GROUP BY 
                    Status";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    adapter.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database Error in GetStudentStatusCounts: {ex.Message}");
            }

            return dt;
        }
    }
}
