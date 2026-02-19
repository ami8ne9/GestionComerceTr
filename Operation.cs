using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;

namespace GestionComerce
{
    public class Operation
    {
        public DateTime Date { get; set; }  // Alias for DateOperation
        public int OperationID { get; set; }
        public decimal PrixOperation { get; set; }
        public decimal Remise { get; set; }
        public decimal CreditValue { get; set; }
        public int UserID { get; set; }
        public int? ClientID { get; set; }       // nullable
        public int? FournisseurID { get; set; }  // nullable
        public int? CreditID { get; set; }       // nullable
        public int? PaymentMethodID { get; set; } // NEW: nullable
        public DateTime DateOperation { get; set; }
        public bool Etat { get; set; }

        // New properties
        public string OperationType { get; set; } = string.Empty;
        public bool Reversed { get; set; }

        private static readonly string ConnectionString = "Server=localhost\\SQLEXPRESS;Database=GESTIONCOMERCEP;Trusted_Connection=True;";

        public async Task<List<Operation>> GetOperationsAsync()
        {
            var operations = new List<Operation>();
            string query = "SELECT * FROM Operation WHERE Etat=1 ORDER BY Date DESC";

            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                using (var cmd = new SqlCommand(query, connection))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        operations.Add(new Operation
                        {
                            Date = reader["Date"] != DBNull.Value ? Convert.ToDateTime(reader["Date"]) : DateTime.MinValue,
                            OperationID = reader["OperationID"] != DBNull.Value ? Convert.ToInt32(reader["OperationID"]) : 0,
                            PrixOperation = reader["PrixOperation"] != DBNull.Value ? Convert.ToDecimal(reader["PrixOperation"]) : 0m,
                            DateOperation = reader["Date"] != DBNull.Value ? Convert.ToDateTime(reader["Date"]) : DateTime.MinValue,
                            Remise = reader["Remise"] != DBNull.Value ? Convert.ToDecimal(reader["Remise"]) : 0m,
                            CreditValue = reader["CreditValue"] != DBNull.Value ? Convert.ToDecimal(reader["CreditValue"]) : 0m,
                            UserID = reader["UserID"] != DBNull.Value ? Convert.ToInt32(reader["UserID"]) : 0,
                            ClientID = reader["ClientID"] != DBNull.Value ? (int?)Convert.ToInt32(reader["ClientID"]) : null,
                            FournisseurID = reader["FournisseurID"] != DBNull.Value ? (int?)Convert.ToInt32(reader["FournisseurID"]) : null,
                            CreditID = reader["CreditID"] != DBNull.Value ? (int?)Convert.ToInt32(reader["CreditID"]) : null,
                            PaymentMethodID = reader["PaymentMethodID"] != DBNull.Value ? (int?)Convert.ToInt32(reader["PaymentMethodID"]) : null, // NEW
                            Etat = reader["Etat"] == DBNull.Value ? true : Convert.ToBoolean(reader["Etat"]),
                            OperationType = reader["OperationType"] == DBNull.Value ? string.Empty : reader["OperationType"].ToString(),
                            Reversed = reader["Reversed"] == DBNull.Value ? false : Convert.ToBoolean(reader["Reversed"])
                        });
                    }
                }
            }

            return operations;
        }

        public async Task<int> InsertOperationAsync()
        {
            string query = @"INSERT INTO Operation 
                            (PrixOperation, Remise, CreditValue, UserID, ClientID, FournisseurID, CreditID, PaymentMethodID, OperationType) 
                            VALUES (@PrixOperation, @Remise, @CreditValue, @UserID, @ClientID, @FournisseurID, @CreditID, @PaymentMethodID, @OperationType); 
                            SELECT SCOPE_IDENTITY();";

            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                try
                {
                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@PrixOperation", PrixOperation);
                        cmd.Parameters.AddWithValue("@Remise", Remise);
                        cmd.Parameters.AddWithValue("@CreditValue", CreditValue);
                        cmd.Parameters.AddWithValue("@UserID", UserID);
                        cmd.Parameters.AddWithValue("@ClientID", (object)ClientID ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@FournisseurID", (object)FournisseurID ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@CreditID", (object)CreditID ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@PaymentMethodID", (object)PaymentMethodID ?? DBNull.Value); // NEW
                        cmd.Parameters.AddWithValue("@OperationType", string.IsNullOrEmpty(OperationType) ? (object)DBNull.Value : OperationType);

                        // AFTER
                        object result = await cmd.ExecuteScalarAsync();
                        int operationId = Convert.ToInt32(result);
                        try
                        {
                            var comptaService = new GestionComerce.ComptabiliteService();
                            DateTime safeDate = (DateOperation == DateTime.MinValue || DateOperation.Year < 1753)
                             ? DateTime.Now
                             : DateOperation;

                            if (OperationType.StartsWith("Vente"))
                            {
                                comptaService.EnregistrerVente(operationId, PrixOperation, PaymentMethodID, ClientID, safeDate);
                            }
                            else if (OperationType.StartsWith("Achat"))
                            {
                                comptaService.EnregistrerAchat(operationId, PrixOperation, string.Format("OP-{0}", operationId), FournisseurID, safeDate);
                            }
                        }
                        catch { /* Don't block the sale if accounting fails */ }

                        return operationId;
                    }
                }
                catch (Exception err)
                {
                    MessageBox.Show($"Operation not inserted, error: {err.Message}");
                    return 0;
                }
            }
        }

        public async Task<int> UpdateOperationAsync()
        {
            string query = @"UPDATE Operation 
                            SET PrixOperation=@PrixOperation, Remise=@Remise, CreditValue=@CreditValue, 
                                UserID=@UserID, ClientID=@ClientID, FournisseurID=@FournisseurID, CreditID=@CreditID,
                                PaymentMethodID=@PaymentMethodID, OperationType=@OperationType, Reversed=@Reversed
                            WHERE OperationID=@OperationID";

            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                using (var cmd = new SqlCommand(query, connection))
                {
                    try
                    {
                        cmd.Parameters.AddWithValue("@PrixOperation", PrixOperation);
                        cmd.Parameters.AddWithValue("@Remise", Remise);
                        cmd.Parameters.AddWithValue("@CreditValue", CreditValue);
                        cmd.Parameters.AddWithValue("@UserID", UserID);
                        cmd.Parameters.AddWithValue("@ClientID", (object)ClientID ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@FournisseurID", (object)FournisseurID ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@CreditID", (object)CreditID ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@PaymentMethodID", (object)PaymentMethodID ?? DBNull.Value); // NEW
                        cmd.Parameters.AddWithValue("@OperationType", string.IsNullOrEmpty(OperationType) ? (object)DBNull.Value : OperationType);
                        cmd.Parameters.AddWithValue("@Reversed", Reversed);
                        cmd.Parameters.AddWithValue("@OperationID", OperationID);

                        await cmd.ExecuteNonQueryAsync();
                        return 1;
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show($"Operation not updated: {err.Message}");
                        return 0;
                    }
                }
            }
        }

        public async Task<int> DeleteOperationAsync()
        {
            string query = "UPDATE Operation SET Etat=0 WHERE OperationID=@OperationID";

            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                using (var cmd = new SqlCommand(query, connection))
                {
                    try
                    {
                        cmd.Parameters.AddWithValue("@OperationID", OperationID);
                        await cmd.ExecuteNonQueryAsync();
                        return 1;
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show($"Operation not deleted: {err.Message}");
                        return 0;
                    }
                }
            }
        }
    }
}
