using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;

namespace GestionComerce
{
    // ========================================
    // Classe Livreur
    // ========================================
    public class Livreur
    {
        public int LivreurID { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string Telephone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string VehiculeType { get; set; } = string.Empty;
        public string VehiculeImmatriculation { get; set; } = string.Empty;
        public string Statut { get; set; } = "disponible"; // disponible, occupe, indisponible
        public string ZoneCouverture { get; set; } = string.Empty;
        public string Photo { get; set; } = string.Empty;
        public DateTime? DateEmbauche { get; set; }
        public bool Actif { get; set; } = true;
        public DateTime DateCreation { get; set; }

        // Propriété calculée pour le nom complet
        public string NomComplet => $"{Prenom} {Nom}";

        private static readonly string ConnectionString = "Server=localhost\\SQLEXPRESS;Database=GESTIONCOMERCEP;Trusted_Connection=True;";

        // GET ALL - Récupérer tous les livreurs
        public async Task<List<Livreur>> GetLivreursAsync(bool actifSeulement = true)
        {
            var livreurs = new List<Livreur>();
            string query = "SELECT * FROM Livreur";

            if (actifSeulement)
                query += " WHERE Actif=1";

            query += " ORDER BY Nom, Prenom";

            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                using (var cmd = new SqlCommand(query, connection))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        livreurs.Add(new Livreur
                        {
                            LivreurID = reader["LivreurID"] != DBNull.Value ? Convert.ToInt32(reader["LivreurID"]) : 0,
                            Nom = reader["Nom"] != DBNull.Value ? reader["Nom"].ToString() : string.Empty,
                            Prenom = reader["Prenom"] != DBNull.Value ? reader["Prenom"].ToString() : string.Empty,
                            Telephone = reader["Telephone"] != DBNull.Value ? reader["Telephone"].ToString() : string.Empty,
                            Email = reader["Email"] != DBNull.Value ? reader["Email"].ToString() : string.Empty,
                            VehiculeType = reader["VehiculeType"] != DBNull.Value ? reader["VehiculeType"].ToString() : string.Empty,
                            VehiculeImmatriculation = reader["VehiculeImmatriculation"] != DBNull.Value ? reader["VehiculeImmatriculation"].ToString() : string.Empty,
                            Statut = reader["Statut"] != DBNull.Value ? reader["Statut"].ToString() : "disponible",
                            ZoneCouverture = reader["ZoneCouverture"] != DBNull.Value ? reader["ZoneCouverture"].ToString() : string.Empty,
                            Photo = reader["Photo"] != DBNull.Value ? reader["Photo"].ToString() : string.Empty,
                            DateEmbauche = reader["DateEmbauche"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["DateEmbauche"]) : null,
                            Actif = reader["Actif"] != DBNull.Value ? Convert.ToBoolean(reader["Actif"]) : true,
                            DateCreation = reader["DateCreation"] != DBNull.Value ? Convert.ToDateTime(reader["DateCreation"]) : DateTime.MinValue
                        });
                    }
                }
            }

            return livreurs;
        }

        // GET ONE - Récupérer un livreur par ID
        public async Task<Livreur> GetLivreurByIDAsync(int livreurId)
        {
            string query = "SELECT * FROM Livreur WHERE LivreurID=@LivreurID";

            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@LivreurID", livreurId);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Livreur
                            {
                                LivreurID = reader["LivreurID"] != DBNull.Value ? Convert.ToInt32(reader["LivreurID"]) : 0,
                                Nom = reader["Nom"] != DBNull.Value ? reader["Nom"].ToString() : string.Empty,
                                Prenom = reader["Prenom"] != DBNull.Value ? reader["Prenom"].ToString() : string.Empty,
                                Telephone = reader["Telephone"] != DBNull.Value ? reader["Telephone"].ToString() : string.Empty,
                                Email = reader["Email"] != DBNull.Value ? reader["Email"].ToString() : string.Empty,
                                VehiculeType = reader["VehiculeType"] != DBNull.Value ? reader["VehiculeType"].ToString() : string.Empty,
                                VehiculeImmatriculation = reader["VehiculeImmatriculation"] != DBNull.Value ? reader["VehiculeImmatriculation"].ToString() : string.Empty,
                                Statut = reader["Statut"] != DBNull.Value ? reader["Statut"].ToString() : "disponible",
                                ZoneCouverture = reader["ZoneCouverture"] != DBNull.Value ? reader["ZoneCouverture"].ToString() : string.Empty,
                                Photo = reader["Photo"] != DBNull.Value ? reader["Photo"].ToString() : string.Empty,
                                DateEmbauche = reader["DateEmbauche"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["DateEmbauche"]) : null,
                                Actif = reader["Actif"] != DBNull.Value ? Convert.ToBoolean(reader["Actif"]) : true,
                                DateCreation = reader["DateCreation"] != DBNull.Value ? Convert.ToDateTime(reader["DateCreation"]) : DateTime.MinValue
                            };
                        }
                    }
                }
            }

            return null;
        }

        // GET DISPONIBLES - Récupérer les livreurs disponibles
        public async Task<List<Livreur>> GetLivreursDisponiblesAsync()
        {
            var livreurs = new List<Livreur>();
            string query = "SELECT * FROM Livreur WHERE Statut='disponible' AND Actif=1 ORDER BY Nom, Prenom";

            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                using (var cmd = new SqlCommand(query, connection))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        livreurs.Add(new Livreur
                        {
                            LivreurID = reader["LivreurID"] != DBNull.Value ? Convert.ToInt32(reader["LivreurID"]) : 0,
                            Nom = reader["Nom"] != DBNull.Value ? reader["Nom"].ToString() : string.Empty,
                            Prenom = reader["Prenom"] != DBNull.Value ? reader["Prenom"].ToString() : string.Empty,
                            Telephone = reader["Telephone"] != DBNull.Value ? reader["Telephone"].ToString() : string.Empty,
                            Email = reader["Email"] != DBNull.Value ? reader["Email"].ToString() : string.Empty,
                            VehiculeType = reader["VehiculeType"] != DBNull.Value ? reader["VehiculeType"].ToString() : string.Empty,
                            VehiculeImmatriculation = reader["VehiculeImmatriculation"] != DBNull.Value ? reader["VehiculeImmatriculation"].ToString() : string.Empty,
                            Statut = reader["Statut"] != DBNull.Value ? reader["Statut"].ToString() : "disponible",
                            ZoneCouverture = reader["ZoneCouverture"] != DBNull.Value ? reader["ZoneCouverture"].ToString() : string.Empty,
                            Photo = reader["Photo"] != DBNull.Value ? reader["Photo"].ToString() : string.Empty,
                            DateEmbauche = reader["DateEmbauche"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["DateEmbauche"]) : null,
                            Actif = reader["Actif"] != DBNull.Value ? Convert.ToBoolean(reader["Actif"]) : true,
                            DateCreation = reader["DateCreation"] != DBNull.Value ? Convert.ToDateTime(reader["DateCreation"]) : DateTime.MinValue
                        });
                    }
                }
            }

            return livreurs;
        }

        // INSERT - Créer un nouveau livreur
        public async Task<int> InsertLivreurAsync()
        {
            string query = @"INSERT INTO Livreur 
                            (Nom, Prenom, Telephone, Email, VehiculeType, VehiculeImmatriculation, 
                             Statut, ZoneCouverture, DateEmbauche, Actif)
                            VALUES 
                            (@Nom, @Prenom, @Telephone, @Email, @VehiculeType, @VehiculeImmatriculation, 
                             @Statut, @ZoneCouverture, @DateEmbauche, @Actif);
                            SELECT SCOPE_IDENTITY();";

            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                try
                {
                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Nom", string.IsNullOrEmpty(Nom) ? (object)DBNull.Value : Nom);
                        cmd.Parameters.AddWithValue("@Prenom", string.IsNullOrEmpty(Prenom) ? (object)DBNull.Value : Prenom);
                        cmd.Parameters.AddWithValue("@Telephone", string.IsNullOrEmpty(Telephone) ? (object)DBNull.Value : Telephone);
                        cmd.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(Email) ? (object)DBNull.Value : Email);
                        cmd.Parameters.AddWithValue("@VehiculeType", string.IsNullOrEmpty(VehiculeType) ? (object)DBNull.Value : VehiculeType);
                        cmd.Parameters.AddWithValue("@VehiculeImmatriculation", string.IsNullOrEmpty(VehiculeImmatriculation) ? (object)DBNull.Value : VehiculeImmatriculation);
                        cmd.Parameters.AddWithValue("@Statut", string.IsNullOrEmpty(Statut) ? "disponible" : Statut);
                        cmd.Parameters.AddWithValue("@ZoneCouverture", string.IsNullOrEmpty(ZoneCouverture) ? (object)DBNull.Value : ZoneCouverture);
                        cmd.Parameters.AddWithValue("@DateEmbauche", (object)DateEmbauche ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Actif", Actif);

                        object result = await cmd.ExecuteScalarAsync();
                        return Convert.ToInt32(result);
                    }
                }
                catch (Exception err)
                {
                    MessageBox.Show($"Livreur non inséré, erreur: {err.Message}");
                    return 0;
                }
            }
        }

        // UPDATE - Mettre à jour un livreur
        public async Task<int> UpdateLivreurAsync()
        {
            string query = @"UPDATE Livreur 
                            SET Nom=@Nom, Prenom=@Prenom, Telephone=@Telephone, Email=@Email,
                                VehiculeType=@VehiculeType, VehiculeImmatriculation=@VehiculeImmatriculation,
                                Statut=@Statut, ZoneCouverture=@ZoneCouverture, DateEmbauche=@DateEmbauche, Actif=@Actif
                            WHERE LivreurID=@LivreurID";

            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                using (var cmd = new SqlCommand(query, connection))
                {
                    try
                    {
                        cmd.Parameters.AddWithValue("@Nom", string.IsNullOrEmpty(Nom) ? (object)DBNull.Value : Nom);
                        cmd.Parameters.AddWithValue("@Prenom", string.IsNullOrEmpty(Prenom) ? (object)DBNull.Value : Prenom);
                        cmd.Parameters.AddWithValue("@Telephone", string.IsNullOrEmpty(Telephone) ? (object)DBNull.Value : Telephone);
                        cmd.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(Email) ? (object)DBNull.Value : Email);
                        cmd.Parameters.AddWithValue("@VehiculeType", string.IsNullOrEmpty(VehiculeType) ? (object)DBNull.Value : VehiculeType);
                        cmd.Parameters.AddWithValue("@VehiculeImmatriculation", string.IsNullOrEmpty(VehiculeImmatriculation) ? (object)DBNull.Value : VehiculeImmatriculation);
                        cmd.Parameters.AddWithValue("@Statut", Statut);
                        cmd.Parameters.AddWithValue("@ZoneCouverture", string.IsNullOrEmpty(ZoneCouverture) ? (object)DBNull.Value : ZoneCouverture);
                        cmd.Parameters.AddWithValue("@DateEmbauche", (object)DateEmbauche ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Actif", Actif);
                        cmd.Parameters.AddWithValue("@LivreurID", LivreurID);

                        await cmd.ExecuteNonQueryAsync();
                        return 1;
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show($"Livreur non mis à jour: {err.Message}");
                        return 0;
                    }
                }
            }
        }

        // UPDATE STATUS - Mettre à jour le statut du livreur
        public async Task<int> UpdateStatutAsync(string nouveauStatut)
        {
            string query = "UPDATE Livreur SET Statut=@Statut WHERE LivreurID=@LivreurID";

            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                using (var cmd = new SqlCommand(query, connection))
                {
                    try
                    {
                        cmd.Parameters.AddWithValue("@Statut", nouveauStatut);
                        cmd.Parameters.AddWithValue("@LivreurID", LivreurID);
                        await cmd.ExecuteNonQueryAsync();
                        return 1;
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show($"Statut non mis à jour: {err.Message}");
                        return 0;
                    }
                }
            }
        }

        // DELETE - Désactiver un livreur (soft delete)
        public async Task<int> DeleteLivreurAsync()
        {
            string query = "UPDATE Livreur SET Actif=0 WHERE LivreurID=@LivreurID";

            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                using (var cmd = new SqlCommand(query, connection))
                {
                    try
                    {
                        cmd.Parameters.AddWithValue("@LivreurID", LivreurID);
                        await cmd.ExecuteNonQueryAsync();
                        return 1;
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show($"Livreur non supprimé: {err.Message}");
                        return 0;
                    }
                }
            }
        }

        // GET STATISTICS - Obtenir les statistiques d'un livreur
        public async Task<LivreurStatistiques> GetStatistiquesAsync(DateTime? dateDebut = null, DateTime? dateFin = null)
        {
            var stats = new LivreurStatistiques();
            string query = @"SELECT 
                            COUNT(*) as TotalLivraisons,
                            SUM(CASE WHEN Statut = 'livree' THEN 1 ELSE 0 END) as Livrees,
                            SUM(CASE WHEN Statut = 'en_cours' THEN 1 ELSE 0 END) as EnCours,
                            SUM(CASE WHEN Statut = 'annulee' THEN 1 ELSE 0 END) as Annulees,
                            SUM(TotalCommande) as TotalVentes,
                            SUM(FraisLivraison) as TotalFrais
                            FROM Livraison 
                            WHERE LivreurID=@LivreurID AND Etat=1";

            if (dateDebut.HasValue)
                query += " AND DateCommande >= @DateDebut";
            if (dateFin.HasValue)
                query += " AND DateCommande <= @DateFin";

            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@LivreurID", LivreurID);

                    if (dateDebut.HasValue)
                        cmd.Parameters.AddWithValue("@DateDebut", dateDebut.Value);
                    if (dateFin.HasValue)
                        cmd.Parameters.AddWithValue("@DateFin", dateFin.Value);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            stats.TotalLivraisons = reader["TotalLivraisons"] != DBNull.Value ? Convert.ToInt32(reader["TotalLivraisons"]) : 0;
                            stats.Livrees = reader["Livrees"] != DBNull.Value ? Convert.ToInt32(reader["Livrees"]) : 0;
                            stats.EnCours = reader["EnCours"] != DBNull.Value ? Convert.ToInt32(reader["EnCours"]) : 0;
                            stats.Annulees = reader["Annulees"] != DBNull.Value ? Convert.ToInt32(reader["Annulees"]) : 0;
                            stats.TotalVentes = reader["TotalVentes"] != DBNull.Value ? Convert.ToDecimal(reader["TotalVentes"]) : 0m;
                            stats.TotalFrais = reader["TotalFrais"] != DBNull.Value ? Convert.ToDecimal(reader["TotalFrais"]) : 0m;
                        }
                    }
                }
            }

            return stats;
        }

        // GET LIVRAISONS - Obtenir les livraisons d'un livreur
        public async Task<List<Livraison>> GetLivraisonsAsync(string statutFilter = null)
        {
            var livraisons = new List<Livraison>();
            string query = "SELECT * FROM Livraison WHERE LivreurID=@LivreurID AND Etat=1";

            if (!string.IsNullOrEmpty(statutFilter))
                query += " AND Statut=@Statut";

            query += " ORDER BY DateLivraisonPrevue DESC";

            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@LivreurID", LivreurID);

                    if (!string.IsNullOrEmpty(statutFilter))
                        cmd.Parameters.AddWithValue("@Statut", statutFilter);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            livraisons.Add(new Livraison
                            {
                                LivraisonID = reader["LivraisonID"] != DBNull.Value ? Convert.ToInt32(reader["LivraisonID"]) : 0,
                                OperationID = reader["OperationID"] != DBNull.Value ? Convert.ToInt32(reader["OperationID"]) : 0,
                                ClientNom = reader["ClientNom"] != DBNull.Value ? reader["ClientNom"].ToString() : string.Empty,
                                ClientTelephone = reader["ClientTelephone"] != DBNull.Value ? reader["ClientTelephone"].ToString() : string.Empty,
                                AdresseLivraison = reader["AdresseLivraison"] != DBNull.Value ? reader["AdresseLivraison"].ToString() : string.Empty,
                                Statut = reader["Statut"] != DBNull.Value ? reader["Statut"].ToString() : "en_attente",
                                DateLivraisonPrevue = reader["DateLivraisonPrevue"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["DateLivraisonPrevue"]) : null,
                                TotalCommande = reader["TotalCommande"] != DBNull.Value ? Convert.ToDecimal(reader["TotalCommande"]) : 0m,
                                FraisLivraison = reader["FraisLivraison"] != DBNull.Value ? Convert.ToDecimal(reader["FraisLivraison"]) : 0m
                            });
                        }
                    }
                }
            }

            return livraisons;
        }
    }

    // ========================================
    // Classe LivreurStatistiques
    // ========================================
    public class LivreurStatistiques
    {
        public int TotalLivraisons { get; set; }
        public int Livrees { get; set; }
        public int EnCours { get; set; }
        public int Annulees { get; set; }
        public decimal TotalVentes { get; set; }
        public decimal TotalFrais { get; set; }
    }
}