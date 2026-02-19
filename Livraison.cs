using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;

namespace GestionComerce
{
    // ========================================
    // Classe Livraison
    // ========================================
    public class Livraison
    {
        public int LivraisonID { get; set; }
        public int OperationID { get; set; }
        public int? ClientID { get; set; }
        public string ClientNom { get; set; } = string.Empty;
        public string ClientTelephone { get; set; } = string.Empty;
        public string AdresseLivraison { get; set; } = string.Empty;
        public string Ville { get; set; } = string.Empty;
        public string CodePostal { get; set; } = string.Empty;
        public string ZoneLivraison { get; set; } = string.Empty;
        public decimal FraisLivraison { get; set; }
        public DateTime DateCommande { get; set; }
        public DateTime? DateLivraisonPrevue { get; set; }
        public DateTime? DateLivraisonEffective { get; set; }
        public int? LivreurID { get; set; }
        public string LivreurNom { get; set; } = string.Empty;
        public string Statut { get; set; } = "en_attente"; // en_attente, confirmee, en_preparation, en_cours, livree, annulee
        public string Notes { get; set; } = string.Empty;
        public decimal TotalCommande { get; set; }
        public string ModePaiement { get; set; } = string.Empty;
        public string PaiementStatut { get; set; } = "non_paye"; // non_paye, avance, paye
        public bool Etat { get; set; } = true;
        public DateTime DateCreation { get; set; }

        private static readonly string ConnectionString = "Server=localhost\\SQLEXPRESS;Database=GESTIONCOMERCEP;Trusted_Connection=True;";

        // GET ALL - Récupérer toutes les livraisons
        public async Task<List<Livraison>> GetLivraisonsAsync(string statutFilter = null, int? livreurIdFilter = null)
        {
            var livraisons = new List<Livraison>();
            string query = "SELECT * FROM Livraison WHERE Etat=1";

            if (!string.IsNullOrEmpty(statutFilter))
                query += " AND Statut=@Statut";
            if (livreurIdFilter.HasValue)
                query += " AND LivreurID=@LivreurID";

            query += " ORDER BY DateLivraisonPrevue DESC";

            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                using (var cmd = new SqlCommand(query, connection))
                {
                    if (!string.IsNullOrEmpty(statutFilter))
                        cmd.Parameters.AddWithValue("@Statut", statutFilter);
                    if (livreurIdFilter.HasValue)
                        cmd.Parameters.AddWithValue("@LivreurID", livreurIdFilter.Value);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            livraisons.Add(new Livraison
                            {
                                LivraisonID = reader["LivraisonID"] != DBNull.Value ? Convert.ToInt32(reader["LivraisonID"]) : 0,
                                OperationID = reader["OperationID"] != DBNull.Value ? Convert.ToInt32(reader["OperationID"]) : 0,
                                ClientID = reader["ClientID"] != DBNull.Value ? (int?)Convert.ToInt32(reader["ClientID"]) : null,
                                ClientNom = reader["ClientNom"] != DBNull.Value ? reader["ClientNom"].ToString() : string.Empty,
                                ClientTelephone = reader["ClientTelephone"] != DBNull.Value ? reader["ClientTelephone"].ToString() : string.Empty,
                                AdresseLivraison = reader["AdresseLivraison"] != DBNull.Value ? reader["AdresseLivraison"].ToString() : string.Empty,
                                Ville = reader["Ville"] != DBNull.Value ? reader["Ville"].ToString() : string.Empty,
                                CodePostal = reader["CodePostal"] != DBNull.Value ? reader["CodePostal"].ToString() : string.Empty,
                                ZoneLivraison = reader["ZoneLivraison"] != DBNull.Value ? reader["ZoneLivraison"].ToString() : string.Empty,
                                FraisLivraison = reader["FraisLivraison"] != DBNull.Value ? Convert.ToDecimal(reader["FraisLivraison"]) : 0m,
                                DateCommande = reader["DateCommande"] != DBNull.Value ? Convert.ToDateTime(reader["DateCommande"]) : DateTime.MinValue,
                                DateLivraisonPrevue = reader["DateLivraisonPrevue"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["DateLivraisonPrevue"]) : null,
                                DateLivraisonEffective = reader["DateLivraisonEffective"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["DateLivraisonEffective"]) : null,
                                LivreurID = reader["LivreurID"] != DBNull.Value ? (int?)Convert.ToInt32(reader["LivreurID"]) : null,
                                LivreurNom = reader["LivreurNom"] != DBNull.Value ? reader["LivreurNom"].ToString() : string.Empty,
                                Statut = reader["Statut"] != DBNull.Value ? reader["Statut"].ToString() : "en_attente",
                                Notes = reader["Notes"] != DBNull.Value ? reader["Notes"].ToString() : string.Empty,
                                TotalCommande = reader["TotalCommande"] != DBNull.Value ? Convert.ToDecimal(reader["TotalCommande"]) : 0m,
                                ModePaiement = reader["ModePaiement"] != DBNull.Value ? reader["ModePaiement"].ToString() : string.Empty,
                                PaiementStatut = reader["PaiementStatut"] != DBNull.Value ? reader["PaiementStatut"].ToString() : "non_paye",
                                Etat = reader["Etat"] != DBNull.Value ? Convert.ToBoolean(reader["Etat"]) : true,
                                DateCreation = reader["DateCreation"] != DBNull.Value ? Convert.ToDateTime(reader["DateCreation"]) : DateTime.MinValue
                            });
                        }
                    }
                }
            }

            return livraisons;
        }

        // GET ONE - Récupérer une livraison par ID
        public async Task<Livraison> GetLivraisonByIDAsync(int livraisonId)
        {
            string query = "SELECT * FROM Livraison WHERE LivraisonID=@LivraisonID AND Etat=1";

            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@LivraisonID", livraisonId);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Livraison
                            {
                                LivraisonID = reader["LivraisonID"] != DBNull.Value ? Convert.ToInt32(reader["LivraisonID"]) : 0,
                                OperationID = reader["OperationID"] != DBNull.Value ? Convert.ToInt32(reader["OperationID"]) : 0,
                                ClientID = reader["ClientID"] != DBNull.Value ? (int?)Convert.ToInt32(reader["ClientID"]) : null,
                                ClientNom = reader["ClientNom"] != DBNull.Value ? reader["ClientNom"].ToString() : string.Empty,
                                ClientTelephone = reader["ClientTelephone"] != DBNull.Value ? reader["ClientTelephone"].ToString() : string.Empty,
                                AdresseLivraison = reader["AdresseLivraison"] != DBNull.Value ? reader["AdresseLivraison"].ToString() : string.Empty,
                                Ville = reader["Ville"] != DBNull.Value ? reader["Ville"].ToString() : string.Empty,
                                CodePostal = reader["CodePostal"] != DBNull.Value ? reader["CodePostal"].ToString() : string.Empty,
                                ZoneLivraison = reader["ZoneLivraison"] != DBNull.Value ? reader["ZoneLivraison"].ToString() : string.Empty,
                                FraisLivraison = reader["FraisLivraison"] != DBNull.Value ? Convert.ToDecimal(reader["FraisLivraison"]) : 0m,
                                DateCommande = reader["DateCommande"] != DBNull.Value ? Convert.ToDateTime(reader["DateCommande"]) : DateTime.MinValue,
                                DateLivraisonPrevue = reader["DateLivraisonPrevue"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["DateLivraisonPrevue"]) : null,
                                DateLivraisonEffective = reader["DateLivraisonEffective"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["DateLivraisonEffective"]) : null,
                                LivreurID = reader["LivreurID"] != DBNull.Value ? (int?)Convert.ToInt32(reader["LivreurID"]) : null,
                                LivreurNom = reader["LivreurNom"] != DBNull.Value ? reader["LivreurNom"].ToString() : string.Empty,
                                Statut = reader["Statut"] != DBNull.Value ? reader["Statut"].ToString() : "en_attente",
                                Notes = reader["Notes"] != DBNull.Value ? reader["Notes"].ToString() : string.Empty,
                                TotalCommande = reader["TotalCommande"] != DBNull.Value ? Convert.ToDecimal(reader["TotalCommande"]) : 0m,
                                ModePaiement = reader["ModePaiement"] != DBNull.Value ? reader["ModePaiement"].ToString() : string.Empty,
                                PaiementStatut = reader["PaiementStatut"] != DBNull.Value ? reader["PaiementStatut"].ToString() : "non_paye",
                                Etat = reader["Etat"] != DBNull.Value ? Convert.ToBoolean(reader["Etat"]) : true,
                                DateCreation = reader["DateCreation"] != DBNull.Value ? Convert.ToDateTime(reader["DateCreation"]) : DateTime.MinValue
                            };
                        }
                    }
                }
            }

            return null;
        }

        // INSERT - Créer une nouvelle livraison
        public async Task<int> InsertLivraisonAsync()
        {
            string query = @"INSERT INTO Livraison 
                            (OperationID, ClientID, ClientNom, ClientTelephone, AdresseLivraison, Ville, CodePostal, 
                             ZoneLivraison, FraisLivraison, DateLivraisonPrevue, LivreurID, LivreurNom, Statut, 
                             Notes, TotalCommande, ModePaiement, PaiementStatut) 
                            VALUES 
                            (@OperationID, @ClientID, @ClientNom, @ClientTelephone, @AdresseLivraison, @Ville, @CodePostal, 
                             @ZoneLivraison, @FraisLivraison, @DateLivraisonPrevue, @LivreurID, @LivreurNom, @Statut, 
                             @Notes, @TotalCommande, @ModePaiement, @PaiementStatut); 
                            SELECT SCOPE_IDENTITY();";

            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                try
                {
                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@OperationID", OperationID);
                        cmd.Parameters.AddWithValue("@ClientID", (object)ClientID ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ClientNom", string.IsNullOrEmpty(ClientNom) ? (object)DBNull.Value : ClientNom);
                        cmd.Parameters.AddWithValue("@ClientTelephone", string.IsNullOrEmpty(ClientTelephone) ? (object)DBNull.Value : ClientTelephone);
                        cmd.Parameters.AddWithValue("@AdresseLivraison", string.IsNullOrEmpty(AdresseLivraison) ? (object)DBNull.Value : AdresseLivraison);
                        cmd.Parameters.AddWithValue("@Ville", string.IsNullOrEmpty(Ville) ? (object)DBNull.Value : Ville);
                        cmd.Parameters.AddWithValue("@CodePostal", string.IsNullOrEmpty(CodePostal) ? (object)DBNull.Value : CodePostal);
                        cmd.Parameters.AddWithValue("@ZoneLivraison", string.IsNullOrEmpty(ZoneLivraison) ? (object)DBNull.Value : ZoneLivraison);
                        cmd.Parameters.AddWithValue("@FraisLivraison", FraisLivraison);
                        cmd.Parameters.AddWithValue("@DateLivraisonPrevue", (object)DateLivraisonPrevue ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@LivreurID", (object)LivreurID ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@LivreurNom", string.IsNullOrEmpty(LivreurNom) ? (object)DBNull.Value : LivreurNom);
                        cmd.Parameters.AddWithValue("@Statut", string.IsNullOrEmpty(Statut) ? "en_attente" : Statut);
                        cmd.Parameters.AddWithValue("@Notes", string.IsNullOrEmpty(Notes) ? (object)DBNull.Value : Notes);
                        cmd.Parameters.AddWithValue("@TotalCommande", TotalCommande);
                        cmd.Parameters.AddWithValue("@ModePaiement", string.IsNullOrEmpty(ModePaiement) ? (object)DBNull.Value : ModePaiement);
                        cmd.Parameters.AddWithValue("@PaiementStatut", string.IsNullOrEmpty(PaiementStatut) ? "non_paye" : PaiementStatut);

                        object result = await cmd.ExecuteScalarAsync();
                        int newId = Convert.ToInt32(result);

                        // Ajouter à l'historique
                        await AddHistoriqueAsync(newId, null, Statut, "Livraison créée", connection);

                        return newId;
                    }
                }
                catch (Exception err)
                {
                    MessageBox.Show($"Livraison non insérée, erreur: {err.Message}");
                    return 0;
                }
            }
        }

        // UPDATE - Mettre à jour une livraison
        public async Task<int> UpdateLivraisonAsync()
        {
            string query = @"UPDATE Livraison 
                            SET ClientNom=@ClientNom, ClientTelephone=@ClientTelephone, AdresseLivraison=@AdresseLivraison,
                                Ville=@Ville, CodePostal=@CodePostal, ZoneLivraison=@ZoneLivraison, 
                                FraisLivraison=@FraisLivraison, DateLivraisonPrevue=@DateLivraisonPrevue,
                                LivreurID=@LivreurID, LivreurNom=@LivreurNom, Statut=@Statut, 
                                Notes=@Notes, ModePaiement=@ModePaiement, PaiementStatut=@PaiementStatut
                            WHERE LivraisonID=@LivraisonID";

            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                using (var cmd = new SqlCommand(query, connection))
                {
                    try
                    {
                        cmd.Parameters.AddWithValue("@ClientNom", string.IsNullOrEmpty(ClientNom) ? (object)DBNull.Value : ClientNom);
                        cmd.Parameters.AddWithValue("@ClientTelephone", string.IsNullOrEmpty(ClientTelephone) ? (object)DBNull.Value : ClientTelephone);
                        cmd.Parameters.AddWithValue("@AdresseLivraison", string.IsNullOrEmpty(AdresseLivraison) ? (object)DBNull.Value : AdresseLivraison);
                        cmd.Parameters.AddWithValue("@Ville", string.IsNullOrEmpty(Ville) ? (object)DBNull.Value : Ville);
                        cmd.Parameters.AddWithValue("@CodePostal", string.IsNullOrEmpty(CodePostal) ? (object)DBNull.Value : CodePostal);
                        cmd.Parameters.AddWithValue("@ZoneLivraison", string.IsNullOrEmpty(ZoneLivraison) ? (object)DBNull.Value : ZoneLivraison);
                        cmd.Parameters.AddWithValue("@FraisLivraison", FraisLivraison);
                        cmd.Parameters.AddWithValue("@DateLivraisonPrevue", (object)DateLivraisonPrevue ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@LivreurID", (object)LivreurID ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@LivreurNom", string.IsNullOrEmpty(LivreurNom) ? (object)DBNull.Value : LivreurNom);
                        cmd.Parameters.AddWithValue("@Statut", Statut);
                        cmd.Parameters.AddWithValue("@Notes", string.IsNullOrEmpty(Notes) ? (object)DBNull.Value : Notes);
                        cmd.Parameters.AddWithValue("@ModePaiement", string.IsNullOrEmpty(ModePaiement) ? (object)DBNull.Value : ModePaiement);
                        cmd.Parameters.AddWithValue("@PaiementStatut", PaiementStatut);
                        cmd.Parameters.AddWithValue("@LivraisonID", LivraisonID);

                        await cmd.ExecuteNonQueryAsync();
                        return 1;
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show($"Livraison non mise à jour: {err.Message}");
                        return 0;
                    }
                }
            }
        }

        // UPDATE STATUS - Mettre à jour le statut
        public async Task<int> UpdateStatutAsync(string nouveauStatut, string commentaire = "")
        {
            string selectQuery = "SELECT Statut FROM Livraison WHERE LivraisonID=@LivraisonID";
            string updateQuery = "UPDATE Livraison SET Statut=@Statut";

            if (nouveauStatut == "livree")
                updateQuery += ", DateLivraisonEffective=GETDATE()";

            updateQuery += " WHERE LivraisonID=@LivraisonID";

            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                try
                {
                    // Récupérer l'ancien statut
                    string ancienStatut = null;
                    using (var selectCmd = new SqlCommand(selectQuery, connection))
                    {
                        selectCmd.Parameters.AddWithValue("@LivraisonID", LivraisonID);
                        var result = await selectCmd.ExecuteScalarAsync();
                        ancienStatut = result?.ToString();
                    }

                    // Mettre à jour le statut
                    using (var updateCmd = new SqlCommand(updateQuery, connection))
                    {
                        updateCmd.Parameters.AddWithValue("@Statut", nouveauStatut);
                        updateCmd.Parameters.AddWithValue("@LivraisonID", LivraisonID);
                        await updateCmd.ExecuteNonQueryAsync();
                    }

                    // Ajouter à l'historique
                    await AddHistoriqueAsync(LivraisonID, ancienStatut, nouveauStatut, commentaire, connection);

                    return 1;
                }
                catch (Exception err)
                {
                    MessageBox.Show($"Statut non mis à jour: {err.Message}");
                    return 0;
                }
            }
        }

        // DELETE - Supprimer (soft delete)
        public async Task<int> DeleteLivraisonAsync()
        {
            string query = "UPDATE Livraison SET Etat=0 WHERE LivraisonID=@LivraisonID";

            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                using (var cmd = new SqlCommand(query, connection))
                {
                    try
                    {
                        cmd.Parameters.AddWithValue("@LivraisonID", LivraisonID);
                        await cmd.ExecuteNonQueryAsync();
                        return 1;
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show($"Livraison non supprimée: {err.Message}");
                        return 0;
                    }
                }
            }
        }

        // GET STATISTICS - Obtenir les statistiques
        public async Task<LivraisonStatistiques> GetStatistiquesAsync(DateTime? dateDebut = null, DateTime? dateFin = null)
        {
            var stats = new LivraisonStatistiques();
            string query = @"SELECT 
                            COUNT(*) as TotalLivraisons,
                            SUM(CASE WHEN Statut = 'livree' THEN 1 ELSE 0 END) as Livrees,
                            SUM(CASE WHEN Statut = 'en_cours' THEN 1 ELSE 0 END) as EnCours,
                            SUM(CASE WHEN Statut = 'en_attente' THEN 1 ELSE 0 END) as EnAttente,
                            SUM(CASE WHEN Statut = 'annulee' THEN 1 ELSE 0 END) as Annulees,
                            SUM(TotalCommande) as TotalVentes,
                            SUM(FraisLivraison) as TotalFraisLivraison,
                            AVG(FraisLivraison) as FraisMoyen
                            FROM Livraison WHERE Etat=1";

            if (dateDebut.HasValue)
                query += " AND DateCommande >= @DateDebut";
            if (dateFin.HasValue)
                query += " AND DateCommande <= @DateFin";

            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                using (var cmd = new SqlCommand(query, connection))
                {
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
                            stats.EnAttente = reader["EnAttente"] != DBNull.Value ? Convert.ToInt32(reader["EnAttente"]) : 0;
                            stats.Annulees = reader["Annulees"] != DBNull.Value ? Convert.ToInt32(reader["Annulees"]) : 0;
                            stats.TotalVentes = reader["TotalVentes"] != DBNull.Value ? Convert.ToDecimal(reader["TotalVentes"]) : 0m;
                            stats.TotalFraisLivraison = reader["TotalFraisLivraison"] != DBNull.Value ? Convert.ToDecimal(reader["TotalFraisLivraison"]) : 0m;
                            stats.FraisMoyen = reader["FraisMoyen"] != DBNull.Value ? Convert.ToDecimal(reader["FraisMoyen"]) : 0m;
                        }
                    }
                }
            }

            return stats;
        }

        // Ajouter à l'historique
        private async Task AddHistoriqueAsync(int livraisonId, string ancienStatut, string nouveauStatut, string commentaire, SqlConnection connection)
        {
            try
            {
                string query = @"INSERT INTO LivraisonHistorique 
                                (LivraisonID, AncienStatut, NouveauStatut, Commentaire)
                                VALUES (@LivraisonID, @AncienStatut, @NouveauStatut, @Commentaire)";

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@LivraisonID", livraisonId);
                    cmd.Parameters.AddWithValue("@AncienStatut", (object)ancienStatut ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@NouveauStatut", nouveauStatut);
                    cmd.Parameters.AddWithValue("@Commentaire", string.IsNullOrEmpty(commentaire) ? (object)DBNull.Value : commentaire);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception err)
            {
                Console.WriteLine($"Historique non ajouté: {err.Message}");
            }
        }

        // GET HISTORIQUE - Obtenir l'historique d'une livraison
        public async Task<List<LivraisonHistorique>> GetHistoriqueAsync()
        {
            var historique = new List<LivraisonHistorique>();
            string query = "SELECT * FROM LivraisonHistorique WHERE LivraisonID=@LivraisonID ORDER BY DateChangement DESC";

            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@LivraisonID", LivraisonID);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            historique.Add(new LivraisonHistorique
                            {
                                HistoriqueID = reader["HistoriqueID"] != DBNull.Value ? Convert.ToInt32(reader["HistoriqueID"]) : 0,
                                LivraisonID = reader["LivraisonID"] != DBNull.Value ? Convert.ToInt32(reader["LivraisonID"]) : 0,
                                AncienStatut = reader["AncienStatut"] != DBNull.Value ? reader["AncienStatut"].ToString() : string.Empty,
                                NouveauStatut = reader["NouveauStatut"] != DBNull.Value ? reader["NouveauStatut"].ToString() : string.Empty,
                                Commentaire = reader["Commentaire"] != DBNull.Value ? reader["Commentaire"].ToString() : string.Empty,
                                DateChangement = reader["DateChangement"] != DBNull.Value ? Convert.ToDateTime(reader["DateChangement"]) : DateTime.MinValue
                            });
                        }
                    }
                }
            }

            return historique;
        }
    }

    // ========================================
    // Classe LivraisonHistorique
    // ========================================
    public class LivraisonHistorique
    {
        public int HistoriqueID { get; set; }
        public int LivraisonID { get; set; }
        public string AncienStatut { get; set; } = string.Empty;
        public string NouveauStatut { get; set; } = string.Empty;
        public string Commentaire { get; set; } = string.Empty;
        public DateTime DateChangement { get; set; }
    }

    // ========================================
    // Classe LivraisonStatistiques
    // ========================================
    public class LivraisonStatistiques
    {
        public int TotalLivraisons { get; set; }
        public int Livrees { get; set; }
        public int EnCours { get; set; }
        public int EnAttente { get; set; }
        public int Annulees { get; set; }
        public decimal TotalVentes { get; set; }
        public decimal TotalFraisLivraison { get; set; }
        public decimal FraisMoyen { get; set; }
    }
}