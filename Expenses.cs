using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;


namespace GestionComerce
{
    public class Expenses
    {
        #region Properties
        public int ExpenseID { get; set; }
        public string ExpenseName { get; set; }
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime? LastPaidDate { get; set; }
        public string RecurringType { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        // Computed Properties for UI
        public int DaysUntilDue
        {
            get { return (DueDate - DateTime.Now).Days; }
        }

        public string FormattedAmount
        {
            get { return Amount.ToString("C"); }
        }

        public string FormattedDueDate
        {
            get { return DueDate.ToString("dd/MM/yyyy"); }
        }
        #endregion

        #region CRUD Methods

        // CREATE - Add new expense
        public bool Add()
        {
            try
            {
                using (SqlConnection conn = DBHelper.GetConnection())
                {
                    conn.Open();
                    string query = @"INSERT INTO Expenses (ExpenseName, Category, Amount, DueDate, RecurringType, Notes, PaymentStatus)
                                   VALUES (@ExpenseName, @Category, @Amount, @DueDate, @RecurringType, @Notes, 'Pending');
                                   SELECT CAST(SCOPE_IDENTITY() AS INT);";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ExpenseName", this.ExpenseName);
                        cmd.Parameters.AddWithValue("@Category", this.Category);
                        cmd.Parameters.AddWithValue("@Amount", this.Amount);
                        cmd.Parameters.AddWithValue("@DueDate", this.DueDate);
                        cmd.Parameters.AddWithValue("@RecurringType", this.RecurringType ?? "Une fois");
                        cmd.Parameters.AddWithValue("@Notes", (object)this.Notes ?? DBNull.Value);

                        this.ExpenseID = (int)cmd.ExecuteScalar();
                        return this.ExpenseID > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de l'ajout de la dépense: {ex.Message}", ex);
            }
        }

        // READ - Get all expenses
        public static List<Expenses> GetAll()
        {
            List<Expenses> expenses = new List<Expenses>();

            try
            {
                using (SqlConnection conn = DBHelper.GetConnection())
                {
                    conn.Open();
                    string query = @"SELECT ExpenseID, ExpenseName, Category, Amount, DueDate, 
                                   PaymentStatus, LastPaidDate, RecurringType, Notes, CreatedDate, ModifiedDate
                                   FROM Expenses 
                                   ORDER BY DueDate DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                expenses.Add(new Expenses
                                {
                                    ExpenseID = reader.GetInt32(0),
                                    ExpenseName = reader.GetString(1),
                                    Category = reader.GetString(2),
                                    Amount = reader.GetDecimal(3),
                                    DueDate = reader.GetDateTime(4),
                                    PaymentStatus = reader.GetString(5),
                                    LastPaidDate = reader.IsDBNull(6) ? (DateTime?)null : reader.GetDateTime(6),
                                    RecurringType = reader.IsDBNull(7) ? null : reader.GetString(7),
                                    Notes = reader.IsDBNull(8) ? null : reader.GetString(8),
                                    CreatedDate = reader.GetDateTime(9),
                                    ModifiedDate = reader.GetDateTime(10)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors du chargement des dépenses: {ex.Message}", ex);
            }

            return expenses;
        }

        // READ - Get expense by ID
        public static Expenses GetById(int expenseId)
        {
            try
            {
                using (SqlConnection conn = DBHelper.GetConnection())
                {
                    conn.Open();
                    string query = @"SELECT ExpenseID, ExpenseName, Category, Amount, DueDate, 
                                   PaymentStatus, LastPaidDate, RecurringType, Notes, CreatedDate, ModifiedDate
                                   FROM Expenses 
                                   WHERE ExpenseID = @ExpenseID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ExpenseID", expenseId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Expenses
                                {
                                    ExpenseID = reader.GetInt32(0),
                                    ExpenseName = reader.GetString(1),
                                    Category = reader.GetString(2),
                                    Amount = reader.GetDecimal(3),
                                    DueDate = reader.GetDateTime(4),
                                    PaymentStatus = reader.GetString(5),
                                    LastPaidDate = reader.IsDBNull(6) ? (DateTime?)null : reader.GetDateTime(6),
                                    RecurringType = reader.IsDBNull(7) ? null : reader.GetString(7),
                                    Notes = reader.IsDBNull(8) ? null : reader.GetString(8),
                                    CreatedDate = reader.GetDateTime(9),
                                    ModifiedDate = reader.GetDateTime(10)
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de la récupération de la dépense: {ex.Message}", ex);
            }

            return null;
        }

        // UPDATE - Update existing expense
        public bool Update()
        {
            try
            {
                using (SqlConnection conn = DBHelper.GetConnection())
                {
                    conn.Open();
                    string query = @"UPDATE Expenses 
                                   SET ExpenseName = @ExpenseName,
                                       Category = @Category,
                                       Amount = @Amount,
                                       DueDate = @DueDate,
                                       RecurringType = @RecurringType,
                                       Notes = @Notes,
                                       ModifiedDate = GETDATE()
                                   WHERE ExpenseID = @ExpenseID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ExpenseID", this.ExpenseID);
                        cmd.Parameters.AddWithValue("@ExpenseName", this.ExpenseName);
                        cmd.Parameters.AddWithValue("@Category", this.Category);
                        cmd.Parameters.AddWithValue("@Amount", this.Amount);
                        cmd.Parameters.AddWithValue("@DueDate", this.DueDate);
                        cmd.Parameters.AddWithValue("@RecurringType", this.RecurringType ?? "Une fois");
                        cmd.Parameters.AddWithValue("@Notes", (object)this.Notes ?? DBNull.Value);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de la mise à jour de la dépense: {ex.Message}", ex);
            }
        }

        // DELETE - Delete expense
        public bool Delete()
        {
            return Delete(this.ExpenseID);
        }

        public static bool Delete(int expenseId)
        {
            try
            {
                using (SqlConnection conn = DBHelper.GetConnection())
                {
                    conn.Open();

                    // Delete payment history first (foreign key constraint)
                    string deleteHistoryQuery = "DELETE FROM ExpensePaymentHistory WHERE ExpenseID = @ExpenseID";
                    using (SqlCommand cmd = new SqlCommand(deleteHistoryQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@ExpenseID", expenseId);
                        cmd.ExecuteNonQuery();
                    }

                    // Delete expense
                    string deleteQuery = "DELETE FROM Expenses WHERE ExpenseID = @ExpenseID";
                    using (SqlCommand cmd = new SqlCommand(deleteQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@ExpenseID", expenseId);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de la suppression de la dépense: {ex.Message}", ex);
            }
        }

        // SPECIAL METHODS

        // Get upcoming expenses (expenses due within specified days)
        public static List<Expenses> GetUpcoming(int daysAhead = 7)
        {
            List<Expenses> expenses = new List<Expenses>();

            try
            {
                using (SqlConnection conn = DBHelper.GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_GetUpcomingExpenses", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@DaysAhead", daysAhead);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                expenses.Add(new Expenses
                                {
                                    ExpenseID = reader.GetInt32(0),
                                    ExpenseName = reader.GetString(1),
                                    Category = reader.GetString(2),
                                    Amount = reader.GetDecimal(3),
                                    DueDate = reader.GetDateTime(4),
                                    PaymentStatus = reader.GetString(5),
                                    RecurringType = reader.IsDBNull(6) ? null : reader.GetString(6)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de la vérification des paiements: {ex.Message}", ex);
            }

            return expenses;
        }

        // Mark expense as paid
        public bool MarkAsPaid(string paymentMethod = "Cash", string paymentNotes = null)
        {
            try
            {
                using (SqlConnection conn = DBHelper.GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_MarkExpenseAsPaid", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ExpenseID", this.ExpenseID);
                        cmd.Parameters.AddWithValue("@PaymentAmount", this.Amount);
                        cmd.Parameters.AddWithValue("@PaymentDate", DateTime.Now);
                        cmd.Parameters.AddWithValue("@PaymentMethod", (object)paymentMethod ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Notes", (object)paymentNotes ?? DBNull.Value);

                        cmd.ExecuteNonQuery();
                        this.PaymentStatus = "Paid";
                        this.LastPaidDate = DateTime.Now;
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors du paiement: {ex.Message}", ex);
            }
        }

        // Get expenses by category
        public static List<Expenses> GetByCategory(string category)
        {
            List<Expenses> expenses = new List<Expenses>();

            try
            {
                using (SqlConnection conn = DBHelper.GetConnection())
                {
                    conn.Open();
                    string query = @"SELECT ExpenseID, ExpenseName, Category, Amount, DueDate, 
                                   PaymentStatus, LastPaidDate, RecurringType, Notes, CreatedDate, ModifiedDate
                                   FROM Expenses 
                                   WHERE Category = @Category
                                   ORDER BY DueDate DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Category", category);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                expenses.Add(new Expenses
                                {
                                    ExpenseID = reader.GetInt32(0),
                                    ExpenseName = reader.GetString(1),
                                    Category = reader.GetString(2),
                                    Amount = reader.GetDecimal(3),
                                    DueDate = reader.GetDateTime(4),
                                    PaymentStatus = reader.GetString(5),
                                    LastPaidDate = reader.IsDBNull(6) ? (DateTime?)null : reader.GetDateTime(6),
                                    RecurringType = reader.IsDBNull(7) ? null : reader.GetString(7),
                                    Notes = reader.IsDBNull(8) ? null : reader.GetString(8),
                                    CreatedDate = reader.GetDateTime(9),
                                    ModifiedDate = reader.GetDateTime(10)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors du chargement des dépenses par catégorie: {ex.Message}", ex);
            }

            return expenses;
        }

        // Get expenses by status
        public static List<Expenses> GetByStatus(string status)
        {
            List<Expenses> expenses = new List<Expenses>();

            try
            {
                using (SqlConnection conn = DBHelper.GetConnection())
                {
                    conn.Open();
                    string query = @"SELECT ExpenseID, ExpenseName, Category, Amount, DueDate, 
                                   PaymentStatus, LastPaidDate, RecurringType, Notes, CreatedDate, ModifiedDate
                                   FROM Expenses 
                                   WHERE PaymentStatus = @Status
                                   ORDER BY DueDate DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Status", status);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                expenses.Add(new Expenses
                                {
                                    ExpenseID = reader.GetInt32(0),
                                    ExpenseName = reader.GetString(1),
                                    Category = reader.GetString(2),
                                    Amount = reader.GetDecimal(3),
                                    DueDate = reader.GetDateTime(4),
                                    PaymentStatus = reader.GetString(5),
                                    LastPaidDate = reader.IsDBNull(6) ? (DateTime?)null : reader.GetDateTime(6),
                                    RecurringType = reader.IsDBNull(7) ? null : reader.GetString(7),
                                    Notes = reader.IsDBNull(8) ? null : reader.GetString(8),
                                    CreatedDate = reader.GetDateTime(9),
                                    ModifiedDate = reader.GetDateTime(10)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors du chargement des dépenses par statut: {ex.Message}", ex);
            }

            return expenses;
        }

        #endregion
    }
}