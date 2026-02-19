using System;
using System.Collections.Generic;

namespace GestionComerce
{
    // =============================================
    // ACCOUNTING MODELS
    // =============================================

    public class PlanComptable
    {
        public string CodeCompte { get; set; }
        public string Libelle { get; set; }
        public int Classe { get; set; }
        public string TypeCompte { get; set; }
        public string SensNormal { get; set; }
        public bool EstActif { get; set; }
        public DateTime DateCreation { get; set; }
    }

    public class JournalComptable
    {
        public int JournalID { get; set; }
        public string NumPiece { get; set; }
        public DateTime DateEcriture { get; set; }
        public string Libelle { get; set; }
        public string TypeOperation { get; set; }
        public string RefExterne { get; set; }
        public bool EstValide { get; set; }
        public DateTime? DateValidation { get; set; }
        public string ValidePar { get; set; }
        public string Remarques { get; set; }
        public DateTime DateCreation { get; set; }
        public List<EcrituresComptables> Ecritures { get; set; }

        public JournalComptable()
        {
            Ecritures = new List<EcrituresComptables>();
        }
    }

    public class EcrituresComptables
    {
        public int EcritureID { get; set; }
        public int JournalID { get; set; }
        public string CodeCompte { get; set; }
        public string Libelle { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public DateTime DateEcriture { get; set; }
        public string LibelleCompte { get; set; } // For display purposes
    }

    public class ExerciceComptable
    {
        public int ExerciceID { get; set; }
        public int Annee { get; set; }
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public bool EstCloture { get; set; }
        public DateTime? DateCloture { get; set; }
    }

    // =============================================
    // DTOs FOR FINANCIAL REPORTS
    // =============================================

    public class BilanDTO
    {
        public DateTime DateBilan { get; set; }
        public List<BilanLigneDTO> Actifs { get; set; }
        public List<BilanLigneDTO> Passifs { get; set; }
        public decimal TotalActif { get; set; }
        public decimal TotalPassif { get; set; }

        public BilanDTO()
        {
            Actifs = new List<BilanLigneDTO>();
            Passifs = new List<BilanLigneDTO>();
        }
    }

    public class BilanLigneDTO
    {
        public string CodeCompte { get; set; }
        public string Libelle { get; set; }
        public decimal Montant { get; set; }
        public int Classe { get; set; }
    }

    public class CPCDTO
    {
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public List<CPCLigneDTO> Produits { get; set; }
        public List<CPCLigneDTO> Charges { get; set; }
        public decimal TotalProduits { get; set; }
        public decimal TotalCharges { get; set; }
        public decimal ResultatNet { get; set; }

        public CPCDTO()
        {
            Produits = new List<CPCLigneDTO>();
            Charges = new List<CPCLigneDTO>();
        }
    }

    public class CPCLigneDTO
    {
        public string CodeCompte { get; set; }
        public string Libelle { get; set; }
        public decimal Montant { get; set; }
        public int Classe { get; set; }
    }

    public class DashboardFinancierDTO
    {
        public decimal TotalVentes { get; set; }
        public decimal TotalAchats { get; set; }
        public decimal TotalSalaires { get; set; }
        public decimal TotalDepenses { get; set; }
        public decimal BeneficeNet { get; set; }
        public decimal TresorerieCaisse { get; set; }
        public decimal TresorerieBanque { get; set; }
        public decimal TresorerieTotale { get; set; }
    }
}