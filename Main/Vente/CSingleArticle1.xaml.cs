using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace GestionComerce.Main.Vente
{
    public partial class CSingleArticle1 : UserControl
    {
        CMainV mainv;
        public Article a;
        List<Famille> lf;
        List<Fournisseur> lfo; private string iconSize;

        public CSingleArticle1(Article a, CMainV mainv, List<Famille> lf, List<Fournisseur> lfo, int s, string iconSize = "Moyennes")
        {
            InitializeComponent();

            this.a = a;
            this.mainv = mainv;
            this.lf = lf;
            this.lfo = lfo;
            this.iconSize = iconSize;

            // Load article image
            LoadArticleImage();

            // Set common data
            string familleName = "";
            foreach (Famille f in lf)
            {
                if (f.FamilleID == a.FamillyID)
                {
                    familleName = f.FamilleName;
                    break;
                }
            }

            string fournisseurName = "";
            foreach (Fournisseur fo in lfo)
            {
                if (a.FournisseurID == fo.FournisseurID)
                {
                    fournisseurName = fo.Nom;
                    break;
                }
            }

            if (s == 0) // Row layout (normal)
            {
                ApplyRowLayout();

                ArticleID.Text = a.ArticleID.ToString();
                ArticleName.Text = a.ArticleName;
                PrixVente.Text = a.PrixVente.ToString("F2") + " DH";
                Quantite.Text = a.Quantite.ToString();
                PrixAchat.Text = a.PrixAchat.ToString("F2") + " DH";
                PrixMP.Text = a.PrixMP.ToString("F2") + " DH";
                Famille.Text = familleName;
                FournisseurName.Text = fournisseurName;
                Code.Text = a.Code.ToString();
            }
            else if (s == 1) // Row layout (compact - for selected article preview)
            {
                ApplyRowLayout();

                ArticleID.Visibility = Visibility.Collapsed;
                ArticleIDC.Width = new GridLength(0);
                PrixAchat.Visibility = Visibility.Collapsed;
                PrixAchatC.Width = new GridLength(0);
                PrixVente.Visibility = Visibility.Collapsed;
                PrixVenteC.Width = new GridLength(0);
                PrixMP.Visibility = Visibility.Collapsed;
                PrixMPC.Width = new GridLength(0);
                Famille.Visibility = Visibility.Collapsed;
                FamilleC.Width = new GridLength(0);
                ImageColumnRow.Width = new GridLength(0);

                ArticleIDC.MinWidth = 0;
                PrixAchatC.MinWidth = 0;
                PrixVenteC.MinWidth = 0;
                PrixMPC.MinWidth = 0;
                FamilleC.MinWidth = 0;
                ImageColumnRow.MinWidth = 0;

                ArticleNameC.MinWidth = 30;
                QuantiteC.MinWidth = 30;
                FournisseurNameC.MinWidth = 30;
                CodeC.MinWidth = 30;

                ArticleNameC.Width = new GridLength(1, GridUnitType.Star);
                QuantiteC.Width = new GridLength(1, GridUnitType.Star);
                FournisseurNameC.Width = new GridLength(1, GridUnitType.Star);
                CodeC.Width = new GridLength(1, GridUnitType.Star);

                ArticleName.Text = a.ArticleName;
                Quantite.Text = a.Quantite.ToString();
                FournisseurName.Text = fournisseurName;
                Code.Text = a.Code.ToString();
            }
            else if (s == 2) // Card layout
            {
                ApplyCardLayout();

                CardArticleName.Text = a.ArticleName;
                CardPrixAchat.Text = a.PrixAchat.ToString("F2") + " DH";
                CardPrixVente.Text = a.PrixVente.ToString("F2") + " DH";
                CardFournisseur.Text = fournisseurName;
                CardFamille.Text = familleName;
                CardCode.Text = a.Code.ToString();
                CardQuantite.Text = a.Quantite.ToString();
            }
        }

        private void ApplyRowLayout()
        {
            this.Width = double.NaN;
            this.Height = 48;
            this.Margin = new Thickness(0, 0, 0, 0);

            RowLayout.Visibility = Visibility.Visible;
            CardLayout.Visibility = Visibility.Collapsed;
        }

        private void ApplyCardLayout()
        {
            // Définir les dimensions selon la taille
            int cardHeight = 0;
            int imageHeight = 0;
            int cardWidth = 0;

            switch (iconSize)
            {
                case "Grandes":
                    cardHeight = 320;
                    imageHeight = 160;
                    cardWidth = 300; // Set width for large icons
                    break;
                case "Moyennes":
                    cardHeight = 310;
                    imageHeight = 140;
                    cardWidth = 0; // Let grid handle it
                    break;
                case "Petites":
                    cardHeight = 180;
                    imageHeight = 100;
                    cardWidth = 0; // Let grid handle it
                    break;
                default:
                    cardHeight = 340;
                    imageHeight = 140;
                    cardWidth = 0;
                    break;
            }

            // Set width only for Grandes, otherwise let Grid handle it
            // Set width for Grandes, otherwise let Grid handle it
            if (iconSize == "Grandes")
            {
                this.Width = 590; // Fixed width for large icons
                this.HorizontalAlignment = HorizontalAlignment.Center;
            }
            else
            {
                this.Width = double.NaN; // Auto width for Moyennes and Petites
                this.HorizontalAlignment = HorizontalAlignment.Stretch;
            }
            this.Height = cardHeight;

            // Don't set Width - let Grid handle it with Star sizing
            this.Height = cardHeight;
            this.HorizontalAlignment = HorizontalAlignment.Stretch;

            // Mettre à jour la hauteur de la section image dans le Grid
            if (CardLayout != null && CardLayout.Child is Grid cardGrid)
            {
                if (cardGrid.RowDefinitions.Count > 0)
                {
                    cardGrid.RowDefinitions[0].Height = new GridLength(imageHeight);
                }
            }

            RowLayout.Visibility = Visibility.Collapsed;
            CardLayout.Visibility = Visibility.Visible;

            // Hide/show elements based on size
            UpdateCardVisibility();
        }

        private void UpdateCardVisibility()
        {
            if (CardLayout?.Child is Grid cardGrid && cardGrid.Children.Count > 1)
            {
                var infoPanel = cardGrid.Children[1] as StackPanel;
                if (infoPanel != null)
                {
                    // For small icons, show only image, name, and prices
                    if (iconSize == "Petites")
                    {
                        // Hide detailed information panels
                        foreach (UIElement child in infoPanel.Children)
                        {
                            if (child is StackPanel panel)
                            {
                                // Check the name of the panel
                                if (panel.Name == "FournisseurPanel" || panel.Name == "StockPanel")
                                {
                                    panel.Visibility = Visibility.Collapsed;
                                }
                                else
                                {
                                    panel.Visibility = Visibility.Visible;
                                }
                            }
                            else if (child is Grid grid)
                            {
                                // Hide famille/code grid for small icons
                                if (grid.Name == "FamilleCodeGrid")
                                {
                                    grid.Visibility = Visibility.Collapsed;
                                }
                                else if (grid.Name == "PricesGrid")
                                {
                                    grid.Visibility = Visibility.Visible;
                                }
                            }
                            else if (child is TextBlock)
                            {
                                // Keep article name visible
                                child.Visibility = Visibility.Visible;
                            }
                        }
                    }
                    else
                    {
                        // Show all information for medium and large icons
                        foreach (UIElement child in infoPanel.Children)
                        {
                            child.Visibility = Visibility.Visible;
                        }
                    }
                }
            }
        }

        private void LoadArticleImage()
        {
            try
            {
                if (a.ArticleImage != null && a.ArticleImage.Length > 0)
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = new MemoryStream(a.ArticleImage);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    RowArticleImage.Source = bitmap;
                    CardArticleImage.Source = bitmap;
                }
            }
            catch
            {
                // Image loading failed, leave empty
            }
        }

        private void ArticleClicked(object sender, MouseButtonEventArgs e)
        {
            if (a.Quantite <= 0)
            {
                MessageBox.Show(
                    $"L'article '{a.ArticleName}' n'a plus de stock disponible.\n\nQuantité en stock : 0",
                    "Stock Insuffisant",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            foreach (UIElement element in mainv.SelectedArticles.Children)
            {
                if (element is CSingleArticle2 item)
                {
                    if (item.a.ArticleID == a.ArticleID)
                    {
                        if (a.Quantite <= Convert.ToInt32(item.Quantite.Text))
                        {
                            MessageBox.Show("La quantité dans le panier est la même que celle que vous avez en stock");
                            return;
                        }
                        item.Quantite.Text = (Convert.ToInt32(item.Quantite.Text) + 1).ToString();
                        item.qte++;
                        mainv.TotalNett += a.PrixVente;
                        mainv.TotalNet.Text = mainv.TotalNett.ToString("F2") + " DH";
                        mainv.NbrA += 1;
                        mainv.ArticleCount.Text = mainv.NbrA.ToString();
                        return;
                    }
                }
            }

            mainv.TotalNett += a.PrixVente;
            mainv.TotalNet.Text = mainv.TotalNett.ToString("F2") + " DH";
            mainv.NbrA += 1;
            mainv.ArticleCount.Text = mainv.NbrA.ToString();
            CSingleArticle2 sa = new CSingleArticle2(a, 1, mainv);
            mainv.SelectedArticles.Children.Add(sa);
            mainv.UpdateCartEmptyState();
            mainv.SelectedArticle.Child = new CSingleArticle1(a, mainv, lf, lfo, 1);
        }
    }
}