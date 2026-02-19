using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;

namespace GestionComerce
{
    public class OperationArticle
    {
        public int OperationArticleID { get; set; }
        public int ArticleID { get; set; }
        public int OperationID { get; set; }
        public int QteArticle { get; set; }
        public DateTime Date { get; set; }
        public bool Etat { get; set; }
        public bool Reversed { get; set; }

        private static readonly string ConnectionString = "Server=localhost\\SQLEXPRESS;Database=GESTIONCOMERCEP;Trusted_Connection=True;";

        // ================== GET (Active Only - Etat=1) ==================
        public async Task<List<OperationArticle>> GetOperationArticlesAsync()
        {
            var list = new List<OperationArticle>();
            string query = "SELECT * FROM OperationArticle WHERE Etat=1 ";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(new OperationArticle
                        {
                            OperationArticleID = Convert.ToInt32(reader["OperationArticleID"]),
                            ArticleID = Convert.ToInt32(reader["ArticleID"]),
                            OperationID = Convert.ToInt32(reader["OperationID"]),
                            QteArticle = Convert.ToInt32(reader["QteArticle"]),
                            Etat = reader["Etat"] == DBNull.Value ? true : Convert.ToBoolean(reader["Etat"]),
                            Reversed = reader["Reversed"] == DBNull.Value ? false : Convert.ToBoolean(reader["Reversed"])
                        });
                    }
                }
            }
            return list;
        }

        // ================== GET ALL (Including Deleted - No Etat Filter) ==================
        public async Task<List<OperationArticle>> GetAllOperationArticlesAsync()
        {
            var list = new List<OperationArticle>();
            // NO Etat filter - gets ALL operation articles including deleted ones
            string query = "SELECT * FROM OperationArticle";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(new OperationArticle
                        {
                            OperationArticleID = Convert.ToInt32(reader["OperationArticleID"]),
                            ArticleID = Convert.ToInt32(reader["ArticleID"]),
                            OperationID = Convert.ToInt32(reader["OperationID"]),
                            QteArticle = Convert.ToInt32(reader["QteArticle"]),
                            Etat = reader["Etat"] == DBNull.Value ? true : Convert.ToBoolean(reader["Etat"]),
                            Reversed = reader["Reversed"] == DBNull.Value ? false : Convert.ToBoolean(reader["Reversed"])
                        });
                    }
                }
            }
            return list;
        }

        // ================== INSERT ==================
        public async Task<int> InsertOperationArticleAsync()
        {
            string query = "INSERT INTO OperationArticle (ArticleID, OperationID, QteArticle, Reversed) " +
                           "VALUES (@ArticleID, @OperationID, @QteArticle, @Reversed); SELECT SCOPE_IDENTITY();";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                try
                {
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@ArticleID", this.ArticleID);
                        cmd.Parameters.AddWithValue("@OperationID", this.OperationID);
                        cmd.Parameters.AddWithValue("@QteArticle", this.QteArticle);
                        cmd.Parameters.AddWithValue("@Reversed", this.Reversed);

                        object result = await cmd.ExecuteScalarAsync();
                        return Convert.ToInt32(result);
                    }
                }
                catch (Exception err)
                {
                    MessageBox.Show($"OperationArticle not inserted, error: {err}");
                    return 0;
                }
            }
        }

        // ================== UPDATE ==================
        public async Task<int> UpdateOperationArticleAsync()
        {
            string query = "UPDATE OperationArticle SET ArticleID=@ArticleID, OperationID=@OperationID, QteArticle=@QteArticle, Reversed=@Reversed " +
                           "WHERE OperationArticleID=@OperationArticleID";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    try
                    {
                        cmd.Parameters.AddWithValue("@ArticleID", this.ArticleID);
                        cmd.Parameters.AddWithValue("@OperationID", this.OperationID);
                        cmd.Parameters.AddWithValue("@QteArticle", this.QteArticle);
                        cmd.Parameters.AddWithValue("@Reversed", this.Reversed);
                        cmd.Parameters.AddWithValue("@OperationArticleID", this.OperationArticleID);

                        await cmd.ExecuteNonQueryAsync();
                        return 1;
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show($"OperationArticle not updated: {err}");
                        return 0;
                    }
                }
            }
        }

        // ================== DELETE (Soft Delete) ==================
        public async Task<int> DeleteOperationArticleAsync()
        {
            string query = "UPDATE OperationArticle SET Etat=0 WHERE OperationArticleID=@OperationArticleID";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    try
                    {
                        cmd.Parameters.AddWithValue("@OperationArticleID", this.OperationArticleID);
                        await cmd.ExecuteNonQueryAsync();
                        return 1;
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show($"OperationArticle not deleted: {err}");
                        return 0;
                    }
                }
            }
        }

        // ================== REVERSE (Helper) ==================
        public async Task<int> ReverseAsync()
        {
            string query = "UPDATE OperationArticle SET Reversed=1 WHERE OperationArticleID=@OperationArticleID";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    try
                    {
                        cmd.Parameters.AddWithValue("@OperationArticleID", this.OperationArticleID);
                        await cmd.ExecuteNonQueryAsync();
                        this.Reversed = true;
                        return 1;
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show($"OperationArticle not reversed: {err}");
                        return 0;
                    }
                }
            }
        }
    }
}