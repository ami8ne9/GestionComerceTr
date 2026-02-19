using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using GestionComerce;

namespace GestionComerce
{
    public class ExpenseCategories
    {
        #region Properties
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        #endregion

        #region CRUD Methods

        // CREATE - Add new category
        public bool Add()
        {
            try
            {
                using (SqlConnection conn = DBHelper.GetConnection())
                {
                    conn.Open();
                    string query = @"INSERT INTO ExpenseCategories (CategoryName, Description, IsActive)
                                   VALUES (@CategoryName, @Description, @IsActive);
                                   SELECT CAST(SCOPE_IDENTITY() AS INT);";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CategoryName", this.CategoryName);
                        cmd.Parameters.AddWithValue("@Description", (object)this.Description ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@IsActive", this.IsActive);

                        this.CategoryID = (int)cmd.ExecuteScalar();
                        return this.CategoryID > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de l'ajout de la catégorie: {ex.Message}", ex);
            }
        }

        // READ - Get all active categories
        public static List<ExpenseCategories> GetAllActive()
        {
            List<ExpenseCategories> categories = new List<ExpenseCategories>();

            try
            {
                using (SqlConnection conn = DBHelper.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT CategoryID, CategoryName, Description, IsActive FROM ExpenseCategories WHERE IsActive = 1 ORDER BY CategoryName";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                categories.Add(new ExpenseCategories
                                {
                                    CategoryID = reader.GetInt32(0),
                                    CategoryName = reader.GetString(1),
                                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                                    IsActive = reader.GetBoolean(3)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors du chargement des catégories: {ex.Message}", ex);
            }

            return categories;
        }

        // READ - Get all categories (including inactive)
        public static List<ExpenseCategories> GetAll()
        {
            List<ExpenseCategories> categories = new List<ExpenseCategories>();

            try
            {
                using (SqlConnection conn = DBHelper.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT CategoryID, CategoryName, Description, IsActive FROM ExpenseCategories ORDER BY CategoryName";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                categories.Add(new ExpenseCategories
                                {
                                    CategoryID = reader.GetInt32(0),
                                    CategoryName = reader.GetString(1),
                                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                                    IsActive = reader.GetBoolean(3)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors du chargement des catégories: {ex.Message}", ex);
            }

            return categories;
        }

        // READ - Get category by ID
        public static ExpenseCategories GetById(int categoryId)
        {
            try
            {
                using (SqlConnection conn = DBHelper.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT CategoryID, CategoryName, Description, IsActive FROM ExpenseCategories WHERE CategoryID = @CategoryID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CategoryID", categoryId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new ExpenseCategories
                                {
                                    CategoryID = reader.GetInt32(0),
                                    CategoryName = reader.GetString(1),
                                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                                    IsActive = reader.GetBoolean(3)
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de la récupération de la catégorie: {ex.Message}", ex);
            }

            return null;
        }

        // UPDATE - Update existing category
        public bool Update()
        {
            try
            {
                using (SqlConnection conn = DBHelper.GetConnection())
                {
                    conn.Open();
                    string query = @"UPDATE ExpenseCategories 
                                   SET CategoryName = @CategoryName,
                                       Description = @Description,
                                       IsActive = @IsActive
                                   WHERE CategoryID = @CategoryID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CategoryID", this.CategoryID);
                        cmd.Parameters.AddWithValue("@CategoryName", this.CategoryName);
                        cmd.Parameters.AddWithValue("@Description", (object)this.Description ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@IsActive", this.IsActive);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de la mise à jour de la catégorie: {ex.Message}", ex);
            }
        }

        // DELETE - Soft delete (set IsActive to false)
        public bool Delete()
        {
            return Delete(this.CategoryID);
        }

        public static bool Delete(int categoryId)
        {
            try
            {
                using (SqlConnection conn = DBHelper.GetConnection())
                {
                    conn.Open();
                    string query = "UPDATE ExpenseCategories SET IsActive = 0 WHERE CategoryID = @CategoryID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CategoryID", categoryId);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de la suppression de la catégorie: {ex.Message}", ex);
            }
        }

        // HARD DELETE - Permanently delete category
        public static bool HardDelete(int categoryId)
        {
            try
            {
                using (SqlConnection conn = DBHelper.GetConnection())
                {
                    conn.Open();
                    string query = "DELETE FROM ExpenseCategories WHERE CategoryID = @CategoryID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CategoryID", categoryId);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de la suppression permanente de la catégorie: {ex.Message}", ex);
            }
        }

        #endregion
    }
}