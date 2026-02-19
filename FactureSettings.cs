using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;

namespace GestionComerce
{
    public class FactureSettings
    {
        public int FactureSettingsID { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyPhone { get; set; }
        public string CompanyEmail { get; set; }
        public string LogoPath { get; set; }
        public string InvoicePrefix { get; set; }
        public decimal TaxPercentage { get; set; }
        public string TermsAndConditions { get; set; }
        public string FooterText { get; set; }

        private static readonly string ConnectionString = "Server=localhost\\SQLEXPRESS;Database=GESTIONCOMERCEP;Trusted_Connection=True;";

        // Get the current facture settings (should only be one row)
        public static async Task<FactureSettings> GetFactureSettingsAsync()
        {
            FactureSettings settings = new FactureSettings();
            string query = "SELECT TOP 1 * FROM FactureSettings ORDER BY FactureSettingsID DESC";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        settings.FactureSettingsID = Convert.ToInt32(reader["FactureSettingsID"]);
                        settings.CompanyName = reader["CompanyName"]?.ToString() ?? "";
                        settings.CompanyAddress = reader["CompanyAddress"]?.ToString() ?? "";
                        settings.CompanyPhone = reader["CompanyPhone"]?.ToString() ?? "";
                        settings.CompanyEmail = reader["CompanyEmail"]?.ToString() ?? "";
                        settings.LogoPath = reader["LogoPath"]?.ToString() ?? "";
                        settings.InvoicePrefix = reader["InvoicePrefix"]?.ToString() ?? "FAC-";
                        settings.TaxPercentage = reader["TaxPercentage"] != DBNull.Value ? Convert.ToDecimal(reader["TaxPercentage"]) : 20.00m;
                        settings.TermsAndConditions = reader["TermsAndConditions"]?.ToString() ?? "";
                        settings.FooterText = reader["FooterText"]?.ToString() ?? "";
                    }
                }
            }

            return settings;
        }

        // ADDED: Alias method for LoadSettingsAsync (calls GetFactureSettingsAsync)
        // This is used by WSelectCient when printing tickets
        public static async Task<FactureSettings> LoadSettingsAsync()
        {
            try
            {
                FactureSettings settings = await GetFactureSettingsAsync();

                // If no settings found in database, return defaults
                if (settings.FactureSettingsID == 0)
                {
                    settings.CompanyName = "NOM DE L'ENTREPRISE";
                    settings.CompanyAddress = "Adresse de l'entreprise";
                    settings.CompanyPhone = "0600000000";
                    settings.CompanyEmail = "email@entreprise.ma";
                    settings.LogoPath = "";
                    settings.InvoicePrefix = "FAC-";
                    settings.TaxPercentage = 20;
                    settings.TermsAndConditions = "";
                    settings.FooterText = "MERCI DE VOTRE VISITE";
                }

                return settings;
            }
            catch (Exception ex)
            {
                // If any error occurs, return default settings
                return new FactureSettings
                {
                    CompanyName = "NOM DE L'ENTREPRISE",
                    CompanyAddress = "Adresse de l'entreprise",
                    CompanyPhone = "0600000000",
                    CompanyEmail = "email@entreprise.ma",
                    LogoPath = "",
                    InvoicePrefix = "FAC-",
                    TaxPercentage = 20,
                    TermsAndConditions = "",
                    FooterText = "MERCI DE VOTRE VISITE"
                };
            }
        }

        // Save or Update facture settings
        public async Task<int> SaveFactureSettingsAsync()
        {
            string query;

            // Check if settings exist
            if (FactureSettingsID > 0)
            {
                // Update existing settings
                query = @"UPDATE FactureSettings SET 
                         CompanyName=@CompanyName, 
                         CompanyAddress=@CompanyAddress, 
                         CompanyPhone=@CompanyPhone, 
                         CompanyEmail=@CompanyEmail, 
                         LogoPath=@LogoPath, 
                         InvoicePrefix=@InvoicePrefix, 
                         TaxPercentage=@TaxPercentage, 
                         TermsAndConditions=@TermsAndConditions, 
                         FooterText=@FooterText,
                         UpdatedDate=GETDATE()
                         WHERE FactureSettingsID=@FactureSettingsID";
            }
            else
            {
                // Insert new settings
                query = @"INSERT INTO FactureSettings 
                         (CompanyName, CompanyAddress, CompanyPhone, CompanyEmail, LogoPath, 
                          InvoicePrefix, TaxPercentage, TermsAndConditions, FooterText) 
                         VALUES 
                         (@CompanyName, @CompanyAddress, @CompanyPhone, @CompanyEmail, @LogoPath, 
                          @InvoicePrefix, @TaxPercentage, @TermsAndConditions, @FooterText);
                         SELECT SCOPE_IDENTITY();";
            }

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                try
                {
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@CompanyName", CompanyName ?? "");
                        cmd.Parameters.AddWithValue("@CompanyAddress", CompanyAddress ?? "");
                        cmd.Parameters.AddWithValue("@CompanyPhone", CompanyPhone ?? "");
                        cmd.Parameters.AddWithValue("@CompanyEmail", CompanyEmail ?? "");
                        cmd.Parameters.AddWithValue("@LogoPath", LogoPath ?? "");
                        cmd.Parameters.AddWithValue("@InvoicePrefix", InvoicePrefix ?? "FAC-");
                        cmd.Parameters.AddWithValue("@TaxPercentage", TaxPercentage);
                        cmd.Parameters.AddWithValue("@TermsAndConditions", TermsAndConditions ?? "");
                        cmd.Parameters.AddWithValue("@FooterText", FooterText ?? "");

                        if (FactureSettingsID > 0)
                        {
                            cmd.Parameters.AddWithValue("@FactureSettingsID", FactureSettingsID);
                            await cmd.ExecuteNonQueryAsync();
                            return 1;
                        }
                        else
                        {
                            object result = await cmd.ExecuteScalarAsync();
                            return Convert.ToInt32(result);
                        }
                    }
                }
                catch (Exception err)
                {
                    MessageBox.Show($"Erreur lors de l'enregistrement des paramètres de facture: {err.Message}");
                    return 0;
                }
            }
        }
    }
}