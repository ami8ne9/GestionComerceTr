using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace GestionComerce.Main.Facturation
{
    // Invoice Model
    public class Invoice
    {

        public int InvoiceID { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string InvoiceType { get; set; }
        public string InvoiceIndex { get; set; }
        public string CreditClientName { get; set; }
        public decimal CreditMontant { get; set; }
        // New Fields
        public string Objet { get; set; }
        public string NumberLetters { get; set; }
        public string NameFactureGiven { get; set; }
        public string NameFactureReceiver { get; set; }
        public string ReferenceClient { get; set; }
        public string PaymentMethod { get; set; }

        // User/Company Information
        public string UserName { get; set; }
        public string UserICE { get; set; }
        public string UserVAT { get; set; }
        public string UserPhone { get; set; }
        public string UserAddress { get; set; }
        public string UserEtatJuridique { get; set; }
        public string UserIdSociete { get; set; }
        public string UserSiegeEntreprise { get; set; }

        // Client Information
        public string ClientName { get; set; }
        public string ClientICE { get; set; }
        public string ClientVAT { get; set; }
        public string ClientPhone { get; set; }
        public string ClientAddress { get; set; }
        public string ClientEtatJuridique { get; set; }
        public string ClientIdSociete { get; set; }
        public string ClientSiegeEntreprise { get; set; }

        // Financial Information
        public string Currency { get; set; }
        public decimal TVARate { get; set; }
        public decimal TotalHT { get; set; }
        public decimal TotalTVA { get; set; }
        public decimal TotalTTC { get; set; }
        public decimal Remise { get; set; }
        public decimal TotalAfterRemise { get; set; }

        // Invoice State
        public int EtatFacture { get; set; }
        public bool IsReversed { get; set; }

        // Additional Information
        public string Description { get; set; }
        public string LogoPath { get; set; }

        // Audit Fields
        public DateTime CreatedDate { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public bool IsDeleted { get; set; }

        // Navigation Property
        public List<InvoiceArticle> Articles { get; set; } = new List<InvoiceArticle>();

        // Nested Article class - keeps everything organized in one place
        public class InvoiceArticle
        {
            public int InvoiceArticleID { get; set; }
            public int InvoiceID { get; set; }
            public int? OperationID { get; set; }
            public int ArticleID { get; set; }
            public string ArticleName { get; set; }
            public decimal PrixUnitaire { get; set; }
            public decimal Quantite { get; set; }
            public decimal TVA { get; set; }
            public bool IsReversed { get; set; }
            public DateTime CreatedDate { get; set; }
            public bool IsDeleted { get; set; }

            // Calculated properties
            public decimal TotalHT => PrixUnitaire * Quantite;
            public decimal MontantTVA => (TVA / 100) * TotalHT;
            public decimal TotalTTC => TotalHT + MontantTVA;
        }

        // Helper method to calculate totals from articles
        public void CalculateTotals()
        {
            TotalHT = 0;
            TotalTVA = 0;

            foreach (var article in Articles.Where(a => a.Quantite > 0))
            {
                if (article.IsReversed == IsReversed)
                {
                    TotalHT += article.TotalHT;
                    TotalTVA += article.MontantTVA;
                }
            }

            TotalTTC = TotalHT + TotalTVA;
            TotalAfterRemise = TotalHT - Remise + (TotalHT > 0 ? (TotalTVA / TotalHT) * (TotalHT - Remise) : 0);
        }
    }

    // Invoice Repository - CRUD Operations
    public class InvoiceRepository
    {
        private readonly string _connectionString;

        public InvoiceRepository(string connectionString)
        {
            _connectionString = "Server=localhost\\SQLEXPRESS;Database=GESTIONCOMERCEP;Trusted_Connection=True;";
        }

        #region Invoice CRUD Operations

        // Create Invoice with Articles
        public async Task<int> CreateInvoiceAsync(Invoice invoice)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string invoiceQuery = @"
                    INSERT INTO Invoice (
                        InvoiceNumber, InvoiceDate, InvoiceType, InvoiceIndex,
                        CreditClientName, CreditMontant,
                        Objet, NumberLetters, NameFactureGiven, NameFactureReceiver, ReferenceClient,
                        UserName, UserICE, UserVAT, UserPhone, UserAddress, 
                        UserEtatJuridique, UserIdSociete, UserSiegeEntreprise,
                        ClientName, ClientICE, ClientVAT, ClientPhone, ClientAddress,
                        ClientEtatJuridique, ClientIdSociete, ClientSiegeEntreprise,
                        Currency, TVARate, TotalHT, TotalTVA, TotalTTC, 
                        Remise, TotalAfterRemise, EtatFacture, IsReversed,
                        Description, LogoPath, CreatedBy
                    ) VALUES (
                        @InvoiceNumber, @InvoiceDate, @InvoiceType, @InvoiceIndex,
                        @CreditClientName, @CreditMontant,
                        @Objet, @NumberLetters, @NameFactureGiven, @NameFactureReceiver, @ReferenceClient,
                        @UserName, @UserICE, @UserVAT, @UserPhone, @UserAddress,
                        @UserEtatJuridique, @UserIdSociete, @UserSiegeEntreprise,
                        @ClientName, @ClientICE, @ClientVAT, @ClientPhone, @ClientAddress,
                        @ClientEtatJuridique, @ClientIdSociete, @ClientSiegeEntreprise,
                        @Currency, @TVARate, @TotalHT, @TotalTVA, @TotalTTC,
                        @Remise, @TotalAfterRemise, @EtatFacture, @IsReversed,
                        @Description, @LogoPath, @CreatedBy
                    );
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                        SqlCommand cmd = new SqlCommand(invoiceQuery, conn, transaction);
                        AddInvoiceParameters(cmd, invoice);

                        int invoiceId = (int)await cmd.ExecuteScalarAsync();

                        foreach (var article in invoice.Articles)
                        {
                            await InsertInvoiceArticleAsync(conn, transaction, invoiceId, article);
                        }

                        transaction.Commit();
                        return invoiceId;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        // Read Invoice by ID with Articles
        public async Task<Invoice> GetInvoiceByIdAsync(int invoiceId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                // Get Invoice
                string invoiceQuery = @"
                    SELECT * FROM Invoice 
                    WHERE InvoiceID = @InvoiceID AND IsDeleted = 0";

                SqlCommand cmd = new SqlCommand(invoiceQuery, conn);
                cmd.Parameters.AddWithValue("@InvoiceID", invoiceId);

                Invoice invoice = null;

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        invoice = MapInvoiceFromReader(reader);
                    }
                }

                if (invoice != null)
                {
                    // Get Articles
                    invoice.Articles = await GetInvoiceArticlesAsync(invoiceId);
                }

                return invoice;
            }
        }

        // Read Invoice by Number
        public async Task<Invoice> GetInvoiceByNumberAsync(string invoiceNumber)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                string query = @"
                    SELECT * FROM Invoice 
                    WHERE InvoiceNumber = @InvoiceNumber AND IsDeleted = 0";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@InvoiceNumber", invoiceNumber);

                Invoice invoice = null;

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        invoice = MapInvoiceFromReader(reader);
                    }
                }

                if (invoice != null)
                {
                    invoice.Articles = await GetInvoiceArticlesAsync(invoice.InvoiceID);
                }

                return invoice;
            }
        }

        // Read All Invoices
        public async Task<List<Invoice>> GetAllInvoicesAsync(bool includeDeleted = false)
        {
            List<Invoice> invoices = new List<Invoice>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                string query = includeDeleted
                    ? "SELECT * FROM Invoice ORDER BY InvoiceDate DESC"
                    : "SELECT * FROM Invoice WHERE IsDeleted = 0 ORDER BY InvoiceDate DESC";

                SqlCommand cmd = new SqlCommand(query, conn);

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        invoices.Add(MapInvoiceFromReader(reader));
                    }
                }
            }

            return invoices;
        }

        // Search Invoices
        public async Task<List<Invoice>> SearchInvoicesAsync(string searchTerm, DateTime? startDate = null, DateTime? endDate = null)
        {
            List<Invoice> invoices = new List<Invoice>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                string query = @"
                    SELECT * FROM Invoice 
                    WHERE IsDeleted = 0
                    AND (
                        InvoiceNumber LIKE @SearchTerm OR
                        ClientName LIKE @SearchTerm OR
                        ClientICE LIKE @SearchTerm
                    )";

                if (startDate.HasValue)
                    query += " AND InvoiceDate >= @StartDate";

                if (endDate.HasValue)
                    query += " AND InvoiceDate <= @EndDate";

                query += " ORDER BY InvoiceDate DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@SearchTerm", "%" + searchTerm + "%");

                if (startDate.HasValue)
                    cmd.Parameters.AddWithValue("@StartDate", startDate.Value);

                if (endDate.HasValue)
                    cmd.Parameters.AddWithValue("@EndDate", endDate.Value);

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        invoices.Add(MapInvoiceFromReader(reader));
                    }
                }
            }

            return invoices;
        }

        // Update Invoice
        public async Task<bool> UpdateInvoiceAsync(Invoice invoice)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string updateQuery = @"
                    UPDATE Invoice SET
                        InvoiceNumber = @InvoiceNumber,
                        InvoiceDate = @InvoiceDate,
                        InvoiceType = @InvoiceType,
                        InvoiceIndex = @InvoiceIndex,
                        CreditClientName = @CreditClientName,
                        CreditMontant = @CreditMontant,
                        Objet = @Objet,
                        NumberLetters = @NumberLetters,
                        NameFactureGiven = @NameFactureGiven,
                        NameFactureReceiver = @NameFactureReceiver,
                        ReferenceClient = @ReferenceClient,
                        UserName = @UserName,
                        UserICE = @UserICE,
                        UserVAT = @UserVAT,
                        UserPhone = @UserPhone,
                        UserAddress = @UserAddress,
                        UserEtatJuridique = @UserEtatJuridique,
                        UserIdSociete = @UserIdSociete,
                        UserSiegeEntreprise = @UserSiegeEntreprise,
                        ClientName = @ClientName,
                        ClientICE = @ClientICE,
                        ClientVAT = @ClientVAT,
                        ClientPhone = @ClientPhone,
                        ClientAddress = @ClientAddress,
                        ClientEtatJuridique = @ClientEtatJuridique,
                        ClientIdSociete = @ClientIdSociete,
                        ClientSiegeEntreprise = @ClientSiegeEntreprise,
                        Currency = @Currency,
                        TVARate = @TVARate,
                        TotalHT = @TotalHT,
                        TotalTVA = @TotalTVA,
                        TotalTTC = @TotalTTC,
                        Remise = @Remise,
                        TotalAfterRemise = @TotalAfterRemise,
                        EtatFacture = @EtatFacture,
                        IsReversed = @IsReversed,
                        Description = @Description,
                        LogoPath = @LogoPath,
                        ModifiedDate = GETDATE(),
                        ModifiedBy = @ModifiedBy
                    WHERE InvoiceID = @InvoiceID";

                        SqlCommand cmd = new SqlCommand(updateQuery, conn, transaction);
                        AddInvoiceParameters(cmd, invoice);
                        cmd.Parameters.AddWithValue("@InvoiceID", invoice.InvoiceID);

                        await cmd.ExecuteNonQueryAsync();

                        string deleteArticlesQuery = "DELETE FROM InvoiceArticle WHERE InvoiceID = @InvoiceID";
                        SqlCommand deleteCmd = new SqlCommand(deleteArticlesQuery, conn, transaction);
                        deleteCmd.Parameters.AddWithValue("@InvoiceID", invoice.InvoiceID);
                        await deleteCmd.ExecuteNonQueryAsync();

                        foreach (var article in invoice.Articles)
                        {
                            await InsertInvoiceArticleAsync(conn, transaction, invoice.InvoiceID, article);
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }


        // Soft Delete Invoice
        public async Task<bool> DeleteInvoiceAsync(int invoiceId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                string query = @"
                    UPDATE Invoice 
                    SET IsDeleted = 1, ModifiedDate = GETDATE() 
                    WHERE InvoiceID = @InvoiceID";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@InvoiceID", invoiceId);

                int rowsAffected = await cmd.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
        }

        // Hard Delete Invoice (use with caution)
        public async Task<bool> HardDeleteInvoiceAsync(int invoiceId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                string query = "DELETE FROM Invoice WHERE InvoiceID = @InvoiceID";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@InvoiceID", invoiceId);

                int rowsAffected = await cmd.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
        }

        #endregion

        #region Invoice Articles Operations

        private async Task InsertInvoiceArticleAsync(SqlConnection conn, SqlTransaction transaction, int invoiceId, Invoice.InvoiceArticle article)
        {
            string query = @"
                INSERT INTO InvoiceArticle (
                    InvoiceID, OperationID, ArticleID, ArticleName,
                    PrixUnitaire, Quantite, TVA, IsReversed
                ) VALUES (
                    @InvoiceID, @OperationID, @ArticleID, @ArticleName,
                    @PrixUnitaire, @Quantite, @TVA, @IsReversed
                )";

            SqlCommand cmd = new SqlCommand(query, conn, transaction);
            cmd.Parameters.AddWithValue("@InvoiceID", invoiceId);
            cmd.Parameters.AddWithValue("@OperationID", (object)article.OperationID ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ArticleID", article.ArticleID);
            cmd.Parameters.AddWithValue("@ArticleName", article.ArticleName);
            cmd.Parameters.AddWithValue("@PrixUnitaire", article.PrixUnitaire);
            cmd.Parameters.AddWithValue("@Quantite", article.Quantite);
            cmd.Parameters.AddWithValue("@TVA", article.TVA);
            cmd.Parameters.AddWithValue("@IsReversed", article.IsReversed);

            await cmd.ExecuteNonQueryAsync();
        }

        // Add this method to your InvoiceRepository class in Invoice.cs
        // Place it in the "Invoice Articles Operations" region

        /// <summary>
        /// Gets all articles for a specific invoice using the database table directly
        /// </summary>
        public async Task<List<Invoice.InvoiceArticle>> GetInvoiceArticlesAsync(int invoiceId)
        {
            List<Invoice.InvoiceArticle> articles = new List<Invoice.InvoiceArticle>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                string query = @"
            SELECT 
                InvoiceArticleID,
                InvoiceID,
                OperationID,
                ArticleID,
                ArticleName,
                PrixUnitaire,
                Quantite,
                TVA,
                IsReversed,
                CreatedDate,
                IsDeleted
            FROM InvoiceArticle 
            WHERE InvoiceID = @InvoiceID 
                AND IsDeleted = 0
            ORDER BY InvoiceArticleID";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@InvoiceID", invoiceId);

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        articles.Add(new Invoice.InvoiceArticle
                        {
                            InvoiceArticleID = reader.GetInt32(reader.GetOrdinal("InvoiceArticleID")),
                            InvoiceID = reader.GetInt32(reader.GetOrdinal("InvoiceID")),
                            OperationID = reader.IsDBNull(reader.GetOrdinal("OperationID"))
                                ? (int?)null
                                : reader.GetInt32(reader.GetOrdinal("OperationID")),
                            ArticleID = reader.GetInt32(reader.GetOrdinal("ArticleID")),
                            ArticleName = reader.GetString(reader.GetOrdinal("ArticleName")),
                            PrixUnitaire = reader.GetDecimal(reader.GetOrdinal("PrixUnitaire")),
                            Quantite = reader.GetDecimal(reader.GetOrdinal("Quantite")),
                            TVA = reader.GetDecimal(reader.GetOrdinal("TVA")),
                            IsReversed = reader.GetBoolean(reader.GetOrdinal("IsReversed")),
                            CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                            IsDeleted = reader.GetBoolean(reader.GetOrdinal("IsDeleted"))
                        });
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine($"GetInvoiceArticlesAsync: Loaded {articles.Count} articles for invoice {invoiceId}");
            return articles;
        }

        #endregion

        #region Helper Methods

        private void AddInvoiceParameters(SqlCommand cmd, Invoice invoice)
        {
            cmd.Parameters.AddWithValue("@InvoiceNumber", invoice.InvoiceNumber ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@InvoiceDate", invoice.InvoiceDate);
            cmd.Parameters.AddWithValue("@InvoiceType", invoice.InvoiceType ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@InvoiceIndex", invoice.InvoiceIndex ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@CreditClientName", invoice.CreditClientName ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@CreditMontant", invoice.CreditMontant);
            cmd.Parameters.AddWithValue("@Objet", invoice.Objet ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@NumberLetters", invoice.NumberLetters ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@NameFactureGiven", invoice.NameFactureGiven ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@NameFactureReceiver", invoice.NameFactureReceiver ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ReferenceClient", invoice.ReferenceClient ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@UserName", invoice.UserName ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@UserICE", invoice.UserICE ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@UserVAT", invoice.UserVAT ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@UserPhone", invoice.UserPhone ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@UserAddress", invoice.UserAddress ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@UserEtatJuridique", invoice.UserEtatJuridique ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@UserIdSociete", invoice.UserIdSociete ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@UserSiegeEntreprise", invoice.UserSiegeEntreprise ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ClientName", invoice.ClientName ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ClientICE", invoice.ClientICE ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ClientVAT", invoice.ClientVAT ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ClientPhone", invoice.ClientPhone ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ClientAddress", invoice.ClientAddress ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ClientEtatJuridique", invoice.ClientEtatJuridique ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ClientIdSociete", invoice.ClientIdSociete ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ClientSiegeEntreprise", invoice.ClientSiegeEntreprise ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Currency", invoice.Currency ?? "DH");
            cmd.Parameters.AddWithValue("@TVARate", invoice.TVARate);
            cmd.Parameters.AddWithValue("@TotalHT", invoice.TotalHT);
            cmd.Parameters.AddWithValue("@TotalTVA", invoice.TotalTVA);
            cmd.Parameters.AddWithValue("@TotalTTC", invoice.TotalTTC);
            cmd.Parameters.AddWithValue("@Remise", invoice.Remise);
            cmd.Parameters.AddWithValue("@TotalAfterRemise", invoice.TotalAfterRemise);
            cmd.Parameters.AddWithValue("@EtatFacture", invoice.EtatFacture);
            cmd.Parameters.AddWithValue("@IsReversed", invoice.IsReversed);
            cmd.Parameters.AddWithValue("@Description", invoice.Description ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@LogoPath", invoice.LogoPath ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@CreatedBy", invoice.CreatedBy ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ModifiedBy", invoice.ModifiedBy ?? (object)DBNull.Value);
        }
        public async Task<bool> InvoiceNumberExistsAsync(string invoiceNumber)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                string query = "SELECT COUNT(*) FROM Invoice WHERE InvoiceNumber = @InvoiceNumber";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@InvoiceNumber", invoiceNumber);

                int count = (int)await cmd.ExecuteScalarAsync();
                return count > 0;
            }
        }
        // Add this helper method to safely get column ordinal
        private int GetOrdinalSafe(SqlDataReader reader, string columnName)
        {
            try
            {
                return reader.GetOrdinal(columnName);
            }
            catch
            {
                return -1;
            }
        }

        // Helper method to safely read decimal values
        private decimal GetSafeDecimal(SqlDataReader reader, string columnName)
        {
            try
            {
                int ordinal = GetOrdinalSafe(reader, columnName);
                if (ordinal == -1 || reader.IsDBNull(ordinal))
                {
                    return 0;
                }

                // Get the SQL type name to handle it appropriately
                string typeName = reader.GetDataTypeName(ordinal);

                // Handle different SQL types
                switch (typeName.ToLower())
                {
                    case "decimal":
                    case "numeric":
                    case "money":
                    case "smallmoney":
                        return reader.GetDecimal(ordinal);

                    case "int":
                    case "smallint":
                    case "tinyint":
                        return Convert.ToDecimal(reader.GetInt32(ordinal));

                    case "bigint":
                        return Convert.ToDecimal(reader.GetInt64(ordinal));

                    case "float":
                    case "real":
                        return Convert.ToDecimal(reader.GetDouble(ordinal));

                    default:
                        // Fallback: try to convert the value
                        object value = reader.GetValue(ordinal);
                        return Convert.ToDecimal(value);
                }
            }
            catch (Exception ex)
            {
                // Log the error if you have logging
                System.Diagnostics.Debug.WriteLine($"Error reading {columnName}: {ex.Message}");
                return 0;
            }
        }

        // Helper method to safely read nullable decimal values
        private decimal? GetSafeNullableDecimal(SqlDataReader reader, string columnName)
        {
            try
            {
                int ordinal = GetOrdinalSafe(reader, columnName);
                if (ordinal == -1 || reader.IsDBNull(ordinal))
                {
                    return null;
                }

                return GetSafeDecimal(reader, columnName);
            }
            catch
            {
                return null;
            }
        }

        // Helper method to safely read integers
        private int GetSafeInt32(SqlDataReader reader, string columnName, int defaultValue = 0)
        {
            try
            {
                int ordinal = GetOrdinalSafe(reader, columnName);
                if (ordinal == -1 || reader.IsDBNull(ordinal))
                {
                    return defaultValue;
                }

                return reader.GetInt32(ordinal);
            }
            catch
            {
                return defaultValue;
            }
        }

        // Helper method to safely read strings
        private string GetSafeString(SqlDataReader reader, string columnName)
        {
            try
            {
                int ordinal = GetOrdinalSafe(reader, columnName);
                if (ordinal == -1 || reader.IsDBNull(ordinal))
                {
                    return null;
                }

                return reader.GetString(ordinal);
            }
            catch
            {
                return null;
            }
        }

        // Helper method to safely read booleans
        private bool GetSafeBoolean(SqlDataReader reader, string columnName, bool defaultValue = false)
        {
            try
            {
                int ordinal = GetOrdinalSafe(reader, columnName);
                if (ordinal == -1 || reader.IsDBNull(ordinal))
                {
                    return defaultValue;
                }

                return reader.GetBoolean(ordinal);
            }
            catch
            {
                return defaultValue;
            }
        }

        // Helper method to safely read DateTime
        private DateTime GetSafeDateTime(SqlDataReader reader, string columnName)
        {
            try
            {
                int ordinal = GetOrdinalSafe(reader, columnName);
                if (ordinal == -1 || reader.IsDBNull(ordinal))
                {
                    return DateTime.MinValue;
                }

                return reader.GetDateTime(ordinal);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        // Helper method to safely read nullable DateTime

        // Helper method to safely read nullable DateTime
        private DateTime? GetSafeNullableDateTime(SqlDataReader reader, string columnName)
        {
            try
            {
                int ordinal = GetOrdinalSafe(reader, columnName);
                if (ordinal == -1 || reader.IsDBNull(ordinal))
                {
                    return null;
                }

                return reader.GetDateTime(ordinal);
            }
            catch
            {
                return null;
            }
        }

        private Invoice MapInvoiceFromReader(SqlDataReader reader)
        {
            return new Invoice
            {
                InvoiceID = GetSafeInt32(reader, "InvoiceID"),
                InvoiceNumber = GetSafeString(reader, "InvoiceNumber"),
                InvoiceDate = GetSafeDateTime(reader, "InvoiceDate"),
                InvoiceType = GetSafeString(reader, "InvoiceType"),
                InvoiceIndex = GetSafeString(reader, "InvoiceIndex"),
                CreditClientName = GetSafeString(reader, "CreditClientName"),
                CreditMontant = GetSafeDecimal(reader, "CreditMontant"),
                Objet = GetSafeString(reader, "Objet"),
                NumberLetters = GetSafeString(reader, "NumberLetters"),
                NameFactureGiven = GetSafeString(reader, "NameFactureGiven"),
                NameFactureReceiver = GetSafeString(reader, "NameFactureReceiver"),
                ReferenceClient = GetSafeString(reader, "ReferenceClient"),
                UserName = GetSafeString(reader, "UserName"),
                UserICE = GetSafeString(reader, "UserICE"),
                UserVAT = GetSafeString(reader, "UserVAT"),
                UserPhone = GetSafeString(reader, "UserPhone"),
                UserAddress = GetSafeString(reader, "UserAddress"),
                UserEtatJuridique = GetSafeString(reader, "UserEtatJuridique"),
                UserIdSociete = GetSafeString(reader, "UserIdSociete"),
                UserSiegeEntreprise = GetSafeString(reader, "UserSiegeEntreprise"),
                ClientName = GetSafeString(reader, "ClientName"),
                ClientICE = GetSafeString(reader, "ClientICE"),
                ClientVAT = GetSafeString(reader, "ClientVAT"),
                ClientPhone = GetSafeString(reader, "ClientPhone"),
                ClientAddress = GetSafeString(reader, "ClientAddress"),
                ClientEtatJuridique = GetSafeString(reader, "ClientEtatJuridique"),
                ClientIdSociete = GetSafeString(reader, "ClientIdSociete"),
                ClientSiegeEntreprise = GetSafeString(reader, "ClientSiegeEntreprise"),
                Currency = GetSafeString(reader, "Currency") ?? "DH",

                // Decimal fields with safe reading
                TVARate = GetSafeDecimal(reader, "TVARate"),
                TotalHT = GetSafeDecimal(reader, "TotalHT"),
                TotalTVA = GetSafeDecimal(reader, "TotalTVA"),
                TotalTTC = GetSafeDecimal(reader, "TotalTTC"),
                Remise = GetSafeDecimal(reader, "Remise"),
                TotalAfterRemise = GetSafeDecimal(reader, "TotalAfterRemise"),

                EtatFacture = GetSafeInt32(reader, "EtatFacture"),
                IsReversed = GetSafeBoolean(reader, "IsReversed"),
                Description = GetSafeString(reader, "Description"),
                LogoPath = GetSafeString(reader, "LogoPath"),
                CreatedDate = GetSafeDateTime(reader, "CreatedDate"),
                CreatedBy = GetSafeInt32(reader, "CreatedBy") == 0 ? (int?)null : GetSafeInt32(reader, "CreatedBy"),
                ModifiedDate = GetSafeNullableDateTime(reader, "ModifiedDate"),
                ModifiedBy = GetSafeInt32(reader, "ModifiedBy") == 0 ? (int?)null : GetSafeInt32(reader, "ModifiedBy"),
                IsDeleted = GetSafeBoolean(reader, "IsDeleted")
            };
        }

        #endregion
    }
}