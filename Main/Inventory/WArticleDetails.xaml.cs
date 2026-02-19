using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace GestionComerce.Main.Inventory
{
    public partial class WArticleDetails : Window
    {
        public WArticleDetails(Article article, List<Famille> lf, List<Fournisseur> lfo)
        {
            InitializeComponent();

            // Basic Information
            ArticleID.Text = article.ArticleID.ToString();
            Code.Text = article.Code.ToString();
            ArticleName.Text = article.ArticleName ?? "N/A";

            // Famille
            string familleName = "N/A";
            foreach (Famille f in lf)
            {
                if (f.FamilleID == article.FamillyID)
                {
                    familleName = f.FamilleName;
                    break;
                }
            }
            Famille.Text = familleName;

            // Fournisseur
            string fournisseurName = "N/A";
            foreach (Fournisseur fo in lfo)
            {
                if (fo.FournisseurID == article.FournisseurID)
                {
                    fournisseurName = fo.Nom;
                    break;
                }
            }
            Fournisseur.Text = fournisseurName;

            // Prices
            PrixAchat.Text = article.PrixAchat.ToString("0.00") + " DH";
            PrixVente.Text = article.PrixVente.ToString("0.00") + " DH";
            PrixMP.Text = article.PrixMP.ToString("0.00") + " DH";

            // Stock
            Quantite.Text = article.Quantite.ToString();

            // New Fields
            Marque.Text = string.IsNullOrWhiteSpace(article.marque) ? "N/A" : article.marque;
            TVA.Text = article.tva.ToString("0.00") + " %";
            NumeroLot.Text = string.IsNullOrWhiteSpace(article.numeroLot) ? "N/A" : article.numeroLot;
            BonLivraison.Text = string.IsNullOrWhiteSpace(article.bonlivraison) ? "N/A" : article.bonlivraison;

            // Dates - Handle nullable DateTime
            if (article.Date.HasValue)
            {
                DateArticle.Text = article.Date.Value.ToString("dd/MM/yyyy");
            }
            else
            {
                DateArticle.Text = "N/A";
            }

            if (article.DateLivraison.HasValue)
            {
                DateLivraison.Text = article.DateLivraison.Value.ToString("dd/MM/yyyy");
            }
            else
            {
                DateLivraison.Text = "N/A";
            }

            // Date Expiration with color coding
            if (article.DateExpiration.HasValue)
            {
                DateExpiration.Text = article.DateExpiration.Value.ToString("dd/MM/yyyy");

                // Calculate time until expiration
                TimeSpan timeUntilExpiration = article.DateExpiration.Value - DateTime.Now;

                if (timeUntilExpiration.TotalDays < 0)
                {
                    // Expired - Red
                    DateExpiration.Foreground = new SolidColorBrush(Color.FromRgb(220, 38, 38));
                    DateExpiration.FontWeight = FontWeights.Bold;
                    DateExpiration.Text += " (EXPIRÉ)";
                }
                else if (timeUntilExpiration.TotalDays <= 30)
                {
                    // Expiring soon - Orange
                    DateExpiration.Foreground = new SolidColorBrush(Color.FromRgb(245, 158, 11));
                    DateExpiration.FontWeight = FontWeights.SemiBold;
                    int daysLeft = (int)Math.Ceiling(timeUntilExpiration.TotalDays);
                    DateExpiration.Text += $" (Expire dans {daysLeft} jours)";
                }
                else if (timeUntilExpiration.TotalDays <= 90)
                {
                    // Expiring in 3 months - Yellow
                    DateExpiration.Foreground = new SolidColorBrush(Color.FromRgb(234, 179, 8));
                    DateExpiration.FontWeight = FontWeights.Medium;
                }
                else
                {
                    // Good - Green
                    DateExpiration.Foreground = new SolidColorBrush(Color.FromRgb(5, 150, 105));
                }
            }
            else
            {
                DateExpiration.Text = "N/A";
                DateExpiration.Foreground = new SolidColorBrush(Color.FromRgb(107, 114, 128));
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}