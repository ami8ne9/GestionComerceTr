using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;

namespace GestionComerce
{
    public class Fournisseur
    {
        public int FournisseurID { get; set; }
        public string Nom { get; set; }
        public string Telephone { get; set; }
        public bool Etat { get; set; }

        // 🆕 New Columns
        public string EtatJuridic { get; set; }
        public string ICE { get; set; }
        public string SiegeEntreprise { get; set; }
        public string Adresse { get; set; }
        public string Code { get; set; }

        private static readonly string ConnectionString =
            "Server=localhost\\SQLEXPRESS;Database=GESTIONCOMERCEP;Trusted_Connection=True;";

        // 🔹 Get all active fournisseurs
        public async Task<List<Fournisseur>> GetFournisseursAsync()
        {
            var fournisseurs = new List<Fournisseur>();
            string query = "SELECT * FROM Fournisseur WHERE Etat = 1";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        fournisseurs.Add(new Fournisseur
                        {
                            FournisseurID = Convert.ToInt32(reader["FournisseurID"]),
                            Nom = reader["Nom"].ToString(),
                            Telephone = reader["Telephone"] == DBNull.Value ? string.Empty : reader["Telephone"].ToString(),
                            Etat = reader["Etat"] == DBNull.Value ? true : Convert.ToBoolean(reader["Etat"]),
                            EtatJuridic = reader["EtatJuridic"] == DBNull.Value ? string.Empty : reader["EtatJuridic"].ToString(),
                            ICE = reader["ICE"] == DBNull.Value ? string.Empty : reader["ICE"].ToString(),
                            SiegeEntreprise = reader["SiegeEntreprise"] == DBNull.Value ? string.Empty : reader["SiegeEntreprise"].ToString(),
                            Adresse = reader["Adresse"] == DBNull.Value ? string.Empty : reader["Adresse"].ToString(),
                            Code = reader["Code"] == DBNull.Value ? string.Empty : reader["Code"].ToString()
                        });
                    }
                }
            }
            return fournisseurs;
        }

        // 🔹 Insert new fournisseur
        public async Task<int> InsertFournisseurAsync()
        {
            string query = @"
                INSERT INTO Fournisseur 
                (Nom, Telephone, EtatJuridic, ICE, SiegeEntreprise, Adresse, Code, Etat) 
                VALUES 
                (@Nom, @Telephone, @EtatJuridic, @ICE, @SiegeEntreprise, @Adresse, @Code, 1);
                SELECT SCOPE_IDENTITY();";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                try
                {
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Nom", this.Nom);
                        cmd.Parameters.AddWithValue("@Telephone", (object)this.Telephone ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@EtatJuridic", (object)this.EtatJuridic ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ICE", (object)this.ICE ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@SiegeEntreprise", (object)this.SiegeEntreprise ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Adresse", (object)this.Adresse ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Code", (object)this.Code ?? DBNull.Value);

                        object result = await cmd.ExecuteScalarAsync();
                        return Convert.ToInt32(result);
                    }
                }
                catch (Exception err)
                {
                    MessageBox.Show($"Fournisseur not inserted, error: {err.Message}");
                    return 0;
                }
            }
        }

        // 🔹 Update existing fournisseur
        public async Task<int> UpdateFournisseurAsync()
        {
            string query = @"
                UPDATE Fournisseur 
                SET Nom=@Nom, Telephone=@Telephone, EtatJuridic=@EtatJuridic, ICE=@ICE,
                    SiegeEntreprise=@SiegeEntreprise, Adresse=@Adresse, Code=@Code
                WHERE FournisseurID=@FournisseurID";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    try
                    {
                        cmd.Parameters.AddWithValue("@Nom", this.Nom);
                        cmd.Parameters.AddWithValue("@Telephone", (object)this.Telephone ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@EtatJuridic", (object)this.EtatJuridic ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ICE", (object)this.ICE ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@SiegeEntreprise", (object)this.SiegeEntreprise ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Adresse", (object)this.Adresse ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Code", (object)this.Code ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@FournisseurID", this.FournisseurID);

                        await cmd.ExecuteNonQueryAsync();
                        return 1;
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show($"Fournisseur not updated: {err.Message}");
                        return 0;
                    }
                }
            }
        }

        // 🔹 Soft delete (disable fournisseur)
        public async Task<int> DeleteFournisseurAsync()
        {
            string query = "UPDATE Fournisseur SET Etat=0 WHERE FournisseurID=@FournisseurID";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    try
                    {
                        cmd.Parameters.AddWithValue("@FournisseurID", this.FournisseurID);
                        await cmd.ExecuteNonQueryAsync();
                        return 1;
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show($"Fournisseur not deleted: {err.Message}");
                        return 0;
                    }
                }
            }
        }
    }
}
