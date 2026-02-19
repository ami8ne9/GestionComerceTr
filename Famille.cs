using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GestionComerce
{
    public class Famille
    {
        public int FamilleID { get; set; }
        public string FamilleName { get; set; }
        public int NbrArticle { get; set; }

        private static readonly string ConnectionString = "Server=localhost\\SQLEXPRESS;Database=GESTIONCOMERCEP;Trusted_Connection=True;";

        public async Task<List<Famille>> GetFamillesAsync()
        {
            var Familles = new List<Famille>();
            string Query = "SELECT * FROM Familly where Etat=1";



            using (SqlConnection Connection = new SqlConnection(ConnectionString))
            {
                await Connection.OpenAsync();


                using (SqlCommand cmd = new SqlCommand(Query, Connection))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {

                        Famille Famille = new Famille
                        {
                            FamilleID = Convert.ToInt32(reader["FamilleID"]),
                            FamilleName = reader["FamillyName"].ToString(),
                            NbrArticle = Convert.ToInt32(reader["NbrArticles"])
                        };


                        Familles.Add(Famille);
                    }
                }
            }

            return Familles;
        }

        public async Task<int> InsertFamilleAsync()
        {
            string Query = "INSERT INTO Familly (FamillyName,NbrArticles) " +
                           "VALUES (@FamilleName,@NbrArticle); SELECT SCOPE_IDENTITY();";

            using (SqlConnection Connection = new SqlConnection(ConnectionString))
            {
                await Connection.OpenAsync();
                try
                {
                    using (SqlCommand cmd = new SqlCommand(Query, Connection))
                    {
                        cmd.Parameters.AddWithValue("@FamilleName", this.FamilleName);
                        cmd.Parameters.AddWithValue("@NbrArticle", this.NbrArticle);

                        object result = await cmd.ExecuteScalarAsync();
                        int eId = Convert.ToInt32(result);


                        return eId;
                    }
                }
                catch (Exception err)
                {
                    MessageBox.Show($"Famille not inserted, error: {err}");
                    return 0;
                }
            }
        }

        public async Task<int> DeleteFamilleAsync()
        {
            string Query = "UPDATE Familly SET Etat=0 WHERE FamilleID=@FamilleID";

            using (SqlConnection Connection = new SqlConnection(ConnectionString))
            {
                await Connection.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(Query, Connection))
                {
                    try
                    {
                        cmd.Parameters.AddWithValue("@FamilleID", this.FamilleID);
                        await cmd.ExecuteNonQueryAsync();
                        return 1;
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show($"Famille not deleted: {err}");
                        return 0;
                    }
                }
            }
        }

        public async Task<int> UpdateFamilleAsync()
        {
            string Query = "UPDATE Familly SET " +
                           "FamillyName=@FamilleName, " +
                           "NbrArticles=@NbrArticle " +
                           "WHERE FamilleID=@FamilleID";

            using (SqlConnection Connection = new SqlConnection(ConnectionString))
            {
                await Connection.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(Query, Connection))
                {
                    try
                    {
                        cmd.Parameters.AddWithValue("@FamilleName", this.FamilleName);
                        cmd.Parameters.AddWithValue("@NbrArticle", this.NbrArticle);
                        cmd.Parameters.AddWithValue("@FamilleID", this.FamilleID);
                        await cmd.ExecuteNonQueryAsync();
                        return 1;
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show($"Famille not updated: {err}");
                        return 0;
                    }
                }
            }
        }
    }
}
