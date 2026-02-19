using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;

namespace GestionComerce
{
    public class Client
    {
        public int ClientID { get; set; }
        public string Nom { get; set; }
        public string Telephone { get; set; }
        public string Adresse { get; set; }
        public bool IsCompany { get; set; }
        public string EtatJuridique { get; set; }
        public string ICE { get; set; }
        public string SiegeEntreprise { get; set; }
        public string Code { get; set; }
        public bool Etat { get; set; }
        public decimal? Remise { get; set; }

        private static readonly string ConnectionString = "Server=localhost\\SQLEXPRESS;Database=GESTIONCOMERCEP;Trusted_Connection=True;";

        public async Task<List<Client>> GetClientsAsync()
        {
            var clients = new List<Client>();
            string query = "SELECT * FROM Client WHERE Etat=1";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Client client = new Client
                        {
                            ClientID = Convert.ToInt32(reader["ClientID"]),
                            Nom = reader["Nom"].ToString(),
                            Telephone = reader["Telephone"] == DBNull.Value ? string.Empty : reader["Telephone"].ToString(),
                            Adresse = reader["Adresse"] == DBNull.Value ? string.Empty : reader["Adresse"].ToString(),
                            IsCompany = reader["IsCompany"] == DBNull.Value ? false : Convert.ToBoolean(reader["IsCompany"]),
                            EtatJuridique = reader["EtatJuridique"] == DBNull.Value ? string.Empty : reader["EtatJuridique"].ToString(),
                            ICE = reader["ICE"] == DBNull.Value ? string.Empty : reader["ICE"].ToString(),
                            SiegeEntreprise = reader["SiegeEntreprise"] == DBNull.Value ? string.Empty : reader["SiegeEntreprise"].ToString(),
                            Code = reader["Code"] == DBNull.Value ? string.Empty : reader["Code"].ToString(),
                            Etat = reader["Etat"] == DBNull.Value ? true : Convert.ToBoolean(reader["Etat"]),
                            Remise = reader["Remise"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["Remise"])
                        };
                        clients.Add(client);
                    }
                }
            }
            return clients;
        }

        public async Task<int> InsertClientAsync()
        {
            string query = @"INSERT INTO Client (Nom, Telephone, Adresse, IsCompany, EtatJuridique, ICE, SiegeEntreprise, Code, Remise) 
                           VALUES (@Nom, @Telephone, @Adresse, @IsCompany, @EtatJuridique, @ICE, @SiegeEntreprise, @Code, @Remise); 
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
                        cmd.Parameters.AddWithValue("@Adresse", (object)this.Adresse ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@IsCompany", this.IsCompany);
                        cmd.Parameters.AddWithValue("@EtatJuridique", (object)this.EtatJuridique ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ICE", (object)this.ICE ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@SiegeEntreprise", (object)this.SiegeEntreprise ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Code", (object)this.Code ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Remise", (object)this.Remise ?? DBNull.Value);

                        object result = await cmd.ExecuteScalarAsync();
                        int id = Convert.ToInt32(result);
                        return id;
                    }
                }
                catch (Exception err)
                {
                    MessageBox.Show($"Client not inserted, error: {err}");
                    return 0;
                }
            }
        }

        public async Task<int> UpdateClientAsync()
        {
            string query = @"UPDATE Client 
                           SET Nom=@Nom, Telephone=@Telephone, Adresse=@Adresse, 
                               IsCompany=@IsCompany, EtatJuridique=@EtatJuridique, 
                               ICE=@ICE, SiegeEntreprise=@SiegeEntreprise, Code=@Code, Remise=@Remise 
                           WHERE ClientID=@ClientID";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    try
                    {
                        cmd.Parameters.AddWithValue("@Nom", this.Nom);
                        cmd.Parameters.AddWithValue("@Telephone", (object)this.Telephone ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Adresse", (object)this.Adresse ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@IsCompany", this.IsCompany);
                        cmd.Parameters.AddWithValue("@EtatJuridique", (object)this.EtatJuridique ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ICE", (object)this.ICE ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@SiegeEntreprise", (object)this.SiegeEntreprise ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Code", (object)this.Code ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Remise", (object)this.Remise ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ClientID", this.ClientID);
                        await cmd.ExecuteNonQueryAsync();
                        return 1;
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show($"Client not updated: {err}");
                        return 0;
                    }
                }
            }
        }

        public async Task<int> DeleteClientAsync()
        {
            string query = "UPDATE Client SET Etat=0 WHERE ClientID=@ClientID";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    try
                    {
                        cmd.Parameters.AddWithValue("@ClientID", this.ClientID);
                        await cmd.ExecuteNonQueryAsync();
                        return 1;
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show($"Client not deleted: {err}");
                        return 0;
                    }
                }
            }
        }
    }
}