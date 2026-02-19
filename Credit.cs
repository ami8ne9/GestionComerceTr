using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;

namespace GestionComerce
{
    public class Credit
    {
        public int CreditID { get; set; }
        public int? ClientID { get; set; }          // nullable
        public int? FournisseurID { get; set; }     // nullable
        public decimal Total { get; set; }
        public decimal Paye { get; set; }
        public decimal Difference { get; set; }
        public bool Etat { get; set; }

        private static readonly string ConnectionString =
            "Server=localhost\\SQLEXPRESS;Database=GESTIONCOMERCEP;Trusted_Connection=True;";

        // CHANGE THIS if your real table name is different (e.g. "CreditClient")
        private const string TableName = "Credit";

        public async Task<List<Credit>> GetCreditsAsync()
        {
            var credits = new List<Credit>();
            string query = $"SELECT * FROM {TableName} WHERE Etat = 1";

            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                using (var cmd = new SqlCommand(query, connection))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        credits.Add(new Credit
                        {
                            CreditID = reader["CreditID"] != DBNull.Value ? Convert.ToInt32(reader["CreditID"]) : 0,
                            ClientID = reader["ClientID"] != DBNull.Value ? (int?)Convert.ToInt32(reader["ClientID"]) : null,
                            FournisseurID = reader["FournisseurID"] != DBNull.Value ? (int?)Convert.ToInt32(reader["FournisseurID"]) : null,
                            Total = reader["Total"] != DBNull.Value ? Convert.ToDecimal(reader["Total"]) : 0m,
                            Paye = reader["Paye"] != DBNull.Value ? Convert.ToDecimal(reader["Paye"]) : 0m,
                            Difference = reader["Difference"] != DBNull.Value ? Convert.ToDecimal(reader["Difference"]) : 0m,
                            Etat = reader["Etat"] != DBNull.Value ? Convert.ToBoolean(reader["Etat"]) : true
                        });
                    }
                }
            }

            return credits;
        }

        public async Task<int> InsertCreditAsync()
        {
            string query = $@"
                INSERT INTO {TableName} (ClientID, FournisseurID, Total, Paye)
                VALUES (@ClientID, @FournisseurID, @Total, @Paye);
                SELECT SCOPE_IDENTITY();";

            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                try
                {
                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = (object)ClientID ?? DBNull.Value;
                        cmd.Parameters.Add("@FournisseurID", SqlDbType.Int).Value = (object)FournisseurID ?? DBNull.Value;

                        var pTotal = cmd.Parameters.Add("@Total", SqlDbType.Decimal);
                        pTotal.Precision = 18; pTotal.Scale = 2; pTotal.Value = Total;

                        var pPaye = cmd.Parameters.Add("@Paye", SqlDbType.Decimal);
                        pPaye.Precision = 18; pPaye.Scale = 2; pPaye.Value = Paye;



                        object result = await cmd.ExecuteScalarAsync();
                        return Convert.ToInt32(result);
                    }
                }
                catch (Exception err)
                {
                    MessageBox.Show($"Credit not inserted, error: {err}");
                    return 0;
                }
            }
        }

        public async Task<int> UpdateCreditAsync()
        {
            string query = $@"
                UPDATE {TableName}
                SET ClientID = @ClientID,
                    FournisseurID = @FournisseurID,
                    Total = @Total,
                    Paye = @Paye
                WHERE CreditID = @CreditID";

            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                using (var cmd = new SqlCommand(query, connection))
                {
                    try
                    {
                        cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = (object)ClientID ?? DBNull.Value;
                        cmd.Parameters.Add("@FournisseurID", SqlDbType.Int).Value = (object)FournisseurID ?? DBNull.Value;

                        var pTotal = cmd.Parameters.Add("@Total", SqlDbType.Decimal);
                        pTotal.Precision = 18; pTotal.Scale = 2; pTotal.Value = Total;

                        var pPaye = cmd.Parameters.Add("@Paye", SqlDbType.Decimal);
                        pPaye.Precision = 18; pPaye.Scale = 2; pPaye.Value = Paye;
                        cmd.Parameters.Add("@CreditID", SqlDbType.Int).Value = CreditID;

                        await cmd.ExecuteNonQueryAsync();
                        return 1;
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show($"Credit not updated: {err}");
                        return 0;
                    }
                }
            }
        }

        /// <summary>
        /// Soft-delete the credit (Etat = 0).
        /// If updateRelated = true it will also update related OperationClient/OperationFournisseur rows
        /// so they don't keep referencing a "deleted" credit. Adjust SQL below to match your schema.
        /// </summary>
        public async Task<int> DeleteCreditAsync(bool updateRelated = true)
        {
            string updateCreditSql = $"UPDATE {TableName} SET Etat = 0 WHERE CreditID = @CreditID";

            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                using (var tx = connection.BeginTransaction())
                {
                    try
                    {
                        // 1) mark credit as deleted (soft-delete)
                        using (var cmd = new SqlCommand(updateCreditSql, connection, tx))
                        {
                            cmd.Parameters.Add("@CreditID", SqlDbType.Int).Value = CreditID;
                            await cmd.ExecuteNonQueryAsync();
                        }

                        if (updateRelated)
                        {
                            // ===== OPTION A: NULL out the CreditID in related operation rows (requires CreditID column to be nullable)
                            using (var cmdOpClient = new SqlCommand("UPDATE OperationClient SET CreditID = NULL WHERE CreditID = @CreditID", connection, tx))
                            {
                                cmdOpClient.Parameters.Add("@CreditID", SqlDbType.Int).Value = CreditID;
                                await cmdOpClient.ExecuteNonQueryAsync();
                            }
                            using (var cmdOpFourn = new SqlCommand("UPDATE OperationFournisseur SET CreditID = NULL WHERE CreditID = @CreditID", connection, tx))
                            {
                                cmdOpFourn.Parameters.Add("@CreditID", SqlDbType.Int).Value = CreditID;
                                await cmdOpFourn.ExecuteNonQueryAsync();
                            }

                            // ===== OPTION B (alternative): mark those operation rows as closed/inactive (uncomment & adapt if you prefer)
                            // using (var cmdOpClientState = new SqlCommand("UPDATE OperationClient SET Etat = 0 WHERE CreditID = @CreditID", connection, tx))
                            // {
                            //     cmdOpClientState.Parameters.Add("@CreditID", SqlDbType.Int).Value = CreditID;
                            //     await cmdOpClientState.ExecuteNonQueryAsync();
                            // }
                            // using (var cmdOpFournState = new SqlCommand("UPDATE OperationFournisseur SET Etat = 0 WHERE CreditID = @CreditID", connection, tx))
                            // {
                            //     cmdOpFournState.Parameters.Add("@CreditID", SqlDbType.Int).Value = CreditID;
                            //     await cmdOpFournState.ExecuteNonQueryAsync();
                            // }

                            // ===== OPTION C (example): update a 'HasCredit' or other flag in client/fournisseur tables (if you track such column)
                            // if (ClientID.HasValue)
                            // {
                            //     using (var cmdClient = new SqlCommand("UPDATE Client SET HasCredit = 0 WHERE ClientID = @ClientID", connection, tx))
                            //     {
                            //         cmdClient.Parameters.Add("@ClientID", SqlDbType.Int).Value = ClientID.Value;
                            //         await cmdClient.ExecuteNonQueryAsync();
                            //     }
                            // }
                            // else if (FournisseurID.HasValue)
                            // {
                            //     using (var cmdFourn = new SqlCommand("UPDATE Fournisseur SET HasCredit = 0 WHERE FournisseurID = @FournisseurID", connection, tx))
                            //     {
                            //         cmdFourn.Parameters.Add("@FournisseurID", SqlDbType.Int).Value = FournisseurID.Value;
                            //         await cmdFourn.ExecuteNonQueryAsync();
                            //     }
                            // }
                        }

                        tx.Commit();
                        return 1;
                    }
                    catch (Exception err)
                    {
                        try { tx.Rollback(); } catch { }
                        MessageBox.Show($"Credit not deleted: {err}");
                        return 0;
                    }
                }
            }
        }
    }
}
