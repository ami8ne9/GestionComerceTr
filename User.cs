using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;

namespace GestionComerce { 
    public class User
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string Code { get; set; }
        public int RoleID { get; set; }
        public int Etat { get; set; }

        private static readonly string ConnectionString = "Server=localhost\\SQLEXPRESS;Database=GESTIONCOMERCEP;Trusted_Connection=True;";

        public async Task<List<User>> GetUsersAsync()
        {
            var Users = new List<User>();
            string Query = "SELECT * FROM Users where Etat=1";

            

            using (SqlConnection Connection = new SqlConnection(ConnectionString))
            {
                await Connection.OpenAsync();

                
                using (SqlCommand cmd = new SqlCommand(Query, Connection))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        
                        User user = new User
                        {
                            UserID = Convert.ToInt32(reader["UserID"]),
                            UserName = reader["UserName"].ToString(),
                            Code = reader["Code"].ToString(),
                            RoleID = Convert.ToInt32(reader["RoleID"]),
                            Etat = Convert.ToInt32(reader["Etat"])
                        };

                        
                        Users.Add(user);
                    }
                }
            }

            return Users;
        }

        public async Task<int> InsertUserAsync()
        {
            string Query = "INSERT INTO Users (UserName, Code, RoleID, Etat) " +
                           "VALUES (@UserName, @Code, @RoleID, @Etat); SELECT SCOPE_IDENTITY();";

            using (SqlConnection Connection = new SqlConnection(ConnectionString))
            {
                await Connection.OpenAsync();
                try
                {
                    using (SqlCommand cmd = new SqlCommand(Query, Connection))
                    {
                        cmd.Parameters.AddWithValue("@UserName", this.UserName);
                        cmd.Parameters.AddWithValue("@Code", this.Code);
                        cmd.Parameters.AddWithValue("@RoleID", this.RoleID);
                        cmd.Parameters.AddWithValue("@Etat", this.Etat);

                        object result = await cmd.ExecuteScalarAsync();
                        int eId = Convert.ToInt32(result);


                        return eId;
                    }
                }
                catch (Exception err)
                {
                    MessageBox.Show($"User not inserted, error: {err}");
                    return 0;
                }
            }
        }

        public async Task<int> DeleteUserAsync()
        {
            string Query = "UPDATE Users SET Etat=0 WHERE UserID=@UserID";

            using (SqlConnection Connection = new SqlConnection(ConnectionString))
            {
                await Connection.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(Query, Connection))
                {
                    try
                    {
                        cmd.Parameters.AddWithValue("@UserID", this.UserID);
                        await cmd.ExecuteNonQueryAsync();
                        return 1;
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show($"User not deleted: {err}");
                        return 0;
                    }
                }
            }
        }

        public async Task<int> UpdateUserAsync()
        {
            string Query = "UPDATE Users SET " +
                           "UserName=@UserName, " +
                           "Code=@Code, " +
                           "RoleID=@RoleID," +
                           "Etat=@Etat " +
                           "WHERE UserID=@UserID";

            using (SqlConnection Connection = new SqlConnection(ConnectionString))
            {
                await Connection.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(Query, Connection))
                {
                    try
                    {
                        cmd.Parameters.AddWithValue("@UserName", this.UserName);
                        cmd.Parameters.AddWithValue("@Code", this.Code);
                        cmd.Parameters.AddWithValue("@RoleID", this.RoleID);
                        cmd.Parameters.AddWithValue("@Etat", this.Etat);
                        cmd.Parameters.AddWithValue("@UserID", this.UserID);
                        await cmd.ExecuteNonQueryAsync();
                        return 1;
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show($"User not updated: {err}");
                        return 0;
                    }
                }
            }
        }
    }
}
