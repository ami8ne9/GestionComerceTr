using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using GestionComerce.Models;

namespace GestionComerce
{
    public class ComptabiliteService
    {
        // =============================================
        // AUTOMATIC JOURNAL ENTRY GENERATION
        // =============================================

        /// <summary>
        /// Generates journal entry for a Sale (Operation)
        /// </summary>
        public int EnregistrerVente(int operationID, decimal totalAmount, int? paymentMethodID, int? clientID, DateTime? Date)
        {
            try
            {
                using (SqlConnection conn = DBHelper.GetConnection())
                {
                    conn.Open();
                    SqlTransaction transaction = conn.BeginTransaction();

                    try
                    {
                        string clientName = "Comptant";
                        if (clientID.HasValue && clientID.Value > 0)
                        {
                            string clientQuery = "SELECT ISNULL(NomComplet, Nom) FROM Clients WHERE ClientID = @ClientID";
                            using (SqlCommand cmd = new SqlCommand(clientQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@ClientID", clientID.Value);
                                object result = cmd.ExecuteScalar();
                                if (result != null)
                                    clientName = result.ToString();
                            }
                        }

                        string compteTresorerie = DeterminerCompteTresorerie(paymentMethodID);
                        string numPiece = string.Format("VE-{0:yyyyMMdd}-{1}", DateTime.Now, operationID);
                        DateTime dateEcriture = Date ?? DateTime.Now;

                        string insertJournal = @"
                            INSERT INTO JournalComptable (NumPiece, DateEcriture, Libelle, TypeOperation, RefExterne, EstValide, DateCreation)
                            VALUES (@NumPiece, @DateEcriture, @Libelle, @TypeOperation, @RefExterne, 0, GETDATE());
                            SELECT CAST(SCOPE_IDENTITY() AS INT);";

                        int journalID;
                        using (SqlCommand cmd = new SqlCommand(insertJournal, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@NumPiece", numPiece);
                            cmd.Parameters.AddWithValue("@DateEcriture", dateEcriture);
                            cmd.Parameters.AddWithValue("@Libelle", string.Format("Vente N° {0} - Client: {1}", operationID, clientName));
                            cmd.Parameters.AddWithValue("@TypeOperation", "Vente");
                            cmd.Parameters.AddWithValue("@RefExterne", string.Format("OP-{0}", operationID));
                            journalID = (int)cmd.ExecuteScalar();
                        }

                        decimal montantHT = totalAmount / 1.20m;
                        decimal montantTVA = totalAmount - montantHT;

                        string insertEcriture = @"
                            INSERT INTO EcrituresComptables (JournalID, CodeCompte, Libelle, Debit, Credit, DateEcriture)
                            VALUES (@JournalID, @CodeCompte, @Libelle, @Debit, @Credit, @DateEcriture)";

                        using (SqlCommand cmd = new SqlCommand(insertEcriture, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@JournalID", journalID);
                            cmd.Parameters.AddWithValue("@CodeCompte", compteTresorerie);
                            cmd.Parameters.AddWithValue("@Libelle", string.Format("Encaissement vente N° {0}", operationID));
                            cmd.Parameters.AddWithValue("@Debit", totalAmount);
                            cmd.Parameters.AddWithValue("@Credit", 0);
                            cmd.Parameters.AddWithValue("@DateEcriture", dateEcriture);
                            cmd.ExecuteNonQuery();
                        }

                        using (SqlCommand cmd = new SqlCommand(insertEcriture, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@JournalID", journalID);
                            cmd.Parameters.AddWithValue("@CodeCompte", "7111");
                            cmd.Parameters.AddWithValue("@Libelle", string.Format("Vente marchandises N° {0}", operationID));
                            cmd.Parameters.AddWithValue("@Debit", 0);
                            cmd.Parameters.AddWithValue("@Credit", montantHT);
                            cmd.Parameters.AddWithValue("@DateEcriture", dateEcriture);
                            cmd.ExecuteNonQuery();
                        }

                        using (SqlCommand cmd = new SqlCommand(insertEcriture, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@JournalID", journalID);
                            cmd.Parameters.AddWithValue("@CodeCompte", "4455");
                            cmd.Parameters.AddWithValue("@Libelle", string.Format("TVA facturée N° {0}", operationID));
                            cmd.Parameters.AddWithValue("@Debit", 0);
                            cmd.Parameters.AddWithValue("@Credit", montantTVA);
                            cmd.Parameters.AddWithValue("@DateEcriture", dateEcriture);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return journalID;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Erreur lors de l'enregistrement comptable de la vente: {0}", ex.Message), ex);
            }
        }

        /// <summary>
        /// Generates journal entry for Purchase
        /// </summary>
        public int EnregistrerAchat(int factureID, decimal totalAmount, string numeroFacture, int? fournisseurID, DateTime? dateFacture)
        {
            try
            {
                using (SqlConnection conn = DBHelper.GetConnection())
                {
                    conn.Open();
                    SqlTransaction transaction = conn.BeginTransaction();

                    try
                    {
                        string fournisseurName = "Fournisseur";
                        if (fournisseurID.HasValue && fournisseurID.Value > 0)
                        {
                            string supplierQuery = "SELECT ISNULL(NomComplet, Nom) FROM Fournisseurs WHERE FournisseurID = @FournisseurID";
                            using (SqlCommand cmd = new SqlCommand(supplierQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@FournisseurID", fournisseurID.Value);
                                object result = cmd.ExecuteScalar();
                                if (result != null)
                                    fournisseurName = result.ToString();
                            }
                        }

                        string numPiece = string.Format("AC-{0:yyyyMMdd}-{1}", DateTime.Now, factureID);
                        DateTime dateEcriture = dateFacture ?? DateTime.Now;

                        string insertJournal = @"
                            INSERT INTO JournalComptable (NumPiece, DateEcriture, Libelle, TypeOperation, RefExterne, EstValide, DateCreation)
                            VALUES (@NumPiece, @DateEcriture, @Libelle, @TypeOperation, @RefExterne, 0, GETDATE());
                            SELECT CAST(SCOPE_IDENTITY() AS INT);";

                        int journalID;
                        using (SqlCommand cmd = new SqlCommand(insertJournal, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@NumPiece", numPiece);
                            cmd.Parameters.AddWithValue("@DateEcriture", dateEcriture);
                            cmd.Parameters.AddWithValue("@Libelle", string.Format("Achat N° {0} - {1}", numeroFacture, fournisseurName));
                            cmd.Parameters.AddWithValue("@TypeOperation", "Achat");
                            cmd.Parameters.AddWithValue("@RefExterne", string.Format("FA-{0}", factureID));
                            journalID = (int)cmd.ExecuteScalar();
                        }

                        decimal montantHT = totalAmount / 1.20m;
                        decimal montantTVA = totalAmount - montantHT;

                        string insertEcriture = @"
                            INSERT INTO EcrituresComptables (JournalID, CodeCompte, Libelle, Debit, Credit, DateEcriture)
                            VALUES (@JournalID, @CodeCompte, @Libelle, @Debit, @Credit, @DateEcriture)";

                        using (SqlCommand cmd = new SqlCommand(insertEcriture, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@JournalID", journalID);
                            cmd.Parameters.AddWithValue("@CodeCompte", "6111");
                            cmd.Parameters.AddWithValue("@Libelle", string.Format("Achat marchandises {0}", numeroFacture));
                            cmd.Parameters.AddWithValue("@Debit", montantHT);
                            cmd.Parameters.AddWithValue("@Credit", 0);
                            cmd.Parameters.AddWithValue("@DateEcriture", dateEcriture);
                            cmd.ExecuteNonQuery();
                        }

                        using (SqlCommand cmd = new SqlCommand(insertEcriture, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@JournalID", journalID);
                            cmd.Parameters.AddWithValue("@CodeCompte", "3455");
                            cmd.Parameters.AddWithValue("@Libelle", string.Format("TVA récupérable {0}", numeroFacture));
                            cmd.Parameters.AddWithValue("@Debit", montantTVA);
                            cmd.Parameters.AddWithValue("@Credit", 0);
                            cmd.Parameters.AddWithValue("@DateEcriture", dateEcriture);
                            cmd.ExecuteNonQuery();
                        }

                        using (SqlCommand cmd = new SqlCommand(insertEcriture, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@JournalID", journalID);
                            cmd.Parameters.AddWithValue("@CodeCompte", "4411");
                            cmd.Parameters.AddWithValue("@Libelle", string.Format("Dette {0}", fournisseurName));
                            cmd.Parameters.AddWithValue("@Debit", 0);
                            cmd.Parameters.AddWithValue("@Credit", totalAmount);
                            cmd.Parameters.AddWithValue("@DateEcriture", dateEcriture);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return journalID;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Erreur achat: {0}", ex.Message), ex);
            }
        }

        /// <summary>
        /// Generates journal entry for Salary
        /// </summary>
        public int EnregistrerSalaire(int salaireID, int? employeID, decimal salaireBrut, decimal cotisationCNSS,
            decimal cotisationPatronaleCNSS, decimal cotisationAMO, decimal montantIR, decimal salaireNet, DateTime? datePaiement, DateTime mois)
        {
            try
            {
                using (SqlConnection conn = DBHelper.GetConnection())
                {
                    conn.Open();
                    SqlTransaction transaction = conn.BeginTransaction();

                    try
                    {
                        string employeName = "Employé";
                        if (employeID.HasValue && employeID.Value > 0)
                        {
                            string empQuery = "SELECT ISNULL(NomComplet, Nom) FROM Employes WHERE EmployeID = @EmployeID";
                            using (SqlCommand cmd = new SqlCommand(empQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@EmployeID", employeID.Value);
                                object result = cmd.ExecuteScalar();
                                if (result != null)
                                    employeName = result.ToString();
                            }
                        }

                        string numPiece = string.Format("SA-{0:yyyyMMdd}-{1}", DateTime.Now, salaireID);
                        DateTime dateEcriture = datePaiement ?? DateTime.Now;

                        string insertJournal = @"
                            INSERT INTO JournalComptable (NumPiece, DateEcriture, Libelle, TypeOperation, RefExterne, EstValide, DateCreation)
                            VALUES (@NumPiece, @DateEcriture, @Libelle, @TypeOperation, @RefExterne, 0, GETDATE());
                            SELECT CAST(SCOPE_IDENTITY() AS INT);";

                        int journalID;
                        using (SqlCommand cmd = new SqlCommand(insertJournal, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@NumPiece", numPiece);
                            cmd.Parameters.AddWithValue("@DateEcriture", dateEcriture);
                            cmd.Parameters.AddWithValue("@Libelle", string.Format("Salaire {0} - {1:MMMM yyyy}", employeName, mois));
                            cmd.Parameters.AddWithValue("@TypeOperation", "Salaire");
                            cmd.Parameters.AddWithValue("@RefExterne", string.Format("SAL-{0}", salaireID));
                            journalID = (int)cmd.ExecuteScalar();
                        }

                        string insertEcriture = @"
                            INSERT INTO EcrituresComptables (JournalID, CodeCompte, Libelle, Debit, Credit, DateEcriture)
                            VALUES (@JournalID, @CodeCompte, @Libelle, @Debit, @Credit, @DateEcriture)";

                        using (SqlCommand cmd = new SqlCommand(insertEcriture, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@JournalID", journalID);
                            cmd.Parameters.AddWithValue("@CodeCompte", "6171");
                            cmd.Parameters.AddWithValue("@Libelle", string.Format("Salaire brut {0}", employeName));
                            cmd.Parameters.AddWithValue("@Debit", salaireBrut);
                            cmd.Parameters.AddWithValue("@Credit", 0);
                            cmd.Parameters.AddWithValue("@DateEcriture", dateEcriture);
                            cmd.ExecuteNonQuery();
                        }

                        using (SqlCommand cmd = new SqlCommand(insertEcriture, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@JournalID", journalID);
                            cmd.Parameters.AddWithValue("@CodeCompte", "6181");
                            cmd.Parameters.AddWithValue("@Libelle", string.Format("Cotisation patronale {0}", employeName));
                            cmd.Parameters.AddWithValue("@Debit", cotisationPatronaleCNSS);
                            cmd.Parameters.AddWithValue("@Credit", 0);
                            cmd.Parameters.AddWithValue("@DateEcriture", dateEcriture);
                            cmd.ExecuteNonQuery();
                        }

                        decimal totalCNSS = cotisationCNSS + cotisationPatronaleCNSS;
                        using (SqlCommand cmd = new SqlCommand(insertEcriture, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@JournalID", journalID);
                            cmd.Parameters.AddWithValue("@CodeCompte", "4441");
                            cmd.Parameters.AddWithValue("@Libelle", string.Format("CNSS à payer {0}", employeName));
                            cmd.Parameters.AddWithValue("@Debit", 0);
                            cmd.Parameters.AddWithValue("@Credit", totalCNSS);
                            cmd.Parameters.AddWithValue("@DateEcriture", dateEcriture);
                            cmd.ExecuteNonQuery();
                        }

                        if (cotisationAMO > 0)
                        {
                            using (SqlCommand cmd = new SqlCommand(insertEcriture, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@JournalID", journalID);
                                cmd.Parameters.AddWithValue("@CodeCompte", "4445");
                                cmd.Parameters.AddWithValue("@Libelle", string.Format("AMO {0}", employeName));
                                cmd.Parameters.AddWithValue("@Debit", 0);
                                cmd.Parameters.AddWithValue("@Credit", cotisationAMO);
                                cmd.Parameters.AddWithValue("@DateEcriture", dateEcriture);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        if (montantIR > 0)
                        {
                            using (SqlCommand cmd = new SqlCommand(insertEcriture, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@JournalID", journalID);
                                cmd.Parameters.AddWithValue("@CodeCompte", "4452");
                                cmd.Parameters.AddWithValue("@Libelle", string.Format("IR {0}", employeName));
                                cmd.Parameters.AddWithValue("@Debit", 0);
                                cmd.Parameters.AddWithValue("@Credit", montantIR);
                                cmd.Parameters.AddWithValue("@DateEcriture", dateEcriture);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        using (SqlCommand cmd = new SqlCommand(insertEcriture, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@JournalID", journalID);
                            cmd.Parameters.AddWithValue("@CodeCompte", "5121");
                            cmd.Parameters.AddWithValue("@Libelle", string.Format("Salaire net payé {0}", employeName));
                            cmd.Parameters.AddWithValue("@Debit", 0);
                            cmd.Parameters.AddWithValue("@Credit", salaireNet);
                            cmd.Parameters.AddWithValue("@DateEcriture", dateEcriture);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return journalID;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Erreur salaire: {0}", ex.Message), ex);
            }
        }

        /// <summary>
        /// Generates journal entry for Expense
        /// </summary>
        public int EnregistrerDepense(int expenseID, decimal amount, string category, string beneficiaire, DateTime? dateExpense)
        {
            try
            {
                using (SqlConnection conn = DBHelper.GetConnection())
                {
                    conn.Open();
                    SqlTransaction transaction = conn.BeginTransaction();

                    try
                    {
                        string compteCharge = MapperCategorieDepenseAuCompte(category);
                        string numPiece = string.Format("DP-{0:yyyyMMdd}-{1}", DateTime.Now, expenseID);
                        DateTime dateEcriture = dateExpense ?? DateTime.Now;

                        string insertJournal = @"
                            INSERT INTO JournalComptable (NumPiece, DateEcriture, Libelle, TypeOperation, RefExterne, EstValide, DateCreation)
                            VALUES (@NumPiece, @DateEcriture, @Libelle, @TypeOperation, @RefExterne, 0, GETDATE());
                            SELECT CAST(SCOPE_IDENTITY() AS INT);";

                        int journalID;
                        using (SqlCommand cmd = new SqlCommand(insertJournal, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@NumPiece", numPiece);
                            cmd.Parameters.AddWithValue("@DateEcriture", dateEcriture);
                            cmd.Parameters.AddWithValue("@Libelle", string.Format("Dépense: {0} - {1}", beneficiaire ?? category, category));
                            cmd.Parameters.AddWithValue("@TypeOperation", "Depense");
                            cmd.Parameters.AddWithValue("@RefExterne", string.Format("EX-{0}", expenseID));
                            journalID = (int)cmd.ExecuteScalar();
                        }

                        string insertEcriture = @"
                            INSERT INTO EcrituresComptables (JournalID, CodeCompte, Libelle, Debit, Credit, DateEcriture)
                            VALUES (@JournalID, @CodeCompte, @Libelle, @Debit, @Credit, @DateEcriture)";

                        using (SqlCommand cmd = new SqlCommand(insertEcriture, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@JournalID", journalID);
                            cmd.Parameters.AddWithValue("@CodeCompte", compteCharge);
                            cmd.Parameters.AddWithValue("@Libelle", beneficiaire ?? category);
                            cmd.Parameters.AddWithValue("@Debit", amount);
                            cmd.Parameters.AddWithValue("@Credit", 0);
                            cmd.Parameters.AddWithValue("@DateEcriture", dateEcriture);
                            cmd.ExecuteNonQuery();
                        }

                        using (SqlCommand cmd = new SqlCommand(insertEcriture, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@JournalID", journalID);
                            cmd.Parameters.AddWithValue("@CodeCompte", "5161");
                            cmd.Parameters.AddWithValue("@Libelle", string.Format("Paiement {0}", category));
                            cmd.Parameters.AddWithValue("@Debit", 0);
                            cmd.Parameters.AddWithValue("@Credit", amount);
                            cmd.Parameters.AddWithValue("@DateEcriture", dateEcriture);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return journalID;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Erreur dépense: {0}", ex.Message), ex);
            }
        }

        // =============================================
        // FINANCIAL REPORTS
        // =============================================

        public DashboardFinancierDTO GenererDashboardFinancier(DateTime? dateDebut = null, DateTime? dateFin = null)
        {
            try
            {
                if (!dateDebut.HasValue)
                    dateDebut = new DateTime(DateTime.Now.Year, 1, 1);
                if (!dateFin.HasValue)
                    dateFin = DateTime.Now;

                var dashboard = new DashboardFinancierDTO();

                using (SqlConnection conn = DBHelper.GetConnection())
                {
                    conn.Open();

                    string queryVentes = @"
                        SELECT ISNULL(SUM(PrixOperation), 0)
                        FROM Operation
                        WHERE OperationType LIKE 'Vente%'
                        AND Etat = 1
                        AND Date BETWEEN @DateDebut AND @DateFin";

                    using (SqlCommand cmd = new SqlCommand(queryVentes, conn))
                    {
                        cmd.Parameters.AddWithValue("@DateDebut", dateDebut.Value);
                        cmd.Parameters.AddWithValue("@DateFin", dateFin.Value);
                        dashboard.TotalVentes = Convert.ToDecimal(cmd.ExecuteScalar());
                    }

                    string queryAchats = @"
                        SELECT ISNULL(SUM(TotalAmount), 0)
                        FROM SavedInvoices
                        WHERE InvoiceDate BETWEEN @DateDebut AND @DateFin";

                    using (SqlCommand cmd = new SqlCommand(queryAchats, conn))
                    {
                        cmd.Parameters.AddWithValue("@DateDebut", dateDebut.Value);
                        cmd.Parameters.AddWithValue("@DateFin", dateFin.Value);
                        dashboard.TotalAchats = Convert.ToDecimal(cmd.ExecuteScalar());
                    }

                    string querySalaires = @"
                        SELECT ISNULL(SUM(SalaireNet), 0)
                        FROM Salaires
                        WHERE Statut = 'Payé'
                        AND DatePaiement BETWEEN @DateDebut AND @DateFin";

                    using (SqlCommand cmd = new SqlCommand(querySalaires, conn))
                    {
                        cmd.Parameters.AddWithValue("@DateDebut", dateDebut.Value);
                        cmd.Parameters.AddWithValue("@DateFin", dateFin.Value);
                        dashboard.TotalSalaires = Convert.ToDecimal(cmd.ExecuteScalar());
                    }

                    string queryDepenses = @"
                        SELECT ISNULL(SUM(Amount), 0)
                        FROM Expenses
                        WHERE PaymentStatus = 'Paid'
                        AND LastPaidDate BETWEEN @DateDebut AND @DateFin";

                    using (SqlCommand cmd = new SqlCommand(queryDepenses, conn))
                    {
                        cmd.Parameters.AddWithValue("@DateDebut", dateDebut.Value);
                        cmd.Parameters.AddWithValue("@DateFin", dateFin.Value);
                        dashboard.TotalDepenses = Convert.ToDecimal(cmd.ExecuteScalar());
                    }

                    dashboard.BeneficeNet = dashboard.TotalVentes -
                        (dashboard.TotalAchats + dashboard.TotalSalaires + dashboard.TotalDepenses);

                    string queryBanque = @"
                        SELECT ISNULL(SUM(PrixOperation), 0)
                        FROM Operation
                        WHERE OperationType LIKE 'Vente%'
                        AND PaymentMethodID = 2
                        AND Etat = 1
                        AND Date BETWEEN @DateDebut AND @DateFin";

                    using (SqlCommand cmd = new SqlCommand(queryBanque, conn))
                    {
                        cmd.Parameters.AddWithValue("@DateDebut", dateDebut.Value);
                        cmd.Parameters.AddWithValue("@DateFin", dateFin.Value);
                        dashboard.TresorerieBanque = Convert.ToDecimal(cmd.ExecuteScalar());
                    }

                    string queryCaisse = @"
                        SELECT ISNULL(SUM(PrixOperation), 0)
                        FROM Operation
                        WHERE OperationType LIKE 'Vente%'
                        AND (PaymentMethodID = 1 OR PaymentMethodID IS NULL)
                        AND Etat = 1
                        AND Date BETWEEN @DateDebut AND @DateFin";

                    using (SqlCommand cmd = new SqlCommand(queryCaisse, conn))
                    {
                        cmd.Parameters.AddWithValue("@DateDebut", dateDebut.Value);
                        cmd.Parameters.AddWithValue("@DateFin", dateFin.Value);
                        dashboard.TresorerieCaisse = Convert.ToDecimal(cmd.ExecuteScalar());
                    }

                    dashboard.TresorerieTotale = dashboard.TresorerieBanque + dashboard.TresorerieCaisse;
                }

                return dashboard;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Erreur dashboard: {0}", ex.Message), ex);
            }
        }

        public List<EcrituresComptables> ObtenirJournalGeneral(DateTime? dateDebut = null, DateTime? dateFin = null, string typeOperation = null)
        {
            try
            {
                List<EcrituresComptables> ecritures = new List<EcrituresComptables>();

                using (SqlConnection conn = DBHelper.GetConnection())
                {
                    conn.Open();

                    string query = @"
                        SELECT 
                            ec.EcritureID, ec.JournalID, ec.CodeCompte, ec.Libelle, 
                            ec.Debit, ec.Credit, ec.DateEcriture,
                            pc.Libelle as LibelleCompte,
                            j.NumPiece, j.TypeOperation, j.EstValide
                        FROM EcrituresComptables ec
                        INNER JOIN JournalComptable j ON ec.JournalID = j.JournalID
                        INNER JOIN PlanComptable pc ON ec.CodeCompte = pc.CodeCompte
                        WHERE 1=1";

                    if (dateDebut.HasValue)
                        query += " AND j.DateEcriture >= @DateDebut";
                    if (dateFin.HasValue)
                        query += " AND j.DateEcriture <= @DateFin";
                    if (!string.IsNullOrEmpty(typeOperation))
                        query += " AND j.TypeOperation = @TypeOperation";

                    query += " ORDER BY j.DateEcriture DESC, ec.EcritureID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (dateDebut.HasValue)
                            cmd.Parameters.AddWithValue("@DateDebut", dateDebut.Value);
                        if (dateFin.HasValue)
                            cmd.Parameters.AddWithValue("@DateFin", dateFin.Value);
                        if (!string.IsNullOrEmpty(typeOperation))
                            cmd.Parameters.AddWithValue("@TypeOperation", typeOperation);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ecritures.Add(new EcrituresComptables
                                {
                                    EcritureID = reader.GetInt32(0),
                                    JournalID = reader.GetInt32(1),
                                    CodeCompte = reader.GetString(2),
                                    Libelle = reader.GetString(3),
                                    Debit = reader.GetDecimal(4),
                                    Credit = reader.GetDecimal(5),
                                    DateEcriture = reader.GetDateTime(6),
                                    LibelleCompte = reader.GetString(7)
                                });
                            }
                        }
                    }
                }

                return ecritures;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Erreur journal: {0}", ex.Message), ex);
            }
        }

        public bool ValiderJournal(int journalID, string validePar)
        {
            try
            {
                using (SqlConnection conn = DBHelper.GetConnection())
                {
                    conn.Open();

                    string balanceQuery = @"
                        SELECT SUM(Debit) - SUM(Credit) as Difference
                        FROM EcrituresComptables
                        WHERE JournalID = @JournalID";

                    using (SqlCommand cmd = new SqlCommand(balanceQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@JournalID", journalID);
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            decimal difference = Convert.ToDecimal(result);
                            if (Math.Abs(difference) > 0.01m)
                                throw new Exception(string.Format("Écriture non équilibrée: {0:N2}", difference));
                        }
                    }

                    string updateQuery = @"
                        UPDATE JournalComptable
                        SET EstValide = 1, DateValidation = GETDATE(), ValidePar = @ValidePar
                        WHERE JournalID = @JournalID";

                    using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@JournalID", journalID);
                        cmd.Parameters.AddWithValue("@ValidePar", validePar);
                        cmd.ExecuteNonQuery();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Erreur validation: {0}", ex.Message), ex);
            }
        }

        public BilanDTO GenererBilan(DateTime dateBilan)
        {
            try
            {
                var bilan = new BilanDTO
                {
                    DateBilan = dateBilan,
                    Actifs = new List<BilanLigneDTO>(),
                    Passifs = new List<BilanLigneDTO>()
                };

                using (SqlConnection conn = DBHelper.GetConnection())
                {
                    conn.Open();

                    string query = @"
                        SELECT 
                            pc.CodeCompte,
                            pc.Libelle,
                            pc.Classe,
                            pc.TypeCompte,
                            pc.SensNormal,
                            ISNULL(SUM(ec.Debit),  0) as TotalDebit,
                            ISNULL(SUM(ec.Credit), 0) as TotalCredit
                        FROM PlanComptable pc
                        LEFT JOIN EcrituresComptables ec ON pc.CodeCompte = ec.CodeCompte
                        LEFT JOIN JournalComptable j ON ec.JournalID = j.JournalID
                        WHERE pc.Classe IN (1,2,3,4,5)
                            AND (j.DateEcriture <= @DateBilan OR j.DateEcriture IS NULL)
                        GROUP BY pc.CodeCompte, pc.Libelle, pc.Classe, pc.TypeCompte, pc.SensNormal
                        HAVING (ISNULL(SUM(ec.Debit), 0) - ISNULL(SUM(ec.Credit), 0)) <> 0
                        ORDER BY pc.CodeCompte";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@DateBilan", dateBilan);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string codeCompte = reader.GetString(0);
                                string libelle = reader.GetString(1);
                                int classe = reader.GetInt32(2);
                                string typeCompte = reader.GetString(3);
                                string sensNormal = reader.GetString(4);
                                decimal totalDebit = reader.GetDecimal(5);
                                decimal totalCredit = reader.GetDecimal(6);

                                decimal solde = totalDebit - totalCredit;
                                if (sensNormal == "Credit")
                                    solde = -solde;

                                var ligne = new BilanLigneDTO
                                {
                                    CodeCompte = codeCompte,
                                    Libelle = libelle,
                                    Montant = Math.Abs(solde),
                                    Classe = classe
                                };

                                if (typeCompte == "Actif" || classe == 2 || classe == 3 || (classe == 5 && solde > 0))
                                {
                                    bilan.Actifs.Add(ligne);
                                    bilan.TotalActif += ligne.Montant;
                                }
                                else if (typeCompte == "Passif" || classe == 1 || classe == 4 || (classe == 5 && solde < 0))
                                {
                                    bilan.Passifs.Add(ligne);
                                    bilan.TotalPassif += ligne.Montant;
                                }
                            }
                        }
                    }

                    var cpc = GenererCPC(new DateTime(dateBilan.Year, 1, 1), dateBilan);
                    if (cpc.ResultatNet != 0)
                    {
                        if (cpc.ResultatNet > 0)
                        {
                            bilan.Passifs.Add(new BilanLigneDTO
                            {
                                CodeCompte = "1191",
                                Libelle = "Résultat Net (Bénéfice)",
                                Montant = cpc.ResultatNet,
                                Classe = 1
                            });
                            bilan.TotalPassif += cpc.ResultatNet;
                        }
                        else
                        {
                            bilan.Actifs.Add(new BilanLigneDTO
                            {
                                CodeCompte = "1199",
                                Libelle = "Résultat Net (Perte)",
                                Montant = Math.Abs(cpc.ResultatNet),
                                Classe = 1
                            });
                            bilan.TotalActif += Math.Abs(cpc.ResultatNet);
                        }
                    }
                }

                return bilan;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Erreur génération bilan: {0}", ex.Message), ex);
            }
        }

        public CPCDTO GenererCPC(DateTime dateDebut, DateTime dateFin)
        {
            try
            {
                var cpc = new CPCDTO
                {
                    DateDebut = dateDebut,
                    DateFin = dateFin,
                    Produits = new List<CPCLigneDTO>(),
                    Charges = new List<CPCLigneDTO>()
                };

                using (SqlConnection conn = DBHelper.GetConnection())
                {
                    conn.Open();

                    string query = @"
                        SELECT 
                            pc.CodeCompte,
                            pc.Libelle,
                            pc.Classe,
                            ISNULL(SUM(ec.Debit),  0) as TotalDebit,
                            ISNULL(SUM(ec.Credit), 0) as TotalCredit
                        FROM PlanComptable pc
                        LEFT JOIN EcrituresComptables ec ON pc.CodeCompte = ec.CodeCompte
                        LEFT JOIN JournalComptable j ON ec.JournalID = j.JournalID
                        WHERE pc.Classe IN (6, 7)
                            AND j.DateEcriture BETWEEN @DateDebut AND @DateFin
                        GROUP BY pc.CodeCompte, pc.Libelle, pc.Classe
                        HAVING (ISNULL(SUM(ec.Debit), 0) - ISNULL(SUM(ec.Credit), 0)) <> 0
                        ORDER BY pc.CodeCompte";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@DateDebut", dateDebut);
                        cmd.Parameters.AddWithValue("@DateFin", dateFin);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string codeCompte = reader.GetString(0);
                                string libelle = reader.GetString(1);
                                int classe = reader.GetInt32(2);
                                decimal totalDebit = reader.GetDecimal(3);
                                decimal totalCredit = reader.GetDecimal(4);

                                decimal montant = Math.Abs(totalDebit - totalCredit);

                                var ligne = new CPCLigneDTO
                                {
                                    CodeCompte = codeCompte,
                                    Libelle = libelle,
                                    Montant = montant,
                                    Classe = classe
                                };

                                if (classe == 6)
                                {
                                    cpc.Charges.Add(ligne);
                                    cpc.TotalCharges += montant;
                                }
                                else if (classe == 7)
                                {
                                    cpc.Produits.Add(ligne);
                                    cpc.TotalProduits += montant;
                                }
                            }
                        }
                    }
                }

                cpc.ResultatNet = cpc.TotalProduits - cpc.TotalCharges;
                return cpc;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Erreur génération CPC: {0}", ex.Message), ex);
            }
        }

        public List<EcrituresComptables> ObtenirGrandLivreParCompte(string codeCompte, DateTime? dateDebut = null, DateTime? dateFin = null)
        {
            try
            {
                List<EcrituresComptables> ecritures = new List<EcrituresComptables>();

                using (SqlConnection conn = DBHelper.GetConnection())
                {
                    conn.Open();

                    string query = @"
                        SELECT 
                            ec.EcritureID,
                            ec.JournalID,
                            ec.CodeCompte,
                            ec.Libelle,
                            ec.Debit,
                            ec.Credit,
                            ec.DateEcriture
                        FROM EcrituresComptables ec
                        INNER JOIN JournalComptable j ON ec.JournalID = j.JournalID
                        WHERE ec.CodeCompte = @CodeCompte";

                    if (dateDebut.HasValue)
                        query += " AND j.DateEcriture >= @DateDebut";
                    if (dateFin.HasValue)
                        query += " AND j.DateEcriture <= @DateFin";

                    query += " ORDER BY j.DateEcriture, ec.EcritureID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CodeCompte", codeCompte);
                        if (dateDebut.HasValue)
                            cmd.Parameters.AddWithValue("@DateDebut", dateDebut.Value);
                        if (dateFin.HasValue)
                            cmd.Parameters.AddWithValue("@DateFin", dateFin.Value);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ecritures.Add(new EcrituresComptables
                                {
                                    EcritureID = reader.GetInt32(0),
                                    JournalID = reader.GetInt32(1),
                                    CodeCompte = reader.GetString(2),
                                    Libelle = reader.GetString(3),
                                    Debit = reader.GetDecimal(4),
                                    Credit = reader.GetDecimal(5),
                                    DateEcriture = reader.GetDateTime(6)
                                });
                            }
                        }
                    }
                }

                return ecritures;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Erreur Grand Livre: {0}", ex.Message), ex);
            }
        }

        // =============================================
        // CPC DETAILED (for CPCView)
        // =============================================

        /// <summary>
        /// Returns a fully-detailed CPC breakdown split into Exploitation /
        /// Financier / Non-Courant for both Produits (Class 7) and Charges (Class 6).
        /// Also supplements from direct transactional tables so figures are always
        /// up-to-date regardless of whether journal entries have been posted.
        ///
        /// Moroccan PCGE classification:
        ///   Class 7  –  711x/712x/713x/714x/716x/718x/79x = Exploitation
        ///               73xx = Financiers
        ///               75xx = Non-Courants
        ///   Class 6  –  61xx/62xx = Exploitation
        ///               63xx = Financières
        ///               65xx/69xx = Non-Courantes
        /// </summary>
        public CPCDetailDTO GetCPCData(DateTime startDate, DateTime endDate)
        {
            var result = new CPCDetailDTO
            {
                DateDebut = startDate,
                DateFin = endDate
            };

            using (SqlConnection conn = DBHelper.GetConnection())
            {
                conn.Open();

                // ── STEP 1: Read all Class-6 and Class-7 lines from the posted journal ──
                const string journalQuery = @"
                    SELECT
                        ec.CodeCompte,
                        pc.Libelle,
                        pc.Classe,
                        ISNULL(SUM(ec.Debit),  0) AS TotalDebit,
                        ISNULL(SUM(ec.Credit), 0) AS TotalCredit
                    FROM  EcrituresComptables ec
                    INNER JOIN JournalComptable j  ON ec.JournalID  = j.JournalID
                    INNER JOIN PlanComptable    pc ON ec.CodeCompte = pc.CodeCompte
                    WHERE pc.Classe IN (6, 7)
                      AND j.DateEcriture BETWEEN @DateDebut AND @DateFin
                    GROUP BY ec.CodeCompte, pc.Libelle, pc.Classe
                    HAVING (ISNULL(SUM(ec.Debit), 0) + ISNULL(SUM(ec.Credit), 0)) <> 0
                    ORDER BY ec.CodeCompte";

                var rawLines = new List<(string code, string libelle, int classe, decimal debit, decimal credit)>();

                using (var cmd = new SqlCommand(journalQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@DateDebut", startDate);
                    cmd.Parameters.AddWithValue("@DateFin", endDate);

                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            rawLines.Add((
                                rdr.GetString(0),
                                rdr.GetString(1),
                                rdr.GetInt32(2),
                                rdr.GetDecimal(3),
                                rdr.GetDecimal(4)
                            ));
                        }
                    }
                }

                // ── STEP 2: Classify each account line into the correct section ──
                foreach (var (code, libelle, classe, debit, credit) in rawLines)
                {
                    // Normal balance: Charges (6) = Debit, Produits (7) = Credit
                    decimal montant = classe == 6
                        ? Math.Abs(debit - credit)
                        : Math.Abs(credit - debit);

                    if (montant == 0) continue;

                    var ligne = new CPCLigneDTO
                    {
                        CodeCompte = code,
                        Libelle = libelle,
                        Montant = montant,
                        Classe = classe
                    };

                    if (classe == 7) ClassifyProduit(result, code, ligne);
                    else ClassifyCharge(result, code, ligne);
                }

                // ── STEP 3: Supplement from direct transactional tables ──
                // This ensures the CPC reflects reality even when journal entries
                // are not yet formally validated/posted.
                SupplementFromDirectTables(conn, startDate, endDate, result);
            }

            // ── STEP 4: Compute section totals ──
            result.TotalProduitsExploitation = SumLignes(result.ProduitsExploitation);
            result.TotalProduitsFinanciers = SumLignes(result.ProduitsFinanciers);
            result.TotalProduitsNonCourants = SumLignes(result.ProduitsNonCourants);

            result.TotalChargesExploitation = SumLignes(result.ChargesExploitation);
            result.TotalChargesFinancieres = SumLignes(result.ChargesFinancieres);
            result.TotalChargesNonCourantes = SumLignes(result.ChargesNonCourantes);

            return result;
        }

        // ─────────────────────────────────────────────────────────────
        //  ACCOUNT CLASSIFICATION HELPERS
        // ─────────────────────────────────────────────────────────────

        private static void ClassifyProduit(CPCDetailDTO dto, string code, CPCLigneDTO ligne)
        {
            if (code.StartsWith("73"))
                dto.ProduitsFinanciers.Add(ligne);
            else if (code.StartsWith("75"))
                dto.ProduitsNonCourants.Add(ligne);
            else
                dto.ProduitsExploitation.Add(ligne);
        }

        private static void ClassifyCharge(CPCDetailDTO dto, string code, CPCLigneDTO ligne)
        {
            if (code.StartsWith("63"))
                dto.ChargesFinancieres.Add(ligne);
            else if (code.StartsWith("65") || code.StartsWith("69"))
                dto.ChargesNonCourantes.Add(ligne);
            else
                dto.ChargesExploitation.Add(ligne);
        }

        // ─────────────────────────────────────────────────────────────
        //  DIRECT TABLE SUPPLEMENT
        //  Reads real transactional data and merges it with the journal.
        //  Uses Math.Max to avoid double-counting when both sources exist.
        // ─────────────────────────────────────────────────────────────

        private void SupplementFromDirectTables(
            SqlConnection conn,
            DateTime startDate,
            DateTime endDate,
            CPCDetailDTO dto)
        {
            void UpsertLigne(List<CPCLigneDTO> list, string code, string libelle, decimal montant, int classe)
            {
                if (montant <= 0) return;
                var existing = list.Find(l => l.CodeCompte == code);
                if (existing != null)
                    existing.Montant = Math.Max(existing.Montant, montant);
                else
                    list.Add(new CPCLigneDTO { CodeCompte = code, Libelle = libelle, Montant = montant, Classe = classe });
            }

            // Ventes de marchandises HT → 7111
            decimal ventes = 0;
            using (var cmd = new SqlCommand(@"
                SELECT ISNULL(SUM(PrixOperation), 0) / 1.20
                FROM   Operation
                WHERE  OperationType LIKE 'Vente%'
                  AND  Etat = 1
                  AND  Date BETWEEN @D1 AND @D2", conn))
            {
                cmd.Parameters.AddWithValue("@D1", startDate);
                cmd.Parameters.AddWithValue("@D2", endDate);
                ventes = Convert.ToDecimal(cmd.ExecuteScalar());
            }
            UpsertLigne(dto.ProduitsExploitation, "7111", "Ventes de marchandises", ventes, 7);

            // Achats revendus HT → 6111
            decimal achats = 0;
            using (var cmd = new SqlCommand(@"
                SELECT ISNULL(SUM(TotalAmount), 0) / 1.20
                FROM   SavedInvoices
                WHERE  InvoiceDate BETWEEN @D1 AND @D2", conn))
            {
                cmd.Parameters.AddWithValue("@D1", startDate);
                cmd.Parameters.AddWithValue("@D2", endDate);
                achats = Convert.ToDecimal(cmd.ExecuteScalar());
            }
            UpsertLigne(dto.ChargesExploitation, "6111", "Achats revendus de marchandises", achats, 6);

            // Salaires bruts → 6171
            decimal salaires = 0;
            using (var cmd = new SqlCommand(@"
                SELECT ISNULL(SUM(SalaireBrut), 0)
                FROM   Salaires
                WHERE  Statut = 'Payé'
                  AND  DatePaiement BETWEEN @D1 AND @D2", conn))
            {
                cmd.Parameters.AddWithValue("@D1", startDate);
                cmd.Parameters.AddWithValue("@D2", endDate);
                salaires = Convert.ToDecimal(cmd.ExecuteScalar());
            }
            UpsertLigne(dto.ChargesExploitation, "6171", "Rémunérations du personnel", salaires, 6);

            // Cotisations patronales CNSS → 6181
            decimal cnssPatronal = 0;
            using (var cmd = new SqlCommand(@"
                SELECT ISNULL(SUM(CotisationPatronaleCNSS), 0)
                FROM   Salaires
                WHERE  Statut = 'Payé'
                  AND  DatePaiement BETWEEN @D1 AND @D2", conn))
            {
                cmd.Parameters.AddWithValue("@D1", startDate);
                cmd.Parameters.AddWithValue("@D2", endDate);
                cnssPatronal = Convert.ToDecimal(cmd.ExecuteScalar());
            }
            UpsertLigne(dto.ChargesExploitation, "6181", "Cotisations patronales CNSS / AMO", cnssPatronal, 6);

            // Dépenses grouped by category → mapped accounts
            const string expQuery = @"
                SELECT Category, ISNULL(SUM(Amount), 0) AS Total
                FROM   Expenses
                WHERE  PaymentStatus = 'Paid'
                  AND  LastPaidDate BETWEEN @D1 AND @D2
                GROUP  BY Category";

            using (var cmd = new SqlCommand(expQuery, conn))
            {
                cmd.Parameters.AddWithValue("@D1", startDate);
                cmd.Parameters.AddWithValue("@D2", endDate);

                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        string category = rdr.IsDBNull(0) ? "Autres" : rdr.GetString(0);
                        decimal total = rdr.GetDecimal(1);
                        string compte = MapperCategorieDepenseAuCompte(category);
                        string libelle = MapCategoryToLabel(category);

                        // Frais bancaires (63xx) → Financières, everything else → Exploitation
                        if (compte.StartsWith("63") || category.ToLower().Contains("bancaire") || category.ToLower().Contains("bank"))
                            UpsertLigne(dto.ChargesFinancieres, compte, libelle, total, 6);
                        else
                            UpsertLigne(dto.ChargesExploitation, compte, libelle, total, 6);
                    }
                }
            }
        }

        // =============================================
        // UTILITY METHODS
        // =============================================

        private decimal ObtenirSoldeCompte(string codeCompte, DateTime? dateDebut, DateTime? dateFin, SqlConnection conn)
        {
            string query = @"
                SELECT ISNULL(ABS(SUM(ec.Debit) - SUM(ec.Credit)), 0)
                FROM EcrituresComptables ec
                INNER JOIN JournalComptable j ON ec.JournalID = j.JournalID
                WHERE ec.CodeCompte = @CodeCompte 
                    AND j.DateEcriture BETWEEN @DateDebut AND @DateFin";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@CodeCompte", codeCompte);
                cmd.Parameters.AddWithValue("@DateDebut", dateDebut);
                cmd.Parameters.AddWithValue("@DateFin", dateFin);
                object result = cmd.ExecuteScalar();
                return result != null && result != DBNull.Value ? Convert.ToDecimal(result) : 0;
            }
        }

        private decimal ObtenirSoldeClasse(int classe, DateTime? dateDebut, DateTime? dateFin, SqlConnection conn)
        {
            string query = @"
                SELECT ISNULL(ABS(SUM(ec.Debit) - SUM(ec.Credit)), 0)
                FROM EcrituresComptables ec
                INNER JOIN JournalComptable j ON ec.JournalID = j.JournalID
                INNER JOIN PlanComptable pc ON ec.CodeCompte = pc.CodeCompte
                WHERE pc.Classe = @Classe 
                    AND j.DateEcriture BETWEEN @DateDebut AND @DateFin";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Classe", classe);
                cmd.Parameters.AddWithValue("@DateDebut", dateDebut);
                cmd.Parameters.AddWithValue("@DateFin", dateFin);
                object result = cmd.ExecuteScalar();
                return result != null && result != DBNull.Value ? Convert.ToDecimal(result) : 0;
            }
        }

        private string DeterminerCompteTresorerie(int? paymentMethodID)
        {
            if (!paymentMethodID.HasValue) return "5161";

            switch (paymentMethodID.Value)
            {
                case 1: return "5161"; // Cash / Caisse
                case 2: return "5121"; // Bank / Banque
                case 3: return "5121"; // Chèque → Bank
                default: return "5161";
            }
        }

        private string MapperCategorieDepenseAuCompte(string category)
        {
            if (string.IsNullOrEmpty(category)) return "6125";

            string lower = category.ToLower();

            if (lower.Contains("loyer") || lower.Contains("rent")) return "61211";
            if (lower.Contains("électricité") || lower.Contains("electricity")) return "6185";
            if (lower.Contains("eau") || lower.Contains("water")) return "6185";
            if (lower.Contains("téléphone") || lower.Contains("telephone")) return "6156";
            if (lower.Contains("internet")) return "6156";
            if (lower.Contains("transport")) return "6161";
            if (lower.Contains("carburant") || lower.Contains("fuel")) return "6161";
            if (lower.Contains("fourniture")) return "6182";
            if (lower.Contains("assurance")) return "6134";
            if (lower.Contains("honoraire")) return "6136";
            if (lower.Contains("publicité") || lower.Contains("advertising")) return "6145";
            if (lower.Contains("entretien") || lower.Contains("maintenance")) return "6133";
            if (lower.Contains("bancaire") || lower.Contains("bank")) return "6167";

            return "6125";
        }

        private static string MapCategoryToLabel(string category)
        {
            if (string.IsNullOrEmpty(category)) return "Autres charges";

            string lower = category.ToLower();

            if (lower.Contains("loyer") || lower.Contains("rent")) return "Loyer et charges locatives";
            if (lower.Contains("électricité") || lower.Contains("electricity")) return "Électricité";
            if (lower.Contains("eau") || lower.Contains("water")) return "Eau";
            if (lower.Contains("téléphone") || lower.Contains("telephone")) return "Téléphone";
            if (lower.Contains("internet")) return "Internet";
            if (lower.Contains("transport")) return "Frais de transport";
            if (lower.Contains("carburant") || lower.Contains("fuel")) return "Carburant";
            if (lower.Contains("fourniture")) return "Fournitures de bureau";
            if (lower.Contains("assurance")) return "Primes d'assurance";
            if (lower.Contains("honoraire")) return "Honoraires";
            if (lower.Contains("publicité") || lower.Contains("advertising")) return "Publicité et communication";
            if (lower.Contains("entretien") || lower.Contains("maintenance")) return "Entretien et réparations";
            if (lower.Contains("bancaire") || lower.Contains("bank")) return "Frais bancaires";

            return category;
        }

        private static decimal SumLignes(List<CPCLigneDTO> list)
        {
            decimal total = 0;
            foreach (var l in list) total += l.Montant;
            return total;
        }
    }
}