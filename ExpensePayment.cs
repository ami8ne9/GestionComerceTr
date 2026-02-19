using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using GestionComerce;

namespace GestionComerce
{
    public class ExpensePayment
    {
        #region Properties
        public int PaymentID { get; set; }
        public int ExpenseID { get; set; }
        public decimal PaymentAmount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedDate { get; set; }

        // Computed Properties for UI
        public string FormattedAmount
        {
            get { return PaymentAmount.ToString("C"); }
        }

        public string FormattedDate
        {
            get { return PaymentDate.ToString("dd/MM/yyyy"); }
        }
        #endregion

        #region CRUD Methods

        // CREATE - Add new payment
        public bool Add()
        {
            try
            {
                using (SqlConnection conn = DBHelper.GetConnection())
                {
                    conn.Open();
                    string query = @"INSERT INTO ExpensePaymentHistory (ExpenseID, PaymentAmount, PaymentDate, PaymentMethod, Notes)
                                   VALUES (@ExpenseID, @PaymentAmount, @PaymentDate, @PaymentMethod, @Notes);
                                   SELECT CAST(SCOPE_IDENTITY() AS INT);";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ExpenseID", this.ExpenseID);
                        cmd.Parameters.AddWithValue("@PaymentAmount", this.PaymentAmount);
                        cmd.Parameters.AddWithValue("@PaymentDate", this.PaymentDate);
                        cmd.Parameters.AddWithValue("@PaymentMethod", (object)this.PaymentMethod ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Notes", (object)this.Notes ?? DBNull.Value);

                        this.PaymentID = (int)cmd.ExecuteScalar();
                        return this.PaymentID > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de l'ajout du paiement: {ex.Message}", ex);
            }
        }

        // READ - Get all payments for a specific expense
        public static List<ExpensePayment> GetByExpenseId(int expenseId)
        {
            List<ExpensePayment> payments = new List<ExpensePayment>();

            try
            {
                using (SqlConnection conn = DBHelper.GetConnection())
                {
                    conn.Open();
                    string query = @"SELECT PaymentID, ExpenseID, PaymentAmount, PaymentDate, PaymentMethod, Notes, CreatedDate
                                   FROM ExpensePaymentHistory 
                                   WHERE ExpenseID = @ExpenseID
                                   ORDER BY PaymentDate DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ExpenseID", expenseId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                payments.Add(new ExpensePayment
                                {
                                    PaymentID = reader.GetInt32(0),
                                    ExpenseID = reader.GetInt32(1),
                                    PaymentAmount = reader.GetDecimal(2),
                                    PaymentDate = reader.GetDateTime(3),
                                    PaymentMethod = reader.IsDBNull(4) ? null : reader.GetString(4),
                                    Notes = reader.IsDBNull(5) ? null : reader.GetString(5),
                                    CreatedDate = reader.GetDateTime(6)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors du chargement de l'historique des paiements: {ex.Message}", ex);
            }

            return payments;
        }

        // READ - Get all payments
        public static List<ExpensePayment> GetAll()
        {
            List<ExpensePayment> payments = new List<ExpensePayment>();

            try
            {
                using (SqlConnection conn = DBHelper.GetConnection())
                {
                    conn.Open();
                    string query = @"SELECT PaymentID, ExpenseID, PaymentAmount, PaymentDate, PaymentMethod, Notes, CreatedDate
                                   FROM ExpensePaymentHistory 
                                   ORDER BY PaymentDate DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                payments.Add(new ExpensePayment
                                {
                                    PaymentID = reader.GetInt32(0),
                                    ExpenseID = reader.GetInt32(1),
                                    PaymentAmount = reader.GetDecimal(2),
                                    PaymentDate = reader.GetDateTime(3),
                                    PaymentMethod = reader.IsDBNull(4) ? null : reader.GetString(4),
                                    Notes = reader.IsDBNull(5) ? null : reader.GetString(5),
                                    CreatedDate = reader.GetDateTime(6)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors du chargement des paiements: {ex.Message}", ex);
            }

            return payments;
        }

        // READ - Get payment by ID
        public static ExpensePayment GetById(int paymentId)
        {
            try
            {
                using (SqlConnection conn = DBHelper.GetConnection())
                {
                    conn.Open();
                    string query = @"SELECT PaymentID, ExpenseID, PaymentAmount, PaymentDate, PaymentMethod, Notes, CreatedDate
                                   FROM ExpensePaymentHistory 
                                   WHERE PaymentID = @PaymentID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@PaymentID", paymentId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new ExpensePayment
                                {
                                    PaymentID = reader.GetInt32(0),
                                    ExpenseID = reader.GetInt32(1),
                                    PaymentAmount = reader.GetDecimal(2),
                                    PaymentDate = reader.GetDateTime(3),
                                    PaymentMethod = reader.IsDBNull(4) ? null : reader.GetString(4),
                                    Notes = reader.IsDBNull(5) ? null : reader.GetString(5),
                                    CreatedDate = reader.GetDateTime(6)
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de la récupération du paiement: {ex.Message}", ex);
            }

            return null;
        }

        // UPDATE - Update existing payment
        public bool Update()
        {
            try
            {
                using (SqlConnection conn = DBHelper.GetConnection())
                {
                    conn.Open();
                    string query = @"UPDATE ExpensePaymentHistory 
                                   SET PaymentAmount = @PaymentAmount,
                                       PaymentDate = @PaymentDate,
                                       PaymentMethod = @PaymentMethod,
                                       Notes = @Notes
                                   WHERE PaymentID = @PaymentID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@PaymentID", this.PaymentID);
                        cmd.Parameters.AddWithValue("@PaymentAmount", this.PaymentAmount);
                        cmd.Parameters.AddWithValue("@PaymentDate", this.PaymentDate);
                        cmd.Parameters.AddWithValue("@PaymentMethod", (object)this.PaymentMethod ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Notes", (object)this.Notes ?? DBNull.Value);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de la mise à jour du paiement: {ex.Message}", ex);
            }
        }

        // DELETE - Delete payment
        public bool Delete()
        {
            return Delete(this.PaymentID);
        }

        public static bool Delete(int paymentId)
        {
            try
            {
                using (SqlConnection conn = DBHelper.GetConnection())
                {
                    conn.Open();
                    string query = "DELETE FROM ExpensePaymentHistory WHERE PaymentID = @PaymentID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@PaymentID", paymentId);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de la suppression du paiement: {ex.Message}", ex);
            }
        }

        // SPECIAL METHODS

        // Get total payments for an expense
        public static decimal GetTotalByExpenseId(int expenseId)
        {
            try
            {
                using (SqlConnection conn = DBHelper.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT ISNULL(SUM(PaymentAmount), 0) FROM ExpensePaymentHistory WHERE ExpenseID = @ExpenseID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ExpenseID", expenseId);
                        return (decimal)cmd.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors du calcul du total des paiements: {ex.Message}", ex);
            }
        }

        #endregion
    }
}