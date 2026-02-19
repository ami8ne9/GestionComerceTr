using System;
using System.ComponentModel;
using System.Data.SqlClient;

namespace Superete
{
    // Classe combinée : Modèle + DAL
    public class ParametresGeneraux : INotifyPropertyChanged
    {
        // Propriétés du modèle
        private int _id;
        private int _userId;
        private string _afficherClavier;
        private bool _masquerEtiquettesVides;
        private bool _supprimerArticlesQuantiteZero;
        private string _langue;
        private bool _imprimerFactureParDefaut;
        private bool _imprimerTicketParDefaut;
        private string _methodePaiementParDefaut;
        private string _vueParDefaut;
        private string _trierParDefaut;
        private string _tailleIcones;
        private DateTime _dateCreation;
        private DateTime _dateModification;

        public int Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(Id)); }
        }

        public int UserId
        {
            get => _userId;
            set { _userId = value; OnPropertyChanged(nameof(UserId)); }
        }

        public string AfficherClavier
        {
            get => _afficherClavier;
            set { _afficherClavier = value; OnPropertyChanged(nameof(AfficherClavier)); }
        }

        public bool MasquerEtiquettesVides
        {
            get => _masquerEtiquettesVides;
            set { _masquerEtiquettesVides = value; OnPropertyChanged(nameof(MasquerEtiquettesVides)); }
        }

        public bool SupprimerArticlesQuantiteZero
        {
            get => _supprimerArticlesQuantiteZero;
            set { _supprimerArticlesQuantiteZero = value; OnPropertyChanged(nameof(SupprimerArticlesQuantiteZero)); }
        }

        public string Langue
        {
            get => _langue;
            set { _langue = value; OnPropertyChanged(nameof(Langue)); }
        }

        public bool ImprimerFactureParDefaut
        {
            get => _imprimerFactureParDefaut;
            set { _imprimerFactureParDefaut = value; OnPropertyChanged(nameof(ImprimerFactureParDefaut)); }
        }

        public bool ImprimerTicketParDefaut
        {
            get => _imprimerTicketParDefaut;
            set { _imprimerTicketParDefaut = value; OnPropertyChanged(nameof(ImprimerTicketParDefaut)); }
        }

        public string MethodePaiementParDefaut
        {
            get => _methodePaiementParDefaut;
            set { _methodePaiementParDefaut = value; OnPropertyChanged(nameof(MethodePaiementParDefaut)); }
        }

        public string VueParDefaut
        {
            get => _vueParDefaut;
            set { _vueParDefaut = value; OnPropertyChanged(nameof(VueParDefaut)); }
        }

        public string TrierParDefaut
        {
            get => _trierParDefaut;
            set { _trierParDefaut = value; OnPropertyChanged(nameof(TrierParDefaut)); }
        }

        public string TailleIcones
        {
            get => _tailleIcones;
            set { _tailleIcones = value; OnPropertyChanged(nameof(TailleIcones)); }
        }

        public DateTime DateCreation
        {
            get => _dateCreation;
            set { _dateCreation = value; OnPropertyChanged(nameof(DateCreation)); }
        }

        public DateTime DateModification
        {
            get => _dateModification;
            set { _dateModification = value; OnPropertyChanged(nameof(DateModification)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Méthodes statiques pour l'accès à la base de données
        private static string _connectionString;

        public static void SetConnectionString(string connectionString)
        {
            _connectionString = "Server=localhost\\SQLEXPRESS;Database=GESTIONCOMERCEP;Trusted_Connection=True;";
        }

        /// <summary>
        /// Récupère les paramètres généraux d'un utilisateur
        /// </summary>
        public static ParametresGeneraux ObtenirParametresParUserId(int userId, string connectionString)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM ParametresGeneraux WHERE UserId = @UserId";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new ParametresGeneraux
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                                    AfficherClavier = reader.GetString(reader.GetOrdinal("AfficherClavier")),
                                    MasquerEtiquettesVides = reader.GetBoolean(reader.GetOrdinal("MasquerEtiquettesVides")),
                                    SupprimerArticlesQuantiteZero = reader.GetBoolean(reader.GetOrdinal("SupprimerArticlesQuantiteZero")),
                                    Langue = reader.GetString(reader.GetOrdinal("Langue")),
                                    ImprimerFactureParDefaut = reader.GetBoolean(reader.GetOrdinal("ImprimerFactureParDefaut")),
                                    ImprimerTicketParDefaut = reader.GetBoolean(reader.GetOrdinal("ImprimerTicketParDefaut")),
                                    MethodePaiementParDefaut = reader.GetString(reader.GetOrdinal("MethodePaiementParDefaut")),
                                    VueParDefaut = reader.GetString(reader.GetOrdinal("VueParDefaut")),
                                    TrierParDefaut = reader.GetString(reader.GetOrdinal("TrierParDefaut")),
                                    TailleIcones = reader.GetString(reader.GetOrdinal("TailleIcones")),
                                    DateCreation = reader.GetDateTime(reader.GetOrdinal("DateCreation")),
                                    DateModification = reader.GetDateTime(reader.GetOrdinal("DateModification"))
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de la récupération des paramètres : {ex.Message}", ex);
            }

            return null;
        }

        /// <summary>
        /// Crée des paramètres par défaut pour un utilisateur
        /// </summary>
        public static int CreerParametresParDefaut(int userId, string connectionString)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"INSERT INTO ParametresGeneraux 
                        (UserId, AfficherClavier, MasquerEtiquettesVides, SupprimerArticlesQuantiteZero, 
                         Langue, ImprimerFactureParDefaut, ImprimerTicketParDefaut, MethodePaiementParDefaut,
                         VueParDefaut, TrierParDefaut, TailleIcones, DateCreation, DateModification)
                        VALUES (@UserId, @AfficherClavier, @MasquerEtiquettesVides, @SupprimerArticlesQuantiteZero,
                         @Langue, @ImprimerFactureParDefaut, @ImprimerTicketParDefaut, @MethodePaiementParDefaut,
                         @VueParDefaut, @TrierParDefaut, @TailleIcones, @DateCreation, @DateModification);
                        SELECT CAST(SCOPE_IDENTITY() as int)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@AfficherClavier", "Manuel");
                        cmd.Parameters.AddWithValue("@MasquerEtiquettesVides", false);
                        cmd.Parameters.AddWithValue("@SupprimerArticlesQuantiteZero", false);
                        cmd.Parameters.AddWithValue("@Langue", "Français");
                        cmd.Parameters.AddWithValue("@ImprimerFactureParDefaut", false);
                        cmd.Parameters.AddWithValue("@ImprimerTicketParDefaut", false);
                        cmd.Parameters.AddWithValue("@MethodePaiementParDefaut", "Espèces");
                        cmd.Parameters.AddWithValue("@VueParDefaut", "Cartes");
                        cmd.Parameters.AddWithValue("@TrierParDefaut", "Nom (A-Z)");
                        cmd.Parameters.AddWithValue("@TailleIcones", "Moyennes");
                        cmd.Parameters.AddWithValue("@DateCreation", DateTime.Now);
                        cmd.Parameters.AddWithValue("@DateModification", DateTime.Now);

                        return (int)cmd.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de la création des paramètres : {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Met à jour les paramètres généraux
        /// </summary>
        public bool MettreAJourParametres(string connectionString)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"UPDATE ParametresGeneraux 
                        SET AfficherClavier = @AfficherClavier,
                            MasquerEtiquettesVides = @MasquerEtiquettesVides,
                            SupprimerArticlesQuantiteZero = @SupprimerArticlesQuantiteZero,
                            Langue = @Langue,
                            ImprimerFactureParDefaut = @ImprimerFactureParDefaut,
                            ImprimerTicketParDefaut = @ImprimerTicketParDefaut,
                            MethodePaiementParDefaut = @MethodePaiementParDefaut,
                            VueParDefaut = @VueParDefaut,
                            TrierParDefaut = @TrierParDefaut,
                            TailleIcones = @TailleIcones,
                            DateModification = @DateModification
                        WHERE UserId = @UserId";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", this.UserId);
                        cmd.Parameters.AddWithValue("@AfficherClavier", this.AfficherClavier);
                        cmd.Parameters.AddWithValue("@MasquerEtiquettesVides", this.MasquerEtiquettesVides);
                        cmd.Parameters.AddWithValue("@SupprimerArticlesQuantiteZero", this.SupprimerArticlesQuantiteZero);
                        cmd.Parameters.AddWithValue("@Langue", this.Langue);
                        cmd.Parameters.AddWithValue("@ImprimerFactureParDefaut", this.ImprimerFactureParDefaut);
                        cmd.Parameters.AddWithValue("@ImprimerTicketParDefaut", this.ImprimerTicketParDefaut);
                        cmd.Parameters.AddWithValue("@MethodePaiementParDefaut", this.MethodePaiementParDefaut);
                        cmd.Parameters.AddWithValue("@VueParDefaut", this.VueParDefaut);
                        cmd.Parameters.AddWithValue("@TrierParDefaut", this.TrierParDefaut);
                        cmd.Parameters.AddWithValue("@TailleIcones", this.TailleIcones);
                        cmd.Parameters.AddWithValue("@DateModification", DateTime.Now);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de la mise à jour des paramètres : {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtient ou crée les paramètres pour un utilisateur
        /// </summary>
        public static ParametresGeneraux ObtenirOuCreerParametres(int userId, string connectionString)
        {
            var parametres = ObtenirParametresParUserId(userId, connectionString);

            if (parametres == null)
            {
                // Créer des paramètres par défaut
                int newId = CreerParametresParDefaut(userId, connectionString);
                parametres = ObtenirParametresParUserId(userId, connectionString);
            }

            return parametres;
        }
    }
}