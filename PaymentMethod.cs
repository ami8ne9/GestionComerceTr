using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;

namespace GestionComerce
{
    public class PaymentMethod
    {
        public int PaymentMethodID { get; set; }
        public string PaymentMethodName { get; set; }
        public string ImagePath { get; set; } // New property for image

        private static readonly string ConnectionString = "Server=localhost\\SQLEXPRESS;Database=GESTIONCOMERCEP;Trusted_Connection=True;";

        // Get all payment methods
        public async Task<List<PaymentMethod>> GetPaymentMethodsAsync()
        {
            var methods = new List<PaymentMethod>();
            string query = "SELECT * FROM PaymentMethod";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        PaymentMethod method = new PaymentMethod
                        {
                            PaymentMethodID = Convert.ToInt32(reader["PaymentMethodID"]),
                            PaymentMethodName = reader["PaymentMethodName"].ToString(),
                            ImagePath = reader["ImagePath"]?.ToString() ?? ""
                        };
                        methods.Add(method);
                    }
                }
            }
            return methods;
        }

        // Insert new payment method
        public async Task<int> InsertPaymentMethodAsync()
        {
            string query = "INSERT INTO PaymentMethod (PaymentMethodName, ImagePath) VALUES (@PaymentMethodName, @ImagePath); SELECT SCOPE_IDENTITY();";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                try
                {
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@PaymentMethodName", this.PaymentMethodName ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ImagePath", this.ImagePath ?? (object)DBNull.Value);
                        object result = await cmd.ExecuteScalarAsync();
                        return Convert.ToInt32(result);
                    }
                }
                catch (Exception err)
                {
                    MessageBox.Show($"Payment method not inserted, error: {err}");
                    return 0;
                }
            }
        }

        // Update existing payment method
        public async Task<int> UpdatePaymentMethodAsync()
        {
            string query = "UPDATE PaymentMethod SET PaymentMethodName=@PaymentMethodName, ImagePath=@ImagePath WHERE PaymentMethodID=@PaymentMethodID";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    try
                    {
                        cmd.Parameters.AddWithValue("@PaymentMethodName", this.PaymentMethodName ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ImagePath", this.ImagePath ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@PaymentMethodID", this.PaymentMethodID);
                        await cmd.ExecuteNonQueryAsync();
                        return 1;
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show($"Payment method not updated: {err}");
                        return 0;
                    }
                }
            }
        }

        // Delete payment method
        public async Task<int> DeletePaymentMethodAsync()
        {
            string query = "DELETE FROM PaymentMethod WHERE PaymentMethodID=@PaymentMethodID";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    try
                    {
                        cmd.Parameters.AddWithValue("@PaymentMethodID", this.PaymentMethodID);
                        await cmd.ExecuteNonQueryAsync();
                        return 1;
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show($"Payment method not deleted: {err}");
                        return 0;
                    }
                }
            }
        }
    }
}