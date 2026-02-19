using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GestionComerce.Main.Facturation
{
    public class FactureEnregistree
    {
        public int SavedInvoiceID { get; set; }
        public byte[] InvoiceImage { get; set; }  // Store actual image binary data
        public string ImageFileName { get; set; }  // Original filename
        public int FournisseurID { get; set; }
        public string FournisseurNom { get; set; } // For display purposes
        public decimal TotalAmount { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string Description { get; set; }
        public string InvoiceReference { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        // Display properties for DataGrid
        public string Fournisseur
        {
            get { return FournisseurNom; }
        }

        public decimal? Montant
        {
            get { return TotalAmount; }
        }

        public DateTime DateReception
        {
            get { return CreatedDate; }
        }

        public string NumeroFacture
        {
            get { return InvoiceReference; }
        }

        public string InvoiceImagePath
        {
            get { return ImageFileName; }
            set { ImageFileName = value; }
        }

        public FactureEnregistree()
        {
            InvoiceDate = DateTime.Now;
            CreatedDate = DateTime.Now;
            UpdatedDate = DateTime.Now;
        }

        // Helper method to get connection string
        private static string GetConnectionString()
        {
            return @"Server=localhost\SQLEXPRESS;Database=GESTIONCOMERCEP;Trusted_Connection=True;";
        }

        // Get all invoices
        public static List<FactureEnregistree> GetAllInvoices()
        {
            return GetAllSavedInvoicesAsync();
        }

        // Method with different name for compatibility - FIXED
        public static List<FactureEnregistree> GetAllSavedInvoicesAsync()
        {
            List<FactureEnregistree> invoices = new List<FactureEnregistree>();
            string query = @"SELECT 
                            si.SavedInvoiceID, si.InvoiceImage, si.ImageFileName, si.FournisseurID, 
                            si.TotalAmount, si.InvoiceDate, si.Description, 
                            si.InvoiceReference, si.Notes, si.CreatedDate, si.UpdatedDate,
                            ISNULL(f.Nom, 'N/A') as FournisseurNom
                            FROM SavedInvoices si
                            LEFT JOIN Fournisseur f ON si.FournisseurID = f.FournisseurID
                            ORDER BY si.InvoiceDate DESC";

            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                SqlCommand cmd = new SqlCommand(query, conn);

                try
                {
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        invoices.Add(new FactureEnregistree
                        {
                            SavedInvoiceID = (int)reader["SavedInvoiceID"],
                            InvoiceImage = reader["InvoiceImage"] as byte[],
                            ImageFileName = reader["ImageFileName"] as string,
                            FournisseurID = (int)reader["FournisseurID"],
                            FournisseurNom = reader["FournisseurNom"].ToString(),
                            TotalAmount = (decimal)reader["TotalAmount"],
                            InvoiceDate = (DateTime)reader["InvoiceDate"],
                            Description = reader["Description"] as string,
                            InvoiceReference = reader["InvoiceReference"] as string,
                            Notes = reader["Notes"] as string,
                            CreatedDate = (DateTime)reader["CreatedDate"],
                            UpdatedDate = (DateTime)reader["UpdatedDate"]
                        });
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Erreur lors de la récupération des factures: " + ex.Message);
                }
            }

            return invoices;
        }

        // Insert new invoice
        public bool InsertSavedInvoiceAsync()
        {
            return Insert();
        }

        // Main insert method
        public bool Insert()
        {
            string query = @"INSERT INTO SavedInvoices 
                            (InvoiceImage, ImageFileName, FournisseurID, TotalAmount, InvoiceDate, 
                            Description, InvoiceReference, Notes, CreatedDate, UpdatedDate) 
                            VALUES 
                            (@InvoiceImage, @ImageFileName, @FournisseurID, @TotalAmount, @InvoiceDate, 
                            @Description, @InvoiceReference, @Notes, @CreatedDate, @UpdatedDate);
                            SELECT CAST(SCOPE_IDENTITY() as int)";

            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@InvoiceImage", InvoiceImage);
                cmd.Parameters.AddWithValue("@ImageFileName", (object)ImageFileName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FournisseurID", FournisseurID);
                cmd.Parameters.AddWithValue("@TotalAmount", TotalAmount);
                cmd.Parameters.AddWithValue("@InvoiceDate", InvoiceDate);
                cmd.Parameters.AddWithValue("@Description", (object)Description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@InvoiceReference", (object)InvoiceReference ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Notes", (object)Notes ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CreatedDate", CreatedDate);
                cmd.Parameters.AddWithValue("@UpdatedDate", UpdatedDate);

                try
                {
                    conn.Open();
                    // AFTER
                    this.SavedInvoiceID = (int)cmd.ExecuteScalar();

                    // Hook: Send to accounting
                    // REPLACE your existing achat hook with this:
                    try
                    {
                        DateTime safeDate = (this.InvoiceDate == DateTime.MinValue || this.InvoiceDate.Year < 1753)
                            ? DateTime.Now
                            : this.InvoiceDate;

                        var comptaService = new GestionComerce.ComptabiliteService();
                        comptaService.EnregistrerAchat(
                            this.SavedInvoiceID,
                            this.TotalAmount,
                            this.InvoiceReference ?? string.Format("FAC-{0}", this.SavedInvoiceID),
                            this.FournisseurID,
                            safeDate
                        );
                    }
                    catch { }

                    return true;
                }
                catch (Exception ex)
                {
                    throw new Exception("Erreur lors de l'insertion de la facture: " + ex.Message);
                }
            }
        }

        // Update invoice
        public bool UpdateSavedInvoiceAsync()
        {
            return Update();
        }

        // Main update method
        public bool Update()
        {
            string query = @"UPDATE SavedInvoices 
                            SET InvoiceImage = @InvoiceImage,
                                ImageFileName = @ImageFileName,
                                FournisseurID = @FournisseurID,
                                TotalAmount = @TotalAmount, 
                                InvoiceDate = @InvoiceDate, 
                                Description = @Description, 
                                InvoiceReference = @InvoiceReference, 
                                Notes = @Notes
                            WHERE SavedInvoiceID = @SavedInvoiceID";

            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@SavedInvoiceID", SavedInvoiceID);
                cmd.Parameters.AddWithValue("@InvoiceImage", InvoiceImage);
                cmd.Parameters.AddWithValue("@ImageFileName", (object)ImageFileName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FournisseurID", FournisseurID);
                cmd.Parameters.AddWithValue("@TotalAmount", TotalAmount);
                cmd.Parameters.AddWithValue("@InvoiceDate", InvoiceDate);
                cmd.Parameters.AddWithValue("@Description", (object)Description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@InvoiceReference", (object)InvoiceReference ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Notes", (object)Notes ?? DBNull.Value);

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    return true;
                }
                catch (Exception ex)
                {
                    throw new Exception("Erreur lors de la mise à jour de la facture: " + ex.Message);
                }
            }
        }

        // Delete invoice
        public bool DeleteSavedInvoiceAsync()
        {
            return Delete();
        }

        // Main delete method
        public bool Delete()
        {
            string query = "DELETE FROM SavedInvoices WHERE SavedInvoiceID = @SavedInvoiceID";

            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@SavedInvoiceID", SavedInvoiceID);

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    return true;
                }
                catch (Exception ex)
                {
                    throw new Exception("Erreur lors de la suppression de la facture: " + ex.Message);
                }
            }
        }

        // Get invoice by ID - FIXED
        public static FactureEnregistree GetById(int id)
        {
            string query = @"SELECT 
                            si.SavedInvoiceID, si.InvoiceImage, si.ImageFileName, si.FournisseurID,
                            si.TotalAmount, si.InvoiceDate, si.Description, 
                            si.InvoiceReference, si.Notes, si.CreatedDate, si.UpdatedDate,
                            ISNULL(f.Nom, 'N/A') as FournisseurNom
                            FROM SavedInvoices si
                            LEFT JOIN Fournisseur f ON si.FournisseurID = f.FournisseurID
                            WHERE si.SavedInvoiceID = @SavedInvoiceID";

            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@SavedInvoiceID", id);

                try
                {
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        return new FactureEnregistree
                        {
                            SavedInvoiceID = (int)reader["SavedInvoiceID"],
                            InvoiceImage = reader["InvoiceImage"] as byte[],
                            ImageFileName = reader["ImageFileName"] as string,
                            FournisseurID = (int)reader["FournisseurID"],
                            FournisseurNom = reader["FournisseurNom"].ToString(),
                            TotalAmount = (decimal)reader["TotalAmount"],
                            InvoiceDate = (DateTime)reader["InvoiceDate"],
                            Description = reader["Description"] as string,
                            InvoiceReference = reader["InvoiceReference"] as string,
                            Notes = reader["Notes"] as string,
                            CreatedDate = (DateTime)reader["CreatedDate"],
                            UpdatedDate = (DateTime)reader["UpdatedDate"]
                        };
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Erreur lors de la récupération de la facture: " + ex.Message);
                }
            }

            return null;
        }

        // Get list of suppliers for ComboBox
        public static List<SupplierItem> GetSuppliers()
        {
            List<SupplierItem> suppliers = new List<SupplierItem>();
            string query = "SELECT SupplierID, SupplierName FROM Supplier ORDER BY SupplierName";

            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                SqlCommand cmd = new SqlCommand(query, conn);

                try
                {
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        suppliers.Add(new SupplierItem
                        {
                            SupplierID = (int)reader["SupplierID"],
                            SupplierName = reader["SupplierName"].ToString()
                        });
                    }
                }
                catch (Exception ex)
                {
                    // If Supplier table doesn't exist or error, return empty list
                    return new List<SupplierItem>();
                }
            }

            return suppliers;
        }
    }

    // Helper class for Supplier ComboBox
    public class SupplierItem
    {
        public int SupplierID { get; set; }
        public string SupplierName { get; set; }

        public override string ToString()
        {
            return SupplierName;
        }
    }
}