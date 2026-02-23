using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Windows;

namespace GestionComerce
{
    public class Article
    {
        public int ArticleID { get; set; }
        public int Quantite { get; set; }
        public decimal PrixAchat { get; set; }
        public decimal PrixVente { get; set; }
        public decimal PrixMP { get; set; }
        public int FamillyID { get; set; }
        public long Code { get; set; }
        public string ArticleName { get; set; }
        public bool Etat { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? DateExpiration { get; set; }
        public string marque { get; set; }
        public decimal tva { get; set; }
        public string numeroLot { get; set; }
        public string bonlivraison { get; set; }
        public DateTime? DateLivraison { get; set; }
        public int FournisseurID { get; set; }
        public byte[] ArticleImage { get; set; }
        public bool IsUnlimitedStock { get; set; }

        // NEW PACKAGING PROPERTIES
        public int PiecesPerPackage { get; set; } // Number of pieces in one package
        public string PackageType { get; set; } // Type of package (Box, Carton, Bag, Pallet, etc.)
        public decimal PackageWeight { get; set; } // Weight of one package in kg
        public string PackageDimensions { get; set; } // Dimensions (e.g., "30x20x15 cm")

        // NEW INVENTORY MANAGEMENT PROPERTIES
        public int MinimumStock { get; set; } // Minimum stock level (for reorder alerts)
        public int MaximumStock { get; set; } // Maximum stock level
        public string StorageLocation { get; set; } // Warehouse location (e.g., "Aisle 5, Shelf B")
        public string SKU { get; set; } // Stock Keeping Unit - alternative product code
        public string Description { get; set; } // Detailed product description
        public bool IsPerishable { get; set; } // Is the product perishable?
        public string UnitOfMeasure { get; set; } // Unit (piece, kg, liter, meter, etc.)
        public decimal PrixGros { get; set; } // Wholesale price
        public int MinQuantityForGros { get; set; } // Minimum quantity to apply wholesale price
        public string CountryOfOrigin { get; set; } // Country of origin
        public string Manufacturer { get; set; } // Manufacturer name
        public DateTime? LastRestockDate { get; set; } // Last date when stock was replenished
        public string Notes { get; set; } // Additional notes/comments

        private static readonly string ConnectionString = "Server=localhost\\SQLEXPRESS;Database=GESTIONCOMERCEP;Trusted_Connection=True;";

        public async Task<List<Article>> GetArticlesAsync()
        {
            var articles = new List<Article>();
            string query = "SELECT * FROM Article where Etat=1";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        articles.Add(MapReaderToArticle(reader));
                    }
                }
            }
            return articles;
        }

        public async Task<List<Article>> GetAllArticlesAsync()
        {
            var articles = new List<Article>();
            string query = "SELECT * FROM Article";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        articles.Add(MapReaderToArticle(reader));
                    }
                }
            }
            return articles;
        }

        // Helper method to map SqlDataReader to Article object
        private Article MapReaderToArticle(SqlDataReader reader)
        {
            return new Article
            {
                ArticleID = Convert.ToInt32(reader["ArticleID"]),
                Quantite = Convert.ToInt32(reader["Quantite"]),
                PrixAchat = Convert.ToDecimal(reader["PrixAchat"]),
                PrixVente = Convert.ToDecimal(reader["PrixVente"]),
                PrixMP = reader["PrixMP"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["PrixMP"]),
                FamillyID = Convert.ToInt32(reader["FamillyID"]),
                FournisseurID = Convert.ToInt32(reader["FournisseurID"]),
                Code = reader["Code"] == DBNull.Value ? 0 : Convert.ToInt64(reader["Code"]),
                ArticleName = reader["ArticleName"] == DBNull.Value ? string.Empty : reader["ArticleName"].ToString(),
                Date = reader["Date"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["Date"]),
                DateExpiration = reader["DateExpiration"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DateExpiration"]),
                marque = reader["marque"] == DBNull.Value ? string.Empty : reader["marque"].ToString(),
                tva = reader["tva"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tva"]),
                numeroLot = reader["numeroLot"] == DBNull.Value ? string.Empty : reader["numeroLot"].ToString(),
                bonlivraison = reader["bonlivraison"] == DBNull.Value ? string.Empty : reader["bonlivraison"].ToString(),
                DateLivraison = reader["DateLivraison"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DateLivraison"]),
                ArticleImage = reader["ArticleImage"] == DBNull.Value ? null : (byte[])reader["ArticleImage"],
                IsUnlimitedStock = reader["IsUnlimitedStock"] == DBNull.Value ? false : Convert.ToBoolean(reader["IsUnlimitedStock"]),

                // New packaging fields
                PiecesPerPackage = reader["PiecesPerPackage"] == DBNull.Value ? 1 : Convert.ToInt32(reader["PiecesPerPackage"]),
                PackageType = reader["PackageType"] == DBNull.Value ? string.Empty : reader["PackageType"].ToString(),
                PackageWeight = reader["PackageWeight"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["PackageWeight"]),
                PackageDimensions = reader["PackageDimensions"] == DBNull.Value ? string.Empty : reader["PackageDimensions"].ToString(),

                // New inventory management fields
                MinimumStock = reader["MinimumStock"] == DBNull.Value ? 0 : Convert.ToInt32(reader["MinimumStock"]),
                MaximumStock = reader["MaximumStock"] == DBNull.Value ? 0 : Convert.ToInt32(reader["MaximumStock"]),
                StorageLocation = reader["StorageLocation"] == DBNull.Value ? string.Empty : reader["StorageLocation"].ToString(),
                SKU = reader["SKU"] == DBNull.Value ? string.Empty : reader["SKU"].ToString(),
                Description = reader["Description"] == DBNull.Value ? string.Empty : reader["Description"].ToString(),
                IsPerishable = reader["IsPerishable"] == DBNull.Value ? false : Convert.ToBoolean(reader["IsPerishable"]),
                UnitOfMeasure = reader["UnitOfMeasure"] == DBNull.Value ? "piece" : reader["UnitOfMeasure"].ToString(),
                PrixGros = reader["PrixGros"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["PrixGros"]),
                MinQuantityForGros = reader["MinQuantityForGros"] == DBNull.Value ? 0 : Convert.ToInt32(reader["MinQuantityForGros"]),
                CountryOfOrigin = reader["CountryOfOrigin"] == DBNull.Value ? string.Empty : reader["CountryOfOrigin"].ToString(),
                Manufacturer = reader["Manufacturer"] == DBNull.Value ? string.Empty : reader["Manufacturer"].ToString(),
                LastRestockDate = reader["LastRestockDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["LastRestockDate"]),
                Notes = reader["Notes"] == DBNull.Value ? string.Empty : reader["Notes"].ToString(),

                Etat = Convert.ToBoolean(reader["Etat"])
            };
        }

        public async Task<int> InsertArticleAsync()
        {
            string query = @"INSERT INTO Article (
                Quantite, PrixAchat, PrixVente, PrixMP, FamillyID, Code, FournisseurID, ArticleName, 
                Date, DateExpiration, marque, tva, numeroLot, bonlivraison, DateLivraison, 
                ArticleImage, IsUnlimitedStock,
                PiecesPerPackage, PackageType, PackageWeight, PackageDimensions,
                MinimumStock, MaximumStock, StorageLocation, SKU, Description, 
                IsPerishable, UnitOfMeasure, PrixGros, MinQuantityForGros, 
                CountryOfOrigin, Manufacturer, LastRestockDate, Notes
            ) VALUES (
                @Quantite, @PrixAchat, @PrixVente, @PrixMP, @FamillyID, @Code, @FournisseurID, @ArticleName, 
                @Date, @DateExpiration, @marque, @tva, @numeroLot, @bonlivraison, @DateLivraison, 
                @ArticleImage, @IsUnlimitedStock,
                @PiecesPerPackage, @PackageType, @PackageWeight, @PackageDimensions,
                @MinimumStock, @MaximumStock, @StorageLocation, @SKU, @Description, 
                @IsPerishable, @UnitOfMeasure, @PrixGros, @MinQuantityForGros, 
                @CountryOfOrigin, @Manufacturer, @LastRestockDate, @Notes
            ); SELECT SCOPE_IDENTITY();";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                try
                {
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        AddParameters(cmd);
                        object result = await cmd.ExecuteScalarAsync();
                        int id = Convert.ToInt32(result);
                        return id;
                    }
                }
                catch (Exception err)
                {
                    MessageBox.Show($"Article not inserted, error: {err}");
                    return 0;
                }
            }
        }

        public async Task<int> DeleteArticleAsync()
        {
            string query = "UPDATE Article SET Etat=0 WHERE ArticleID=@ArticleID";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    try
                    {
                        cmd.Parameters.AddWithValue("@ArticleID", this.ArticleID);
                        await cmd.ExecuteNonQueryAsync();
                        return 1;
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show($"Article not deleted: {err}");
                        return 0;
                    }
                }
            }
        }

        public async Task<int> BringBackArticleAsync()
        {
            string query = "UPDATE Article SET Etat=1 WHERE ArticleID=@ArticleID";
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    try
                    {
                        cmd.Parameters.AddWithValue("@ArticleID", this.ArticleID);
                        await cmd.ExecuteNonQueryAsync();
                        return 1;
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show($"Article not deleted: {err}");
                        return 0;
                    }
                }
            }
        }

        public async Task<int> UpdateArticleAsync()
        {
            string query = @"UPDATE Article SET 
                Quantite=@Quantite, PrixAchat=@PrixAchat, PrixVente=@PrixVente, PrixMP=@PrixMP, 
                FamillyID=@FamillyID, Code=@Code, ArticleName=@ArticleName, Date=@Date, 
                DateExpiration=@DateExpiration, marque=@marque, tva=@tva, numeroLot=@numeroLot, 
                bonlivraison=@bonlivraison, DateLivraison=@DateLivraison, ArticleImage=@ArticleImage, 
                IsUnlimitedStock=@IsUnlimitedStock,
                PiecesPerPackage=@PiecesPerPackage, PackageType=@PackageType, 
                PackageWeight=@PackageWeight, PackageDimensions=@PackageDimensions,
                MinimumStock=@MinimumStock, MaximumStock=@MaximumStock, 
                StorageLocation=@StorageLocation, SKU=@SKU, Description=@Description, 
                IsPerishable=@IsPerishable, UnitOfMeasure=@UnitOfMeasure, 
                PrixGros=@PrixGros, MinQuantityForGros=@MinQuantityForGros, 
                CountryOfOrigin=@CountryOfOrigin, Manufacturer=@Manufacturer, 
                LastRestockDate=@LastRestockDate, Notes=@Notes
                WHERE ArticleID=@ArticleID";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    try
                    {
                        cmd.Parameters.AddWithValue("@ArticleID", this.ArticleID);
                        AddParameters(cmd);
                        await cmd.ExecuteNonQueryAsync();
                        return 1;
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show($"Article not updated: {err}");
                        return 0;
                    }
                }
            }
        }

        // Helper method to add parameters (used by both Insert and Update)
        private void AddParameters(SqlCommand cmd)
        {
            cmd.Parameters.AddWithValue("@Quantite", this.Quantite);
            cmd.Parameters.AddWithValue("@PrixAchat", this.PrixAchat);
            cmd.Parameters.AddWithValue("@PrixVente", this.PrixVente);
            cmd.Parameters.AddWithValue("@PrixMP", this.PrixMP);
            cmd.Parameters.AddWithValue("@FamillyID", this.FamillyID);
            cmd.Parameters.AddWithValue("@Code", this.Code);
            cmd.Parameters.AddWithValue("@FournisseurID", this.FournisseurID);
            cmd.Parameters.AddWithValue("@ArticleName", this.ArticleName ?? string.Empty);
            cmd.Parameters.AddWithValue("@Date", this.Date.HasValue ? (object)this.Date.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@DateExpiration", this.DateExpiration.HasValue ? (object)this.DateExpiration.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@marque", this.marque ?? string.Empty);
            cmd.Parameters.AddWithValue("@tva", this.tva);
            cmd.Parameters.AddWithValue("@numeroLot", this.numeroLot ?? string.Empty);
            cmd.Parameters.AddWithValue("@bonlivraison", this.bonlivraison ?? string.Empty);
            cmd.Parameters.AddWithValue("@DateLivraison", this.DateLivraison.HasValue ? (object)this.DateLivraison.Value : DBNull.Value);

            if (this.ArticleImage != null && this.ArticleImage.Length > 0)
            {
                cmd.Parameters.Add("@ArticleImage", SqlDbType.VarBinary, -1).Value = this.ArticleImage;
            }
            else
            {
                cmd.Parameters.Add("@ArticleImage", SqlDbType.VarBinary, -1).Value = DBNull.Value;
            }

            cmd.Parameters.AddWithValue("@IsUnlimitedStock", this.IsUnlimitedStock);

            // New packaging parameters
            cmd.Parameters.AddWithValue("@PiecesPerPackage", this.PiecesPerPackage);
            cmd.Parameters.AddWithValue("@PackageType", this.PackageType ?? string.Empty);
            cmd.Parameters.AddWithValue("@PackageWeight", this.PackageWeight);
            cmd.Parameters.AddWithValue("@PackageDimensions", this.PackageDimensions ?? string.Empty);

            // New inventory management parameters
            cmd.Parameters.AddWithValue("@MinimumStock", this.MinimumStock);
            cmd.Parameters.AddWithValue("@MaximumStock", this.MaximumStock);
            cmd.Parameters.AddWithValue("@StorageLocation", this.StorageLocation ?? string.Empty);
            cmd.Parameters.AddWithValue("@SKU", this.SKU ?? string.Empty);
            cmd.Parameters.AddWithValue("@Description", this.Description ?? string.Empty);
            cmd.Parameters.AddWithValue("@IsPerishable", this.IsPerishable);
            cmd.Parameters.AddWithValue("@UnitOfMeasure", this.UnitOfMeasure ?? "piece");
            cmd.Parameters.AddWithValue("@PrixGros", this.PrixGros);
            cmd.Parameters.AddWithValue("@MinQuantityForGros", this.MinQuantityForGros);
            cmd.Parameters.AddWithValue("@CountryOfOrigin", this.CountryOfOrigin ?? string.Empty);
            cmd.Parameters.AddWithValue("@Manufacturer", this.Manufacturer ?? string.Empty);
            cmd.Parameters.AddWithValue("@LastRestockDate", this.LastRestockDate.HasValue ? (object)this.LastRestockDate.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@Notes", this.Notes ?? string.Empty);
        }

        // HELPER METHODS FOR UNLIMITED STOCK

        public string GetStockDisplayString()
        {
            if (IsUnlimitedStock)
            {
                return "∞";
            }
            return Quantite.ToString();
        }

        public bool HasSufficientStock(int requestedQuantity)
        {
            if (IsUnlimitedStock)
            {
                return true;
            }
            return Quantite >= requestedQuantity;
        }

        public void DecrementStock(int quantity)
        {
            if (!IsUnlimitedStock)
            {
                Quantite -= quantity;
            }
        }

        public void IncrementStock(int quantity)
        {
            if (!IsUnlimitedStock)
            {
                Quantite += quantity;
            }
        }

        public int GetEffectiveQuantity()
        {
            if (IsUnlimitedStock)
            {
                return 0;
            }
            return Quantite;
        }

        // NEW HELPER METHODS

        /// <summary>
        /// Checks if stock is below minimum level (reorder alert)
        /// </summary>
        public bool IsLowStock()
        {
            if (IsUnlimitedStock) return false;
            return Quantite <= MinimumStock && MinimumStock > 0;
        }

        /// <summary>
        /// Checks if stock is above maximum level (overstock alert)
        /// </summary>
        public bool IsOverstock()
        {
            if (IsUnlimitedStock) return false;
            return Quantite > MaximumStock && MaximumStock > 0;
        }

        /// <summary>
        /// Calculates total number of packages based on quantity and pieces per package
        /// </summary>
        public int GetTotalPackages()
        {
            if (PiecesPerPackage <= 0) return 0;
            return (int)Math.Ceiling((double)Quantite / PiecesPerPackage);
        }

        /// <summary>
        /// Gets the applicable price based on quantity (wholesale or retail)
        /// </summary>
        public decimal GetApplicablePrice(int quantity)
        {
            if (MinQuantityForGros > 0 && quantity >= MinQuantityForGros && PrixGros > 0)
            {
                return PrixGros;
            }
            return PrixVente;
        }

        /// <summary>
        /// Checks if article is expired or about to expire
        /// </summary>
        public bool IsExpired()
        {
            if (!DateExpiration.HasValue) return false;
            return DateExpiration.Value < DateTime.Now;
        }

        /// <summary>
        /// Checks if article will expire within specified days
        /// </summary>
        public bool IsExpiringWithin(int days)
        {
            if (!DateExpiration.HasValue) return false;
            return DateExpiration.Value <= DateTime.Now.AddDays(days) && !IsExpired();
        }
    }
}