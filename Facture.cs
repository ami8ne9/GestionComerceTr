using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;

namespace Superete
{
    public class Facture
    {
        public int FactureID { get; set; }
        public string Name { get; set; } = "";
        public string ICE { get; set; } = "";
        public string VAT { get; set; } = "";
        public string Telephone { get; set; } = "";
        public string Adresse { get; set; } = "";
        public bool Etat { get; set; }

        // Existing fields
        public string CompanyId { get; set; } = "";
        public string EtatJuridic { get; set; } = "";
        public string SiegeEntreprise { get; set; } = "";
        public string LogoPath { get; set; } = "";

        // 🆕 NEW MOROCCAN BUSINESS FIELDS
        public string IF { get; set; } = "";                    // Identifiant Fiscal
        public string CNSS { get; set; } = "";                  // Caisse Nationale de Sécurité Sociale
        public string RC { get; set; } = "";                    // Registre de Commerce
        public string TP { get; set; } = "";                    // Taxe Professionnelle
        public string RIB { get; set; } = "";                   // Relevé d'Identité Bancaire (24 digits)
        public string Email { get; set; } = "";                 // Email professionnel
        public string SiteWeb { get; set; } = "";               // Site web de l'entreprise
        public string Patente { get; set; } = "";               // Numéro de Patente
        public string Capital { get; set; } = "";               // Capital social
        public string Fax { get; set; } = "";                   // Numéro de fax
        public string Ville { get; set; } = "";                 // Ville
        public string CodePostal { get; set; } = "";            // Code postal
        public string BankName { get; set; } = "";              // Nom de la banque
        public string AgencyCode { get; set; } = "";            // Code agence bancaire

        private static readonly string ConnectionString =
            "Server=localhost\\SQLEXPRESS;Database=GESTIONCOMERCEP;Trusted_Connection=True;";

        // Get single active facture
        public async Task<Facture> GetFactureAsync()
        {
            string query = "SELECT TOP 1 * FROM Facture WHERE Etat = 1";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        Facture f = new Facture
                        {
                            FactureID = Convert.ToInt32(reader["FactureID"]),
                            Name = reader["Name"] != DBNull.Value ? reader["Name"].ToString() : "",
                            ICE = reader["ICE"] != DBNull.Value ? reader["ICE"].ToString() : "",
                            VAT = reader["VAT"] != DBNull.Value ? reader["VAT"].ToString() : "",
                            Telephone = reader["Telephone"] != DBNull.Value ? reader["Telephone"].ToString() : "",
                            Adresse = reader["Adresse"] != DBNull.Value ? reader["Adresse"].ToString() : "",
                            Etat = reader["Etat"] != DBNull.Value && Convert.ToBoolean(reader["Etat"]),
                            CompanyId = reader["CompanyId"] != DBNull.Value ? reader["CompanyId"].ToString() : "",
                            EtatJuridic = reader["EtatJuridic"] != DBNull.Value ? reader["EtatJuridic"].ToString() : "",
                            SiegeEntreprise = reader["SiegeEntreprise"] != DBNull.Value ? reader["SiegeEntreprise"].ToString() : "",
                            LogoPath = reader["LogoPath"] != DBNull.Value ? reader["LogoPath"].ToString() : "",

                            // Load new Moroccan fields
                            IF = reader["IF"] != DBNull.Value ? reader["IF"].ToString() : "",
                            CNSS = reader["CNSS"] != DBNull.Value ? reader["CNSS"].ToString() : "",
                            RC = reader["RC"] != DBNull.Value ? reader["RC"].ToString() : "",
                            TP = reader["TP"] != DBNull.Value ? reader["TP"].ToString() : "",
                            RIB = reader["RIB"] != DBNull.Value ? reader["RIB"].ToString() : "",
                            Email = reader["Email"] != DBNull.Value ? reader["Email"].ToString() : "",
                            SiteWeb = reader["SiteWeb"] != DBNull.Value ? reader["SiteWeb"].ToString() : "",
                            Patente = reader["Patente"] != DBNull.Value ? reader["Patente"].ToString() : "",
                            Capital = reader["Capital"] != DBNull.Value ? reader["Capital"].ToString() : "",
                            Fax = reader["Fax"] != DBNull.Value ? reader["Fax"].ToString() : "",
                            Ville = reader["Ville"] != DBNull.Value ? reader["Ville"].ToString() : "",
                            CodePostal = reader["CodePostal"] != DBNull.Value ? reader["CodePostal"].ToString() : "",
                            BankName = reader["BankName"] != DBNull.Value ? reader["BankName"].ToString() : "",
                            AgencyCode = reader["AgencyCode"] != DBNull.Value ? reader["AgencyCode"].ToString() : ""
                        };

                        return f;
                    }
                }
            }

            return null;
        }

        // Insert or Update facture
        public async Task<int> InsertOrUpdateFactureAsync()
        {
            Facture existing = await GetFactureAsync();

            if (existing == null)
            {
                string insertQuery = @"
                    INSERT INTO Facture (
                        Name, ICE, VAT, Telephone, Adresse, CompanyId, EtatJuridic, SiegeEntreprise, LogoPath,
                        [IF], CNSS, RC, TP, RIB, Email, SiteWeb, Patente, Capital, Fax, Ville, CodePostal, BankName, AgencyCode, Etat
                    )
                    VALUES (
                        @Name, @ICE, @VAT, @Telephone, @Adresse, @CompanyId, @EtatJuridic, @SiegeEntreprise, @LogoPath,
                        @IF, @CNSS, @RC, @TP, @RIB, @Email, @SiteWeb, @Patente, @Capital, @Fax, @Ville, @CodePostal, @BankName, @AgencyCode, 1
                    )";

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand(insertQuery, connection))
                        {
                            AddParameters(cmd);
                            await cmd.ExecuteNonQueryAsync();
                            return 1;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Facture not inserted: " + ex.Message);
                        return 0;
                    }
                }
            }
            else
            {
                string updateQuery = @"
                    UPDATE Facture
                    SET Name=@Name, ICE=@ICE, VAT=@VAT, Telephone=@Telephone, Adresse=@Adresse,
                        CompanyId=@CompanyId, EtatJuridic=@EtatJuridic, SiegeEntreprise=@SiegeEntreprise, LogoPath=@LogoPath,
                        [IF]=@IF, CNSS=@CNSS, RC=@RC, TP=@TP, RIB=@RIB, Email=@Email, SiteWeb=@SiteWeb,
                        Patente=@Patente, Capital=@Capital, Fax=@Fax, Ville=@Ville, CodePostal=@CodePostal,
                        BankName=@BankName, AgencyCode=@AgencyCode
                    WHERE FactureID=@FactureID";

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand(updateQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@FactureID", existing.FactureID);
                            AddParameters(cmd);
                            await cmd.ExecuteNonQueryAsync();
                            return 2;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Facture not updated: " + ex.Message);
                        return 0;
                    }
                }
            }
        }

        private void AddParameters(SqlCommand cmd)
        {
            cmd.Parameters.AddWithValue("@Name", (object)Name ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ICE", (object)ICE ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@VAT", (object)VAT ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Telephone", (object)Telephone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Adresse", (object)Adresse ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CompanyId", (object)CompanyId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EtatJuridic", (object)EtatJuridic ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@SiegeEntreprise", (object)SiegeEntreprise ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@LogoPath", (object)LogoPath ?? DBNull.Value);

            // New Moroccan fields
            cmd.Parameters.AddWithValue("@IF", (object)IF ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CNSS", (object)CNSS ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@RC", (object)RC ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TP", (object)TP ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@RIB", (object)RIB ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Email", (object)Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@SiteWeb", (object)SiteWeb ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Patente", (object)Patente ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Capital", (object)Capital ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Fax", (object)Fax ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Ville", (object)Ville ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CodePostal", (object)CodePostal ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@BankName", (object)BankName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@AgencyCode", (object)AgencyCode ?? DBNull.Value);
        }

        // Soft delete facture
        public async Task<int> DeleteFactureAsync()
        {
            string query = "UPDATE Facture SET Etat = 0 WHERE FactureID=@FactureID";
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                try
                {
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@FactureID", FactureID);
                        await cmd.ExecuteNonQueryAsync();
                        return 1;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Facture not deleted: " + ex.Message);
                    return 0;
                }
            }
        }
    }
}