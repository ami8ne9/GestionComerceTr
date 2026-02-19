using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;

namespace GestionComerce.Models
{
    public class CheckHistory
    {
        public int CheckID { get; set; }
        public string CheckReference { get; set; } = "";
        public byte[] CheckImage { get; set; }  // Store actual image binary data
        public string CheckImagePath { get; set; } = "";  // For backward compatibility
        public int? InvoiceID { get; set; }
        public decimal? CheckAmount { get; set; }
        public DateTime CheckDate { get; set; }
        public string BankName { get; set; } = "";
        public string CheckStatus { get; set; } = "En Attente";
        public string Notes { get; set; } = "";
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        private static readonly string ConnectionString =
            "Server=localhost\\SQLEXPRESS;Database=GESTIONCOMERCEP;Trusted_Connection=True;";

        public CheckHistory()
        {
            CheckDate = DateTime.Now;
            CreatedDate = DateTime.Now;
            UpdatedDate = DateTime.Now;
            CheckStatus = "En Attente";
        }

        // CREATE - Insert new check
        public async Task<int> InsertCheckAsync()
        {
            string query = @"
                INSERT INTO CheckHistory 
                (CheckReference, CheckImage, CheckImagePath, InvoiceID, CheckAmount, CheckDate, 
                 BankName, CheckStatus, Notes, CreatedDate, UpdatedDate)
                VALUES 
                (@CheckReference, @CheckImage, @CheckImagePath, @InvoiceID, @CheckAmount, @CheckDate, 
                 @BankName, @CheckStatus, @Notes, @CreatedDate, @UpdatedDate);
                SELECT CAST(SCOPE_IDENTITY() as int)";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                try
                {
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@CheckReference", (object)CheckReference ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@CheckImage", (object)CheckImage ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@CheckImagePath", (object)CheckImagePath ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@InvoiceID", (object)InvoiceID ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@CheckAmount", (object)CheckAmount ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@CheckDate", CheckDate);
                        cmd.Parameters.AddWithValue("@BankName", (object)BankName ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@CheckStatus", (object)CheckStatus ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Notes", (object)Notes ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@CreatedDate", CreatedDate);
                        cmd.Parameters.AddWithValue("@UpdatedDate", UpdatedDate);

                        CheckID = (int)await cmd.ExecuteScalarAsync();
                        return 1;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Chèque non inséré: " + ex.Message);
                    return 0;
                }
            }
        }

        // READ - Get check by ID
        public async Task<CheckHistory> GetCheckByIDAsync(int checkID)
        {
            string query = @"SELECT CheckID, CheckReference, CheckImage, CheckImagePath, InvoiceID, CheckAmount, 
                                   CheckDate, BankName, CheckStatus, Notes, CreatedDate, UpdatedDate
                            FROM CheckHistory 
                            WHERE CheckID = @CheckID";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@CheckID", checkID);
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return MapReaderToCheck(reader);
                        }
                    }
                }
            }
            return null;
        }

        // READ - Get all checks
        public async Task<List<CheckHistory>> GetAllChecksAsync()
        {
            List<CheckHistory> checks = new List<CheckHistory>();
            string query = @"SELECT CheckID, CheckReference, CheckImage, CheckImagePath, InvoiceID, CheckAmount, 
                                   CheckDate, BankName, CheckStatus, Notes, CreatedDate, UpdatedDate
                            FROM CheckHistory 
                            ORDER BY CheckDate DESC";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        checks.Add(MapReaderToCheck(reader));
                    }
                }
            }
            return checks;
        }

        // READ - Get checks by invoice ID
        public async Task<List<CheckHistory>> GetChecksByInvoiceIDAsync(int invoiceID)
        {
            List<CheckHistory> checks = new List<CheckHistory>();
            string query = @"SELECT CheckID, CheckReference, CheckImage, CheckImagePath, InvoiceID, CheckAmount, 
                                   CheckDate, BankName, CheckStatus, Notes, CreatedDate, UpdatedDate
                            FROM CheckHistory 
                            WHERE InvoiceID = @InvoiceID
                            ORDER BY CheckDate DESC";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@InvoiceID", invoiceID);
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            checks.Add(MapReaderToCheck(reader));
                        }
                    }
                }
            }
            return checks;
        }

        // READ - Search checks by reference
        public async Task<List<CheckHistory>> SearchChecksByReferenceAsync(string reference)
        {
            List<CheckHistory> checks = new List<CheckHistory>();
            string query = @"SELECT CheckID, CheckReference, CheckImage, CheckImagePath, InvoiceID, CheckAmount, 
                                   CheckDate, BankName, CheckStatus, Notes, CreatedDate, UpdatedDate
                            FROM CheckHistory 
                            WHERE CheckReference LIKE @Reference
                            ORDER BY CheckDate DESC";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Reference", "%" + reference + "%");
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            checks.Add(MapReaderToCheck(reader));
                        }
                    }
                }
            }
            return checks;
        }

        // READ - Get checks by status
        public async Task<List<CheckHistory>> GetChecksByStatusAsync(string status)
        {
            List<CheckHistory> checks = new List<CheckHistory>();
            string query = @"SELECT CheckID, CheckReference, CheckImage, CheckImagePath, InvoiceID, CheckAmount, 
                                   CheckDate, BankName, CheckStatus, Notes, CreatedDate, UpdatedDate
                            FROM CheckHistory 
                            WHERE CheckStatus = @Status
                            ORDER BY CheckDate DESC";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Status", status);
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            checks.Add(MapReaderToCheck(reader));
                        }
                    }
                }
            }
            return checks;
        }

        // UPDATE - Update check
        public async Task<int> UpdateCheckAsync()
        {
            string query = @"UPDATE CheckHistory 
                            SET CheckReference = @CheckReference,
                                CheckImage = @CheckImage,
                                CheckImagePath = @CheckImagePath,
                                InvoiceID = @InvoiceID,
                                CheckAmount = @CheckAmount,
                                CheckDate = @CheckDate,
                                BankName = @BankName,
                                CheckStatus = @CheckStatus,
                                Notes = @Notes,
                                UpdatedDate = @UpdatedDate
                            WHERE CheckID = @CheckID";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                try
                {
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@CheckID", CheckID);
                        cmd.Parameters.AddWithValue("@CheckReference", (object)CheckReference ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@CheckImage", (object)CheckImage ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@CheckImagePath", (object)CheckImagePath ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@InvoiceID", (object)InvoiceID ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@CheckAmount", (object)CheckAmount ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@CheckDate", CheckDate);
                        cmd.Parameters.AddWithValue("@BankName", (object)BankName ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@CheckStatus", (object)CheckStatus ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Notes", (object)Notes ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@UpdatedDate", DateTime.Now);

                        await cmd.ExecuteNonQueryAsync();
                        return 1;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Chèque non mis à jour: " + ex.Message);
                    return 0;
                }
            }
        }

        // UPDATE - Update check status only
        public async Task<int> UpdateCheckStatusAsync(string status)
        {
            string query = @"UPDATE CheckHistory 
                            SET CheckStatus = @Status,
                                UpdatedDate = @UpdatedDate
                            WHERE CheckID = @CheckID";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                try
                {
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@CheckID", CheckID);
                        cmd.Parameters.AddWithValue("@Status", status);
                        cmd.Parameters.AddWithValue("@UpdatedDate", DateTime.Now);

                        await cmd.ExecuteNonQueryAsync();
                        CheckStatus = status;
                        return 1;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Statut du chèque non mis à jour: " + ex.Message);
                    return 0;
                }
            }
        }

        // DELETE - Delete check
        public async Task<int> DeleteCheckAsync()
        {
            string query = "DELETE FROM CheckHistory WHERE CheckID = @CheckID";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                try
                {
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@CheckID", CheckID);
                        await cmd.ExecuteNonQueryAsync();
                        return 1;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Chèque non supprimé: " + ex.Message);
                    return 0;
                }
            }
        }

        // Helper method to map SqlDataReader to CheckHistory object
        private CheckHistory MapReaderToCheck(SqlDataReader reader)
        {
            return new CheckHistory
            {
                CheckID = Convert.ToInt32(reader["CheckID"]),
                CheckReference = reader["CheckReference"] != DBNull.Value ? reader["CheckReference"].ToString() : "",
                CheckImage = reader["CheckImage"] != DBNull.Value ? (byte[])reader["CheckImage"] : null,
                CheckImagePath = reader["CheckImagePath"] != DBNull.Value ? reader["CheckImagePath"].ToString() : "",
                InvoiceID = reader["InvoiceID"] != DBNull.Value ? Convert.ToInt32(reader["InvoiceID"]) : (int?)null,
                CheckAmount = reader["CheckAmount"] != DBNull.Value ? Convert.ToDecimal(reader["CheckAmount"]) : (decimal?)null,
                CheckDate = Convert.ToDateTime(reader["CheckDate"]),
                BankName = reader["BankName"] != DBNull.Value ? reader["BankName"].ToString() : "",
                CheckStatus = reader["CheckStatus"] != DBNull.Value ? reader["CheckStatus"].ToString() : "En Attente",
                Notes = reader["Notes"] != DBNull.Value ? reader["Notes"].ToString() : "",
                CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                UpdatedDate = Convert.ToDateTime(reader["UpdatedDate"])
            };
        }
    }
}